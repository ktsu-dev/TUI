// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Core.Services;

using ktsu.TUI.Core.Contracts;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

/// <summary>
/// Main UI application service that orchestrates the TUI
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UIApplication"/> class
/// </remarks>
/// <param name="consoleProvider">The console provider to use</param>
/// <param name="logger">Optional logger</param>
public class UIApplication(IConsoleProvider consoleProvider, ILogger<UIApplication>? logger = null) : IUIApplication
{
	private readonly ILogger<UIApplication>? _logger = logger;
	private CancellationTokenSource? _cancellationTokenSource;

	/// <summary>
	/// Held while <see cref="Shutdown"/> cancels the run's token source and while the run's teardown
	/// takes it, so a source is never disposed under a cancel still using it
	/// </summary>
	private readonly Lock _runCancellationGate = new();

	/// <summary>
	/// Gets the token source linked to the caller's token for the run in progress, or null
	/// between runs
	/// </summary>
	/// <remarks>
	/// A linked source registers a callback on the caller's token, so it is disposed and cleared
	/// when its run ends. Left behind, every run would leave another registration rooted on a
	/// long-lived host token (ktsu-dev/TUI#199). Tests read it to check that.
	/// </remarks>
	internal CancellationTokenSource? RunCancellationSource => Volatile.Read(ref _cancellationTokenSource);

	// Logger message delegates for improved performance
	private static readonly Action<ILogger, Exception?> LogApplicationAlreadyRunning =
		LoggerMessage.Define(LogLevel.Warning, new EventId(1, nameof(LogApplicationAlreadyRunning)), "Application is already running");

	private static readonly Action<ILogger, Exception?> LogStartingApplication =
		LoggerMessage.Define(LogLevel.Information, new EventId(2, nameof(LogStartingApplication)), "Starting UI application");

	private static readonly Action<ILogger, Exception?> LogApplicationCancelled =
		LoggerMessage.Define(LogLevel.Information, new EventId(3, nameof(LogApplicationCancelled)), "UI application was cancelled");

	private static readonly Action<ILogger, Exception?> LogApplicationError =
		LoggerMessage.Define(LogLevel.Error, new EventId(4, nameof(LogApplicationError)), "An error occurred while running the UI application");

	private static readonly Action<ILogger, Exception?> LogApplicationStopped =
		LoggerMessage.Define(LogLevel.Information, new EventId(5, nameof(LogApplicationStopped)), "UI application stopped");

	private static readonly Action<ILogger, Exception?> LogStoppingApplication =
		LoggerMessage.Define(LogLevel.Information, new EventId(6, nameof(LogStoppingApplication)), "Stopping UI application");

	private static readonly Action<ILogger, Exception?> LogNoRootElement =
		LoggerMessage.Define(LogLevel.Debug, new EventId(7, nameof(LogNoRootElement)), "No root element to render");

	private static readonly Action<ILogger, Exception?> LogRenderingUI =
		LoggerMessage.Define(LogLevel.Debug, new EventId(8, nameof(LogRenderingUI)), "Rendering UI");

	private static readonly Action<ILogger, Exception?> LogRenderError =
		LoggerMessage.Define(LogLevel.Error, new EventId(9, nameof(LogRenderError)), "An error occurred while rendering the UI");

	private static readonly Action<ILogger, Exception?> LogStartingInputProcessing =
		LoggerMessage.Define(LogLevel.Debug, new EventId(10, nameof(LogStartingInputProcessing)), "Starting input processing");

	private static readonly Action<ILogger, string, Exception?> LogReceivedInput =
		LoggerMessage.Define<string>(LogLevel.Debug, new EventId(11, nameof(LogReceivedInput)), "Received input: {InputType}");

	private static readonly Action<ILogger, Exception?> LogExitInputReceived =
		LoggerMessage.Define(LogLevel.Information, new EventId(12, nameof(LogExitInputReceived)), "Exit input received");

	private static readonly Action<ILogger, Exception?> LogInputNotHandled =
		LoggerMessage.Define(LogLevel.Debug, new EventId(13, nameof(LogInputNotHandled)), "Input was not handled by any element");

	private static readonly Action<ILogger, Exception?> LogInputProcessingError =
		LoggerMessage.Define(LogLevel.Error, new EventId(14, nameof(LogInputProcessingError)), "An error occurred while processing input");

	private static readonly Action<ILogger, Exception?> LogInputProcessingStopped =
		LoggerMessage.Define(LogLevel.Debug, new EventId(15, nameof(LogInputProcessingStopped)), "Input processing stopped");

	private static readonly Action<ILogger, string, Exception?> LogUIApplicationSetup =
		LoggerMessage.Define<string>(LogLevel.Information, new EventId(16, nameof(LogUIApplicationSetup)), "UI application setup with root element of type {ElementType}");

	private static readonly Action<ILogger, Exception?> LogInterruptReceived =
		LoggerMessage.Define(LogLevel.Information, new EventId(17, nameof(LogInterruptReceived)), "Interrupt signal received, shutting down");

	private static readonly Action<ILogger, int, int, Exception?> LogConsoleResized =
		LoggerMessage.Define<int, int>(LogLevel.Debug, new EventId(18, nameof(LogConsoleResized)), "Console resized to {Width}x{Height}, re-arranging the layout");

	/// <summary>
	/// Gets the source of process interrupt signals that shuts the application down
	/// </summary>
	/// <remarks>
	/// Defaults to the real console and process signals. Tests substitute a source they can raise.
	/// </remarks>
	internal IInterruptSource InterruptSource { get; init; } = new ConsoleInterruptSource();

	/// <summary>
	/// Gets how often the input loop wakes to re-check the terminal size while it is waiting for
	/// a key
	/// </summary>
	/// <remarks>
	/// A resize delivers no input, so a loop that only wakes on a keypress cannot notice one.
	/// Reading <see cref="IConsoleProvider.Dimensions"/> is cheap and nothing is drawn unless the
	/// size actually changed, so this is a size comparison ten times a second rather than a
	/// redraw. Tests shorten it so a resize is picked up without waiting out a real frame.
	/// </remarks>
	internal TimeSpan ResizePollInterval { get; init; } = TimeSpan.FromMilliseconds(100);

	/// <summary>
	/// How many reads in a row may fail before the input loop gives up and ends the run
	/// </summary>
	/// <remarks>
	/// An occasional failed read is tolerated, but a read that can never succeed, such as
	/// <c>Console.ReadKey</c> with stdin redirected, would otherwise be retried in a tight loop
	/// forever at full CPU (ktsu-dev/TUI#143).
	/// </remarks>
	internal const int MaxConsecutiveReadFailures = 10;

	/// <summary>
	/// A read still in progress when the last run ended, because its provider ignored the
	/// cancellation. The next run waits on it rather than starting a second read to race it.
	/// </summary>
	private Task<Models.InputResult>? _pendingRead;

	/// <summary>
	/// How long the end of a run waits for its cancelled read to finish, so a provider that gives
	/// reads up has done so by the time the run returns
	/// </summary>
	private static readonly TimeSpan PendingReadCancellationTimeout = TimeSpan.FromMilliseconds(250);

	/// <summary>
	/// The terminal size the last render pass saw, or null before the first render
	/// </summary>
	private Models.Dimensions? _observedConsoleDimensions;

	/// <summary>
	/// The terminal size each root element was last laid out for
	/// </summary>
	/// <remarks>
	/// Kept per root rather than once for the application, because a host can swap
	/// <see cref="RootElement"/> between screens. A screen detached while the terminal was resized
	/// was never told, and comparing against the size the current root saw would miss that it is
	/// stale (ktsu-dev/TUI#162). Weak, so a screen the host lets go of is not kept alive here.
	/// </remarks>
	private readonly ConditionalWeakTable<IUIElement, object> _rootLaidOutAt = [];

	/// <summary>
	/// 1 when an element changed since the last render pass began, and a pass should draw it even
	/// though no key was pressed; 0 otherwise. An int so it can be set from any thread with
	/// <see cref="Interlocked"/>.
	/// </summary>
	private int _redrawRequested;

	/// <summary>
	/// The managed thread ID running <see cref="Render"/>, or 0 when no pass is in progress
	/// </summary>
	private int _renderingThreadId;

	/// <summary>
	/// Changes posted from other threads through <see cref="Post"/>, run at the start of the next pass
	/// </summary>
	private readonly ConcurrentQueue<Action> _posted = new();

	/// <inheritdoc />
	/// <remarks>
	/// The application listens to the root's <see cref="IUIElement.Invalidated"/>, which every
	/// element bubbles up to it, so a change made outside a keypress (a timer, a background task)
	/// is drawn on the next poll of the input loop (ktsu-dev/TUI#154). Adding or removing elements
	/// from such a thread must go through <see cref="Post"/>, because the loop walks the tree on
	/// every pass (ktsu-dev/TUI#225).
	/// </remarks>
	public IUIElement? RootElement
	{
		get;
		set
		{
			if (ReferenceEquals(field, value))
			{
				return;
			}

			field?.Invalidated -= OnRootInvalidated;

			field = value;
			field?.Invalidated += OnRootInvalidated;

			// Clearing the root needs a pass too, to take the old root's frame off the screen
			// (ktsu-dev/TUI#223)
			RequestRender();
		}
	}

	/// <inheritdoc />
	public IConsoleProvider ConsoleProvider { get; } = Ensure.NotNull(consoleProvider);

	/// <inheritdoc />
	public bool IsRunning { get; private set; }

	/// <inheritdoc />
	public async Task RunAsync(CancellationToken cancellationToken = default)
	{
		if (IsRunning)
		{
			if (_logger != null)
			{
				LogApplicationAlreadyRunning(_logger, null);
			}
			return;
		}

		IsRunning = true;
		_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		CancellationToken runToken = _cancellationTokenSource.Token;
		IDisposable? interruptRegistration = null;

		try
		{
			if (_logger != null)
			{
				LogStartingApplication(_logger, null);
			}

			// Take Ctrl+C and SIGTERM for the duration of the run. Both otherwise end the process
			// outright, skipping the finally below that puts the cursor back.
			interruptRegistration = InterruptSource.Register(OnInterrupt);

			// Initialize console
			ConsoleProvider.Clear();
			ConsoleProvider.SetCursorVisibility(false);

			// Initial render
			Render();

			// Start input processing
			await ProcessInputAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (runToken.IsCancellationRequested)
		{
			if (_logger != null)
			{
				LogApplicationCancelled(_logger, null);
			}
		}
		catch (Exception ex)
		{
			if (_logger != null)
			{
				LogApplicationError(_logger, ex);
			}
			throw;
		}
		finally
		{
			// Stop taking signals before restoring the terminal, so a second Ctrl+C arriving
			// during teardown gets the runtime's default behaviour rather than a second shutdown.
			interruptRegistration?.Dispose();

			// The linked source holds a registration on the caller's token until it is disposed,
			// so a host that runs the UI more than once with a long-lived token would otherwise
			// collect one per run (ktsu-dev/TUI#199).
			CancellationTokenSource? finishedRunCancellation;
			lock (_runCancellationGate)
			{
				finishedRunCancellation = _cancellationTokenSource;
				_cancellationTokenSource = null;
			}

			finishedRunCancellation?.Dispose();
			IsRunning = false;
			ConsoleProvider.SetCursorVisibility(true);
			if (_logger != null)
			{
				LogApplicationStopped(_logger, null);
			}
		}
	}

	/// <summary>
	/// Handles an interrupt signal by shutting the application down through its normal path
	/// </summary>
	private void OnInterrupt()
	{
		if (_logger != null)
		{
			LogInterruptReceived(_logger, null);
		}

		Shutdown();
	}

	/// <summary>
	/// Asks for a render pass without drawing from the calling thread
	/// </summary>
	/// <remarks>
	/// Safe to call from any thread. The pass runs on the input loop's thread within one
	/// <see cref="ResizePollInterval"/>, so it never races with a pass already drawing. Elements
	/// that change through <see cref="IUIElement.Invalidate"/> request one for themselves; this is
	/// for a host that changes what is shown some other way (ktsu-dev/TUI#154). It does not make a
	/// change to the tree's structure safe: use <see cref="Post"/> for that (ktsu-dev/TUI#225).
	/// </remarks>
	public void RequestRender() => Interlocked.Exchange(ref _redrawRequested, 1);

	/// <summary>
	/// Runs <paramref name="action"/> on the input loop's thread, before the next render pass
	/// </summary>
	/// <remarks>
	/// Safe to call from any thread. The element tree is not synchronised: the loop thread walks
	/// every container's children on each pass, so adding or removing a child from a timer or a
	/// background task races that walk, drops frames and can fault <see cref="RunAsync"/>
	/// (ktsu-dev/TUI#225). Make such changes through this method instead. Setting a property an
	/// element only reads while drawing, such as <c>TextElement.Text</c>, is safe to do directly.
	/// <para>
	/// Posted actions run in the order they were posted, followed by a render pass, within one
	/// <see cref="ResizePollInterval"/> while the application runs. When it is not running they run
	/// at the start of the next <see cref="Render"/>. An exception an action throws propagates as
	/// one thrown while handling input would.
	/// </para>
	/// </remarks>
	/// <param name="action">The change to make</param>
	public void Post(Action action)
	{
		_posted.Enqueue(Ensure.NotNull(action));
		RequestRender();
	}

	/// <summary>
	/// Runs every action posted through <see cref="Post"/> so far, on the calling thread
	/// </summary>
	private void RunPostedActions()
	{
		while (_posted.TryDequeue(out Action? action))
		{
			action();
		}
	}

	/// <summary>
	/// Requests a render pass when anything in the tree changes
	/// </summary>
	/// <remarks>
	/// Only flags the request: the event can fire on any thread, and drawing belongs to the input
	/// loop. Invalidations a render pass raises on its own thread, such as the arrange assigning
	/// sizes, are ignored, or every pass would request the next one.
	/// </remarks>
	/// <param name="sender">The element that changed</param>
	/// <param name="e">Unused</param>
	private void OnRootInvalidated(object? sender, EventArgs e)
	{
		if (Volatile.Read(ref _renderingThreadId) != Environment.CurrentManagedThreadId)
		{
			RequestRender();
		}
	}

	/// <inheritdoc />
	public void Shutdown()
	{
		if (_logger != null)
		{
			LogStoppingApplication(_logger, null);
		}

		// The run's teardown clears and disposes the source; the gate keeps it from doing so
		// between reading the field and cancelling it.
		lock (_runCancellationGate)
		{
			_cancellationTokenSource?.Cancel();
		}
	}

	/// <inheritdoc />
	/// <remarks>
	/// Each pass clears the console and redraws every visible element. Dirty tracking is not used
	/// to skip elements — combining a full clear with a dirty-only redraw is what made static
	/// elements disappear on the frame after their first draw (ktsu-dev/TUI#109).
	/// <para>
	/// Each pass also re-checks the terminal size, so a window the user resized mid-run is laid
	/// out at its new size on the next frame rather than staying pinned to the size at launch
	/// (ktsu-dev/TUI#111).
	/// </para>
	/// </remarks>
	public void Render()
	{
		// Before anything reads the tree, so the pass draws the changes and nothing changes it
		// while it is walked. An action may also be what sets the root.
		RunPostedActions();

		if (RootElement == null)
		{
			if (_logger != null)
			{
				LogNoRootElement(_logger, null);
			}

			// Nothing to draw, but whatever an earlier root drew must not stay on screen looking
			// live while it no longer receives input (ktsu-dev/TUI#223)
			Interlocked.Exchange(ref _redrawRequested, 0);
			ConsoleProvider.Clear();
			return;
		}

		// This pass draws whatever was requested before it began. A change made on another thread
		// from here on requests the next one.
		Interlocked.Exchange(ref _redrawRequested, 0);
		Volatile.Write(ref _renderingThreadId, Environment.CurrentManagedThreadId);

		try
		{
			if (_logger != null)
			{
				LogRenderingUI(_logger, null);
			}

			// Clear the console, then redraw the whole tree below. The two halves belong
			// together: a clear without a full redraw erases whatever the last pass drew.
			ConsoleProvider.Clear();

			// Size the layout to the terminal before drawing it, so a resize since the last pass
			// is reflected in this one.
			SyncRootToConsole(RootElement);

			// Render the root element
			RootElement.Render(ConsoleProvider);
		}
		catch (InvalidOperationException ex)
		{
			if (_logger != null)
			{
				LogRenderError(_logger, ex);
			}
		}
		catch (ArgumentException ex)
		{
			if (_logger != null)
			{
				LogRenderError(_logger, ex);
			}
		}
		catch (NotSupportedException ex)
		{
			if (_logger != null)
			{
				LogRenderError(_logger, ex);
			}
		}
		finally
		{
			Volatile.Write(ref _renderingThreadId, 0);
		}
	}

	/// <summary>
	/// Brings the root element's size in line with the terminal, then re-arranges the tree
	/// </summary>
	/// <param name="root">The root element to size</param>
	/// <remarks>
	/// The size used to be taken once, on the first pass that found the root unsized, and never
	/// looked at again — so resizing the window left every layout pinned to the size at launch
	/// (ktsu-dev/TUI#111).
	/// </remarks>
	private void SyncRootToConsole(IUIElement root)
	{
		Models.Dimensions console = ConsoleProvider.Dimensions;
		_observedConsoleDimensions = console;

		// Resized from this root's point of view: the terminal is not the size it was when this
		// root was last laid out. That includes a resize that happened while it was swapped out.
		bool resized = _rootLaidOutAt.TryGetValue(root, out object? laidOutAt) && (Models.Dimensions)laidOutAt != console;
		_rootLaidOutAt.AddOrUpdate(root, console);

		// Adopt the terminal size when the root has none of its own, and again whenever the
		// terminal is resized. In between, a size the host assigned to the root is left alone — a
		// resize is the one thing that overrides it, since the old size no longer fits the window.
		if (resized || root.Dimensions.IsEmpty)
		{
			if (resized && _logger != null)
			{
				LogConsoleResized(_logger, console.Width, console.Height, null);
			}

			root.Dimensions = console;
		}

		// Arrange on every pass, not only when the size changed. Showing an element, changing
		// text, or a size the host assigned all change the layout without a resize, and skipping
		// the arrange left them drawn with stale geometry (ktsu-dev/TUI#133). The pass already
		// clears and redraws everything, so an arrange is cheap beside it. Assigning Dimensions
		// only invalidates, so the tree is walked to re-arrange every container, not just the root.
		ArrangeTree(root);
	}

	/// <summary>
	/// Re-arranges <paramref name="element"/> and every container beneath it, parents first
	/// </summary>
	/// <param name="element">The element to arrange</param>
	/// <remarks>
	/// A container's <see cref="IUIContainer.ArrangeChildren"/> sizes and positions its own
	/// children but does not reach theirs, so arranging only the root would relayout the top level
	/// and leave everything under it at the old size. Parents are arranged first because a child
	/// container can only lay its own children out once it knows its new size.
	/// </remarks>
	private static void ArrangeTree(IUIElement element)
	{
		if (element is not IUIContainer container)
		{
			return;
		}

		container.ArrangeChildren();

		foreach (IUIElement child in container.Children)
		{
			ArrangeTree(child);
		}
	}

	/// <summary>
	/// Gets whether the terminal has changed size since the last render pass
	/// </summary>
	/// <returns>True when a redraw is needed to pick the new size up</returns>
	private bool HasConsoleResized() =>
		RootElement != null && ConsoleProvider.Dimensions != _observedConsoleDimensions;

	/// <inheritdoc />
	public async Task ProcessInputAsync(CancellationToken cancellationToken = default)
	{
		if (_logger != null)
		{
			LogStartingInputProcessing(_logger, null);
		}

		// One read is carried across iterations. The resize poll below wakes the loop without a
		// keypress, and starting a fresh read each time it woke would leave several reads racing
		// for the next key. It is carried across runs too: a provider that cannot give a read up
		// is still waiting on it, and a second read would race it for every key (ktsu-dev/TUI#149).
		Task<Models.InputResult>? pendingRead = _pendingRead;
		_pendingRead = null;
		int consecutiveReadFailures = 0;

		// Cancelled when the loop ends, however it ends, so a provider can give its read up instead
		// of leaving it behind to take the next key meant for the host (ktsu-dev/TUI#149).
		using CancellationTokenSource readCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

		try
		{
			while (!cancellationToken.IsCancellationRequested && IsRunning)
			{
				// Tells the catch blocks below whether the failure came from reading or from handling
				bool readSucceeded = false;

				try
				{
					pendingRead ??= ConsoleProvider.ReadInputAsync(readCancellation.Token);

					if (!pendingRead.IsCompleted)
					{
						// Wake on a timer as well as on input. A resize produces no input at all, so a
						// loop that only wakes for a key cannot notice one. Cancelling the delay is
						// also what lets a shutdown request end a run blocked on the keyboard: a
						// provider parked in Console.ReadKey does not observe the token itself.
						Task idle = Task.Delay(ResizePollInterval, cancellationToken);
						await Task.WhenAny(pendingRead, idle).ConfigureAwait(false);

						if (cancellationToken.IsCancellationRequested)
						{
							break;
						}

						if (!pendingRead.IsCompleted)
						{
							// The timer won the race, so no key arrived. Redraw only if the terminal
							// changed size or an element changed while we waited, and go back to the
							// same pending read.
							if (Volatile.Read(ref _redrawRequested) == 1 || HasConsoleResized())
							{
								Render();
							}

							continue;
						}
					}

					// Cleared before the await so a read that failed is not retried forever by the
					// recoverable-error branches below.
					Task<Models.InputResult> completedRead = pendingRead;
					pendingRead = null;

					if (completedRead.IsCanceled && !cancellationToken.IsCancellationRequested)
					{
						// A read carried over from an earlier run, which its provider gave up when
						// that run ended. It holds no key, so start a fresh one.
						continue;
					}

					Models.InputResult input = await completedRead.ConfigureAwait(false);
					readSucceeded = true;
					consecutiveReadFailures = 0;

					if (_logger != null)
					{
						LogReceivedInput(_logger, input.Type.ToString(), null);
					}

					// Handle global exit conditions
					if (input.IsExit)
					{
						if (_logger != null)
						{
							LogExitInputReceived(_logger, null);
						}
						Shutdown();
						break;
					}

					// Let the root element handle the input
					bool handled = RootElement?.HandleInput(input) ?? false;

					if (!handled && _logger != null)
					{
						LogInputNotHandled(_logger, null);
					}

					// Re-render if needed (elements invalidate themselves when they change)
					Render();
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
					break;
				}
				catch (OperationCanceledException ex)
				{
					// Thrown by a handler or the provider for a reason of its own (an HttpClient
					// timeout, say) rather than because this run was cancelled, so it is a failure
					// like the others here and not a shutdown (ktsu-dev/TUI#216)
					if (RecordInputFailure(ex, readSucceeded, ref consecutiveReadFailures))
					{
						// Rethrowing it as is would end RunAsync as a cancelled task
						throw new InvalidOperationException("Reading input kept failing with an OperationCanceledException that did not come from cancelling the run.", ex);
					}
				}
				catch (InvalidOperationException ex)
				{
					if (RecordInputFailure(ex, readSucceeded, ref consecutiveReadFailures))
					{
						throw;
					}

					// Continue processing for recoverable errors
				}
				catch (ArgumentException ex)
				{
					if (RecordInputFailure(ex, readSucceeded, ref consecutiveReadFailures))
					{
						throw;
					}

					// Continue processing for recoverable errors
				}
				catch (OutOfMemoryException)
				{
					// Critical error - rethrow
					throw;
				}
				catch (StackOverflowException)
				{
					// Critical error - rethrow
					throw;
				}
			}
		}
		finally
		{
			await readCancellation.CancelAsync().ConfigureAwait(false);
			_pendingRead = await AwaitEndOfReadAsync(pendingRead).ConfigureAwait(false);
		}

		if (_logger != null)
		{
			LogInputProcessingStopped(_logger, null);
		}
	}

	/// <summary>
	/// Logs a recoverable failure from reading or handling input and counts consecutive failed reads
	/// </summary>
	/// <param name="ex">The failure</param>
	/// <param name="readSucceeded">Whether the read succeeded, so the failure came from handling it</param>
	/// <param name="consecutiveReadFailures">The count of reads that have failed in a row</param>
	/// <returns>True when reads have failed too many times in a row and the run should give up</returns>
	private bool RecordInputFailure(Exception ex, bool readSucceeded, ref int consecutiveReadFailures)
	{
		if (_logger != null)
		{
			LogInputProcessingError(_logger, ex);
		}

		return !readSucceeded && ++consecutiveReadFailures >= MaxConsecutiveReadFailures;
	}

	/// <summary>
	/// Gives a read the loop has just cancelled a moment to finish
	/// </summary>
	/// <param name="pendingRead">The read the loop was waiting on, if any</param>
	/// <returns>The read if it is still in progress, for the next run to wait on; otherwise null</returns>
	/// <remarks>
	/// A read that completed with a key in the race against the shutdown is dropped, as a key
	/// pressed while the application was closing would be.
	/// </remarks>
	private static async Task<Task<Models.InputResult>?> AwaitEndOfReadAsync(Task<Models.InputResult>? pendingRead)
	{
		if (pendingRead is null || pendingRead.IsCompleted)
		{
			return null;
		}

		await Task.WhenAny(pendingRead, Task.Delay(PendingReadCancellationTimeout)).ConfigureAwait(false);
		return pendingRead.IsCompleted ? null : pendingRead;
	}

	/// <summary>
	/// Sets up the application with the specified root element
	/// </summary>
	/// <param name="rootElement">The root element to display</param>
	public void Setup(IUIElement rootElement)
	{
		Ensure.NotNull(rootElement);

		RootElement = rootElement;
		if (_logger != null)
		{
			LogUIApplicationSetup(_logger, rootElement.GetType().Name, null);
		}
	}

	/// <summary>
	/// Creates a builder for configuring the UI application
	/// </summary>
	/// <param name="consoleProvider">The console provider to use</param>
	/// <returns>A new application builder</returns>
	public static UIApplicationBuilder CreateBuilder(IConsoleProvider? consoleProvider = null) => new(consoleProvider);
}
