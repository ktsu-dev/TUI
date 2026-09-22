// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Core.Services;

using ktsu.TUI.Core.Contracts;
using Microsoft.Extensions.Logging;

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
	/// The terminal size the current layout was computed for, or null before the first render
	/// </summary>
	private Models.Dimensions? _observedConsoleDimensions;

	/// <inheritdoc />
	public IUIElement? RootElement { get; set; }

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
		catch (OperationCanceledException)
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

	/// <inheritdoc />
	public void Shutdown()
	{
		if (_logger != null)
		{
			LogStoppingApplication(_logger, null);
		}
		_cancellationTokenSource?.Cancel();
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
		if (RootElement == null)
		{
			if (_logger != null)
			{
				LogNoRootElement(_logger, null);
			}
			return;
		}

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
	}

	/// <summary>
	/// Brings the root element's size in line with the terminal, re-arranging the tree when it changes
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
		bool resized = _observedConsoleDimensions is Models.Dimensions observed && observed != console;
		_observedConsoleDimensions = console;

		// Adopt the terminal size when the root has none of its own, and again whenever the
		// terminal is resized. In between, a size the host assigned to the root is left alone — a
		// resize is the one thing that overrides it, since the old size no longer fits the window.
		if (!resized && !root.Dimensions.IsEmpty)
		{
			return;
		}

		if (resized && _logger != null)
		{
			LogConsoleResized(_logger, console.Width, console.Height, null);
		}

		root.Dimensions = console;

		// Assigning Dimensions only invalidates; it does not re-run layout. Walk the tree so every
		// container re-arranges inside its new size, not just the root.
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
		// for the next key.
		Task<Models.InputResult>? pendingRead = null;

		while (!cancellationToken.IsCancellationRequested && IsRunning)
		{
			try
			{
				pendingRead ??= ConsoleProvider.ReadInputAsync();

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
						// changed size while we waited, and go back to the same pending read.
						if (HasConsoleResized())
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
				Models.InputResult input = await completedRead.ConfigureAwait(false);

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

				if (!handled)
				{
					if (_logger != null)
					{
						LogInputNotHandled(_logger, null);
					}
				}

				// Re-render if needed (elements invalidate themselves when they change)
				Render();
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (InvalidOperationException ex)
			{
				if (_logger != null)
				{
					LogInputProcessingError(_logger, ex);
				}

				// Continue processing for recoverable errors
			}
			catch (ArgumentException ex)
			{
				if (_logger != null)
				{
					LogInputProcessingError(_logger, ex);
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

		if (_logger != null)
		{
			LogInputProcessingStopped(_logger, null);
		}
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
