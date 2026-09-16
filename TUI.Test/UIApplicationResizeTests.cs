// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for <see cref="UIApplication"/>'s handling of a terminal that changes size mid-run.
/// </summary>
/// <remarks>
/// The size used to be read once, on the first pass that found the root element unsized, and never
/// looked at again — so resizing the window left every layout pinned to the size at launch
/// (ktsu-dev/TUI#111). Two halves are covered here: a render pass picks the new size up, and the
/// input loop produces a pass at all, since a resize delivers no keypress to wake it.
/// </remarks>
[TestClass]
public sealed class UIApplicationResizeTests
{
	/// <summary>
	/// How long any single step of a run is given before it is declared stuck.
	/// </summary>
	/// <remarks>
	/// Bounds every wait in this class. The regression under test is an application that never
	/// notices a resize, so an unbounded wait would hang the suite instead of reporting it.
	/// </remarks>
	private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(10);

	/// <summary>
	/// The size the terminal starts at in these tests.
	/// </summary>
	private static readonly Dimensions LaunchSize = new(80, 24);

	/// <summary>
	/// The size the terminal is resized to. Narrower than <see cref="WideText"/> so the clamping
	/// in <see cref="StackPanel"/> produces a visibly different layout rather than the same one.
	/// </summary>
	private static readonly Dimensions ResizedSize = new(40, 10);

	/// <summary>
	/// Text wider than <see cref="ResizedSize"/> but narrower than <see cref="LaunchSize"/>, so
	/// the width it is laid out at reports which of the two sizes the layout used.
	/// </summary>
	private static readonly string WideText = new('x', 60);

	/// <summary>
	/// Gets or sets the test context MSTest injects, used for its cancellation token so a run
	/// started by a test ends when the test run itself is cancelled.
	/// </summary>
	public TestContext TestContext { get; set; } = null!;

	/// <summary>
	/// A render pass after the terminal changed size must lay the root out at the new size. It
	/// used to keep the size from the first pass forever, so the UI stayed pinned to the size the
	/// window happened to have at launch.
	/// </summary>
	[TestMethod]
	public void RenderAfterAResizeGivesTheRootTheNewTerminalSize()
	{
		// Arrange
		RecordingConsoleProvider provider = new() { Dimensions = LaunchSize };
		StackPanel root = [];
		UIApplication app = new(provider) { RootElement = root };
		app.Render();
		Assert.AreEqual(LaunchSize, root.Dimensions, "The first pass should size the root to the terminal");

		// Act
		provider.Dimensions = ResizedSize;
		app.Render();

		// Assert
		Assert.AreEqual(ResizedSize, root.Dimensions, "A render pass after a resize should adopt the new terminal size");
	}

	/// <summary>
	/// The resize has to reach the whole tree, not just the root. A container arranges its own
	/// children but not theirs, so re-arranging only the root would leave every grandchild at the
	/// size it had before.
	/// </summary>
	[TestMethod]
	public void RenderAfterAResizeRelaysOutEveryContainerBeneathTheRoot()
	{
		// Arrange
		RecordingConsoleProvider provider = new() { Dimensions = LaunchSize };
		BorderElement root = CreateNestedLayout(out StackPanel panel, out TextElement leaf);
		UIApplication app = new(provider) { RootElement = root };
		app.Render();
		Assert.AreEqual(LaunchSize.WithoutPadding(root.Padding), panel.Dimensions, "The panel should start out filling the terminal, inside the border");
		Assert.AreEqual(WideText.Length, leaf.Dimensions.Width, "The label should start out at its full width");

		// Act
		provider.Dimensions = ResizedSize;
		app.Render();

		// Assert
		Dimensions resizedContentArea = ResizedSize.WithoutPadding(root.Padding);
		Assert.AreEqual(resizedContentArea, panel.Dimensions, "The panel should be re-arranged into the resized root");
		Assert.AreEqual(resizedContentArea.Width, leaf.Dimensions.Width, "The label should be clamped to the narrower terminal, which only happens if the panel re-arranged its own children too");
	}

	/// <summary>
	/// Between resizes, a size the host assigned to the root is its own business. Only an actual
	/// change of terminal size overrides it — at which point keeping the old one would draw
	/// outside the window.
	/// </summary>
	[TestMethod]
	public void RenderWithoutAResizeLeavesAHostAssignedRootSizeAlone()
	{
		// Arrange
		Dimensions chosenByHost = new(20, 5);
		RecordingConsoleProvider provider = new() { Dimensions = LaunchSize };
		StackPanel root = new() { Dimensions = chosenByHost };
		UIApplication app = new(provider) { RootElement = root };

		// Act
		app.Render();
		app.Render();

		// Assert
		Assert.AreEqual(chosenByHost, root.Dimensions, "A root the host sized itself should keep that size while the terminal has not changed");
	}

	/// <summary>
	/// A resize is worth a line in the log: it re-lays out the whole tree, and it is the one thing
	/// that overrides a size the host chose, so someone reading the log should be able to see it
	/// happen.
	/// </summary>
	[TestMethod]
	public void AResizeIsReported()
	{
		// Arrange
		RecordingLogger logger = new();
		RecordingConsoleProvider provider = new() { Dimensions = LaunchSize };
		StackPanel root = [];
		UIApplication app = new(provider, logger) { RootElement = root };
		app.Render();

		// Act
		provider.Dimensions = ResizedSize;
		app.Render();

		// Assert
		Assert.IsTrue(
			logger.Messages.Any(m => m.Contains($"{ResizedSize.Width}x{ResizedSize.Height}", StringComparison.Ordinal)),
			$"The new terminal size should be reported, but the log held: {string.Join(" | ", logger.Messages)}");
	}

	/// <summary>
	/// A resize arrives as no input at all, so a loop that only wakes for a keypress cannot see
	/// one. The running application must notice it and redraw without the user pressing anything.
	/// </summary>
	[TestMethod]
	public async Task AResizeWhileWaitingForInputRedrawsWithoutAKeypress()
	{
		// Arrange
		BlockingConsoleProvider provider = new() { Dimensions = LaunchSize };
		BorderElement root = CreateNestedLayout(out StackPanel panel, out TextElement leaf);
		UIApplication app = new(provider)
		{
			RootElement = root,
			ResizePollInterval = TimeSpan.FromMilliseconds(10)
		};

		Task run = app.RunAsync(TestContext.CancellationToken);
		await AssertCompletesAsync(provider.ReadStarted, "The application should start waiting for input").ConfigureAwait(false);

		// Act
		provider.Dimensions = ResizedSize;

		// Assert
		Dimensions resizedContentArea = ResizedSize.WithoutPadding(root.Padding);
		await AssertResizedAsync(leaf, resizedContentArea.Width, "A resize should be picked up while the application is blocked waiting for a key, with no key pressed").ConfigureAwait(false);
		Assert.AreEqual(resizedContentArea, panel.Dimensions, "The whole tree should be re-arranged by the redraw the resize triggered");

		app.Shutdown();
		await AssertCompletesAsync(run, "The run should still end when asked to shut down").ConfigureAwait(false);
	}

	/// <summary>
	/// Waking to check the size must not turn into redrawing on a timer: a full clear and redraw
	/// ten times a second would flicker for no reason. Only a size that actually changed draws.
	/// </summary>
	[TestMethod]
	public async Task WaitingForInputWithoutAResizeDoesNotRedraw()
	{
		// Arrange
		TimeSpan pollInterval = TimeSpan.FromMilliseconds(10);
		BlockingConsoleProvider provider = new() { Dimensions = LaunchSize };
		StackPanel root = [];
		UIApplication app = new(provider)
		{
			RootElement = root,
			ResizePollInterval = pollInterval
		};

		Task run = app.RunAsync(TestContext.CancellationToken);
		await AssertCompletesAsync(provider.ReadStarted, "The application should start waiting for input").ConfigureAwait(false);
		int clearsAfterTheFirstRender = provider.ClearCount;

		// Act
		await Task.Delay(pollInterval * 20, TestContext.CancellationToken).ConfigureAwait(false);

		// Assert
		Assert.AreEqual(clearsAfterTheFirstRender, provider.ClearCount, "Polling for a resize should not redraw while the terminal size is unchanged");

		app.Shutdown();
		await AssertCompletesAsync(run, "The run should still end when asked to shut down").ConfigureAwait(false);
	}

	/// <summary>
	/// Builds a root containing a container that in turn contains a leaf, so a test can tell a
	/// relayout of the root apart from a relayout of the whole tree.
	/// </summary>
	/// <param name="panel">The container between the root and the leaf.</param>
	/// <param name="leaf">The leaf whose width reports which size the layout used.</param>
	/// <returns>The root element.</returns>
	private static BorderElement CreateNestedLayout(out StackPanel panel, out TextElement leaf)
	{
		leaf = new TextElement(WideText);

		panel = [];
		panel.AddChild(leaf);

		BorderElement root = [];
		root.AddChild(panel);

		return root;
	}

	/// <summary>
	/// Waits until <paramref name="element"/> has been laid out at <paramref name="expectedWidth"/>.
	/// </summary>
	/// <param name="element">The element to watch.</param>
	/// <param name="expectedWidth">The width the resize should produce.</param>
	/// <param name="because">The assertion message if it never happens.</param>
	/// <remarks>
	/// Driven by <see cref="IUIElement.Invalidated"/> rather than by sleeping: assigning
	/// <see cref="IUIElement.Dimensions"/> raises it, and completing the task from the handler is
	/// what establishes that the value the assertion reads was published by the application's
	/// thread.
	/// </remarks>
	private static async Task AssertResizedAsync(TextElement element, int expectedWidth, string because)
	{
		TaskCompletionSource resized = new(TaskCreationOptions.RunContinuationsAsynchronously);

		void OnInvalidated(object? sender, EventArgs e)
		{
			if (element.Dimensions.Width == expectedWidth)
			{
				resized.TrySetResult();
			}
		}

		element.Invalidated += OnInvalidated;

		try
		{
			// Covers the resize having already landed between the act and this subscription, in
			// which case no further event is coming.
			OnInvalidated(element, EventArgs.Empty);
			await AssertCompletesAsync(resized.Task, because).ConfigureAwait(false);
		}
		finally
		{
			element.Invalidated -= OnInvalidated;
		}
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
