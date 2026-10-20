// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests that a host can change the element tree from another thread through
/// <see cref="UIApplication.Post"/> while the application runs.
/// </summary>
/// <remarks>
/// The tree is not synchronised, and the loop thread walks every container's children on each
/// pass. A timer adding and removing children directly dropped frames with "Collection was
/// modified" and then faulted the run with a <see cref="NullReferenceException"/> from a torn
/// <c>List.Add</c> (ktsu-dev/TUI#225).
/// </remarks>
[TestClass]
public sealed class UIApplicationPostTests
{
	private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(10);

	private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(1);

	/// <summary>
	/// Gets or sets the test context MSTest injects, used for its cancellation token.
	/// </summary>
	public TestContext TestContext { get; set; } = null!;

	/// <summary>
	/// The issue's reproduction, with the changes posted: a background task adding and removing
	/// children as fast as it can for two seconds neither faults the run nor fails a pass.
	/// </summary>
	[TestMethod]
	public async Task StructuralChangesPostedFromAnotherThreadDoNotFaultTheRun()
	{
		// Arrange
		BlockingConsoleProvider provider = new();
		RecordingLogger logger = new();
		StackPanel root = [];
		for (int i = 0; i < 20; i++)
		{
			root.AddChild(new TextElement("t" + i));
		}

		UIApplication app = new(provider, logger) { RootElement = root, ResizePollInterval = PollInterval };
		Task run = app.RunAsync(TestContext.CancellationToken);
		await AssertCompletesAsync(provider.ReadStarted, "The application should start waiting for input").ConfigureAwait(false);

		// Act
		using CancellationTokenSource stop = new(TimeSpan.FromSeconds(2));
		int posted = 0;
		int pending = 0;
		Task churn = Task.Run(() =>
		{
			while (!stop.IsCancellationRequested)
			{
				// Bounded, so the queue measures the race rather than how fast the loop can drain
				// a backlog of millions.
				if (Volatile.Read(ref pending) >= 100)
				{
					Thread.Yield();
					continue;
				}

				Interlocked.Increment(ref pending);
				app.Post(() =>
				{
					TextElement t = new("x");
					root.AddChild(t);
					root.RemoveChild(t);
					Interlocked.Decrement(ref pending);
				});
				posted++;
			}
		}, TestContext.CancellationToken);
		await churn.ConfigureAwait(false);

		// The last posted change has run once a pass begins after it.
		TaskCompletionSource drained = new(TaskCreationOptions.RunContinuationsAsynchronously);
		app.Post(drained.SetResult);
		await AssertCompletesAsync(drained.Task, "The posted changes should all run").ConfigureAwait(false);

		// Assert
		Assert.IsFalse(run.IsCompleted, $"The run ended while the tree was being changed: {run.Exception}");
		Assert.IsGreaterThan(0, posted);
		Assert.HasCount(20, root.Children);
		Assert.DoesNotContain("An error occurred while rendering the UI", logger.Messages);

		app.Shutdown();
		await AssertCompletesAsync(run, "The run should still end when asked to shut down").ConfigureAwait(false);
	}

	/// <summary>
	/// A change posted from another thread runs and is drawn without a key being pressed.
	/// </summary>
	[TestMethod]
	public async Task APostedChangeRunsAndIsDrawnWithoutAKeypress()
	{
		// Arrange
		BlockingConsoleProvider provider = new();
		StackPanel root = [];
		UIApplication app = new(provider) { RootElement = root, ResizePollInterval = PollInterval };
		Task run = app.RunAsync(TestContext.CancellationToken);
		await AssertCompletesAsync(provider.ReadStarted, "The application should start waiting for input").ConfigureAwait(false);
		int passesBefore = provider.ClearCount;
		TaskCompletionSource ranOn = new(TaskCreationOptions.RunContinuationsAsynchronously);

		// Act
		await Task.Run(() => app.Post(() =>
		{
			root.AddChild(new TextElement("added"));
			ranOn.SetResult();
		}), TestContext.CancellationToken).ConfigureAwait(false);

		// Assert
		await AssertCompletesAsync(ranOn.Task, "The posted change should run").ConfigureAwait(false);
		Assert.HasCount(1, root.Children);
		DateTime deadline = DateTime.UtcNow + StepTimeout;
		while (provider.ClearCount == passesBefore && DateTime.UtcNow < deadline)
		{
			await Task.Delay(PollInterval, TestContext.CancellationToken).ConfigureAwait(false);
		}

		Assert.IsGreaterThan(passesBefore, provider.ClearCount, "Posting a change should produce a render pass with no key pressed");

		app.Shutdown();
		await AssertCompletesAsync(run, "The run should still end when asked to shut down").ConfigureAwait(false);
	}

	/// <summary>
	/// Posting nothing is refused at the call, not when the loop reaches it.
	/// </summary>
	[TestMethod]
	public void PostingNullThrows()
	{
		UIApplication app = new(new BlockingConsoleProvider());

		Assert.ThrowsExactly<ArgumentNullException>(() => app.Post(null!));
	}

	private static async Task AssertCompletesAsync(Task task, string because)
	{
		Task finished = await Task.WhenAny(task, Task.Delay(StepTimeout)).ConfigureAwait(false);
		Assert.AreSame(task, finished, because);
		await task.ConfigureAwait(false);
	}
}
