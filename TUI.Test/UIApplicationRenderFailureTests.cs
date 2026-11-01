// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Elements;
using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for what <see cref="UIApplication"/> does when an element throws while it is drawn
/// (ktsu-dev/TUI#246).
/// </summary>
/// <remarks>
/// A pass clears the console before it draws, so a pass that swallowed an element's exception
/// left every element after the one that threw erased on every frame, with nothing raised and,
/// without a logger, nothing logged. These tests assert that the failure reaches the caller.
/// </remarks>
[TestClass]
public sealed class UIApplicationRenderFailureTests
{
	/// <summary>
	/// How long a run is given to end before it is declared stuck.
	/// </summary>
	private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(10);

	/// <summary>
	/// A short poll so a requested redraw is drawn promptly.
	/// </summary>
	private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(20);

	/// <summary>
	/// Gets or sets the test context MSTest injects.
	/// </summary>
	public TestContext TestContext { get; set; } = null!;

	/// <summary>
	/// The repro from the issue: an element whose <c>OnRender</c> throws
	/// <see cref="ArgumentOutOfRangeException"/> must not leave <see cref="UIApplication.Render"/>
	/// returning normally with a truncated frame.
	/// </summary>
	[TestMethod]
	public void RenderSurfacesAnElementsArgumentOutOfRangeException()
	{
		RecordingConsoleProvider provider = new();
		StackPanel root = [];
		root.AddChild(new TextElement("before"));
		root.AddChild(new ThrowingElement { Throwing = true });
		root.AddChild(new TextElement("after"));
		UIApplication app = new(provider) { RootElement = root };

		Assert.ThrowsExactly<ArgumentOutOfRangeException>(app.Render);
	}

	/// <summary>
	/// Each recoverable exception type the pass used to catch now reaches the caller.
	/// </summary>
	/// <param name="kind">Which exception the element throws.</param>
	[TestMethod]
	[DataRow(nameof(InvalidOperationException))]
	[DataRow(nameof(ArgumentException))]
	[DataRow(nameof(NotSupportedException))]
	public void RenderSurfacesEveryExceptionAnElementThrows(string kind)
	{
		Exception failure = kind switch
		{
			nameof(InvalidOperationException) => new InvalidOperationException("Broken element."),
			nameof(ArgumentException) => new ArgumentException("Broken element."),
			_ => new NotSupportedException("Broken element."),
		};

		StackPanel root = [];
		root.AddChild(new ThrowingElement { Throwing = true, Failure = failure });
		UIApplication app = new(new RecordingConsoleProvider()) { RootElement = root };

		Exception thrown = Assert.Throws<Exception>(app.Render);
		Assert.AreSame(failure, thrown);
	}

	/// <summary>
	/// An element that throws on the first pass ends the run with its exception and restores the
	/// cursor, rather than leaving a run that draws a truncated frame forever.
	/// </summary>
	/// <returns>A task that completes when the test does.</returns>
	[TestMethod]
	public async Task AnElementThatThrowsOnTheFirstPassEndsTheRun()
	{
		BlockingConsoleProvider provider = new();
		StackPanel root = [];
		root.AddChild(new ThrowingElement { Throwing = true });
		UIApplication app = new(provider) { RootElement = root, ResizePollInterval = PollInterval, InterruptSource = new FakeInterruptSource() };

		Task run = await RunUntilEndAsync(app).ConfigureAwait(false);

		await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() => run).ConfigureAwait(false);
		Assert.IsTrue(provider.CursorVisible, "The cursor must be restored when a render failure ends the run");
	}

	/// <summary>
	/// A pass the poll tick draws (after an element invalidated, or a host asked for a redraw) must
	/// not have its failure swallowed by the input loop as a failed read.
	/// </summary>
	/// <returns>A task that completes when the test does.</returns>
	[TestMethod]
	public async Task AnElementThatStartsThrowingOnARequestedRedrawEndsTheRun()
	{
		BlockingConsoleProvider provider = new();
		ThrowingElement element = new();
		StackPanel root = [];
		root.AddChild(element);
		UIApplication app = new(provider) { RootElement = root, ResizePollInterval = PollInterval, InterruptSource = new FakeInterruptSource() };

		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
		Task run = Task.Run(() => app.RunAsync(timeout.Token), TestContext.CancellationToken);
		await provider.ReadStarted.WaitAsync(StepTimeout, TestContext.CancellationToken).ConfigureAwait(false);

		element.Throwing = true;
		app.RequestRender();

		Task finished = await Task.WhenAny(run, Task.Delay(StepTimeout, TestContext.CancellationToken)).ConfigureAwait(false);
		await timeout.CancelAsync().ConfigureAwait(false);

		Assert.AreSame(run, finished, "A redraw whose element throws should end the run");
		await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() => run).ConfigureAwait(false);
		Assert.IsTrue(provider.CursorVisible, "The cursor must be restored when a render failure ends the run");
	}

	/// <summary>
	/// The pass drawn after a key is handled must not have its failure swallowed by the input loop
	/// as a failure to handle the key.
	/// </summary>
	/// <returns>A task that completes when the test does.</returns>
	[TestMethod]
	public async Task AnElementThatStartsThrowingAfterAKeyEndsTheRun()
	{
		BlockingConsoleProvider provider = new();
		ThrowingElement element = new();
		StackPanel root = [];
		root.AddChild(element);
		UIApplication app = new(provider) { RootElement = root, ResizePollInterval = TimeSpan.FromHours(1), InterruptSource = new FakeInterruptSource() };

		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
		Task run = Task.Run(() => app.RunAsync(timeout.Token), TestContext.CancellationToken);
		await provider.ReadStarted.WaitAsync(StepTimeout, TestContext.CancellationToken).ConfigureAwait(false);

		element.Throwing = true;
		provider.Release(InputResult.FromKey(ConsoleKey.A));

		Task finished = await Task.WhenAny(run, Task.Delay(StepTimeout, TestContext.CancellationToken)).ConfigureAwait(false);
		await timeout.CancelAsync().ConfigureAwait(false);

		Assert.AreSame(run, finished, "A pass whose element throws after a key should end the run");
		await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() => run).ConfigureAwait(false);
	}

	/// <summary>
	/// Starts a run and waits for it to end, cancelling it if it outlives <see cref="StepTimeout"/>
	/// so a regression fails the test instead of hanging it.
	/// </summary>
	/// <param name="app">The application to run.</param>
	/// <returns>The run, which has ended.</returns>
	private async Task<Task> RunUntilEndAsync(UIApplication app)
	{
		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
		Task run = Task.Run(() => app.RunAsync(timeout.Token), TestContext.CancellationToken);
		Task finished = await Task.WhenAny(run, Task.Delay(StepTimeout, TestContext.CancellationToken)).ConfigureAwait(false);
		await timeout.CancelAsync().ConfigureAwait(false);

		Assert.AreSame(run, finished, "A render failure should end the run");
		return run;
	}

	/// <summary>
	/// An element with a rendering bug, which can be switched on after the first pass.
	/// </summary>
	private sealed class ThrowingElement : UIElementBase
	{
		/// <summary>
		/// Gets or sets whether the next pass throws. Volatile because a test sets it while the
		/// run's thread draws.
		/// </summary>
		public bool Throwing
		{
			get => Volatile.Read(ref field);
			set => Volatile.Write(ref field, value);
		}

		/// <summary>
		/// Gets the exception to throw, or null for the issue's out-of-range <c>Substring</c>.
		/// </summary>
		public Exception? Failure { get; init; }

		/// <inheritdoc />
		protected override void OnRender(IConsoleProvider provider)
		{
			if (!Throwing)
			{
				provider.WriteAt("fine", Position);
				return;
			}

			if (Failure is not null)
			{
				throw Failure;
			}

			provider.WriteAt("abc".Substring(1, 5), Position);
		}

		/// <inheritdoc />
		protected override Dimensions OnCalculateRequiredDimensions() => new(5, 1);
	}
}
