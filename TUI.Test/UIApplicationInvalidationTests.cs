// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests that a change made outside a keypress is drawn while <see cref="UIApplication"/> waits
/// for input.
/// </summary>
/// <remarks>
/// The application used to render only at startup, after a key and after a resize, and nothing
/// listened to <see cref="Core.Contracts.IUIElement.Invalidated"/>, so a clock, a progress value
/// or a status set when a background task finished stayed frozen until a key was pressed
/// (ktsu-dev/TUI#154).
/// </remarks>
[TestClass]
public sealed class UIApplicationInvalidationTests
{
	/// <summary>
	/// How long any single step of a run is given before it is declared stuck.
	/// </summary>
	private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(10);

	/// <summary>
	/// How often the input loop wakes in these tests.
	/// </summary>
	private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(10);

	/// <summary>
	/// Gets or sets the test context MSTest injects, used for its cancellation token so a run
	/// started by a test ends when the test run itself is cancelled.
	/// </summary>
	public TestContext TestContext { get; set; } = null!;

	/// <summary>
	/// Text changed from another thread while the application waits for a key must be drawn
	/// without a key being pressed.
	/// </summary>
	[TestMethod]
	public async Task TextChangedOffTheLoopThreadIsDrawnWithoutAKeypress()
	{
		// Arrange
		BlockingConsoleProvider provider = new();
		TextElement clock = new("00:00:00");
		StackPanel root = [clock];
		UIApplication app = new(provider) { RootElement = root, ResizePollInterval = PollInterval };

		Task run = app.RunAsync(TestContext.CancellationToken);
		await AssertCompletesAsync(provider.ReadStarted, "The application should start waiting for input").ConfigureAwait(false);
		int passesBefore = provider.ClearCount;

		// Act
		await Task.Run(() => clock.Text = "00:00:01", TestContext.CancellationToken).ConfigureAwait(false);

		// Assert
		await AssertRenderPassAfterAsync(provider, passesBefore, "Changing the text should produce a render pass with no key pressed").ConfigureAwait(false);

		app.Shutdown();
		await AssertCompletesAsync(run, "The run should still end when asked to shut down").ConfigureAwait(false);
	}

	/// <summary>
	/// A host that changes what is shown without going through an element can ask for a pass.
	/// </summary>
	[TestMethod]
	public async Task RequestRenderDrawsWithoutAKeypress()
	{
		// Arrange
		BlockingConsoleProvider provider = new();
		UIApplication app = new(provider) { RootElement = new StackPanel(), ResizePollInterval = PollInterval };

		Task run = app.RunAsync(TestContext.CancellationToken);
		await AssertCompletesAsync(provider.ReadStarted, "The application should start waiting for input").ConfigureAwait(false);
		int passesBefore = provider.ClearCount;

		// Act
		app.RequestRender();

		// Assert
		await AssertRenderPassAfterAsync(provider, passesBefore, "RequestRender should produce a render pass with no key pressed").ConfigureAwait(false);

		app.Shutdown();
		await AssertCompletesAsync(run, "The run should still end when asked to shut down").ConfigureAwait(false);
	}

	/// <summary>
	/// The sizes a pass assigns while arranging the tree invalidate elements too. Those must not
	/// request another pass, or the application would redraw on every poll forever.
	/// </summary>
	[TestMethod]
	public async Task ARenderPassDoesNotRequestTheNextOne()
	{
		// Arrange
		BlockingConsoleProvider provider = new();
		StackPanel root = [new TextElement("left"), new BorderElement { Child = new TextElement("nested") }];
		UIApplication app = new(provider) { RootElement = root, ResizePollInterval = PollInterval };

		Task run = app.RunAsync(TestContext.CancellationToken);
		await AssertCompletesAsync(provider.ReadStarted, "The application should start waiting for input").ConfigureAwait(false);
		await Task.Delay(PollInterval * 5, TestContext.CancellationToken).ConfigureAwait(false);
		int passesBefore = provider.ClearCount;

		// Act
		await Task.Delay(PollInterval * 20, TestContext.CancellationToken).ConfigureAwait(false);

		// Assert
		Assert.AreEqual(passesBefore, provider.ClearCount, "With nothing changing, the application should not keep redrawing");

		app.Shutdown();
		await AssertCompletesAsync(run, "The run should still end when asked to shut down").ConfigureAwait(false);
	}

	/// <summary>
	/// Asserts that a render pass begins after <paramref name="passesBefore"/> were counted.
	/// </summary>
	/// <param name="provider">The provider that counts passes.</param>
	/// <param name="passesBefore">The count before the change.</param>
	/// <param name="because">The assertion message if no pass happens in time.</param>
	private async Task AssertRenderPassAfterAsync(BlockingConsoleProvider provider, int passesBefore, string because)
	{
		DateTime deadline = DateTime.UtcNow + StepTimeout;
		while (provider.ClearCount == passesBefore && DateTime.UtcNow < deadline)
		{
			await Task.Delay(PollInterval, TestContext.CancellationToken).ConfigureAwait(false);
		}

		Assert.IsGreaterThan(passesBefore, provider.ClearCount, because);
	}

	/// <summary>
	/// Asserts that <paramref name="task"/> completes within <see cref="StepTimeout"/>.
	/// </summary>
	/// <param name="task">The task to wait for.</param>
	/// <param name="because">The assertion message if it does not complete in time.</param>
	private static async Task AssertCompletesAsync(Task task, string because)
	{
		Task finished = await Task.WhenAny(task, Task.Delay(StepTimeout)).ConfigureAwait(false);
		Assert.AreSame(task, finished, because);

		// Observed separately so a task that failed reports its own exception, not the timeout.
		await task.ConfigureAwait(false);
	}
}
