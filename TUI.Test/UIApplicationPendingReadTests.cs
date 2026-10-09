// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Elements;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests that a run does not leave a read behind when it ends, where it would take the next key
/// meant for the host or for a later run (ktsu-dev/TUI#149).
/// </summary>
[TestClass]
public sealed class UIApplicationPendingReadTests
{
	/// <summary>
	/// How long any single step is given before it is declared stuck.
	/// </summary>
	private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(10);

	/// <summary>
	/// Gets or sets the test context MSTest injects.
	/// </summary>
	public TestContext TestContext { get; set; } = null!;

	/// <summary>
	/// A run ended by <see cref="UIApplication.Shutdown"/> gives its read up, so no read is left
	/// waiting to take a key the host reads next.
	/// </summary>
	[TestMethod]
	public async Task AShutdownLeavesNoReadOutstanding()
	{
		KeyboardConsoleProvider provider = new(honoursCancellation: true);
		UIApplication app = new(provider) { InterruptSource = new FakeInterruptSource() };
		app.Setup(new KeyCountingElement());

		Task run = app.RunAsync(TestContext.CancellationToken);
		await AssertCompletesAsync(provider.WaitForReadsStartedAsync(1), "The run should start a read").ConfigureAwait(false);
		app.Shutdown();
		await AssertCompletesAsync(run, "Shutdown should end the run").ConfigureAwait(false);

		Assert.AreEqual(0, provider.PendingReadCount, "No read should still be waiting for a key once the run has ended");
	}

	/// <summary>
	/// A key pressed during a second run reaches the root, rather than the read the first run
	/// left behind.
	/// </summary>
	/// <param name="honoursCancellation">Whether the provider gives a read up when asked to.</param>
	[TestMethod]
	[DataRow(true)]
	[DataRow(false)]
	public async Task AKeyPressedInASecondRunReachesTheRoot(bool honoursCancellation)
	{
		KeyboardConsoleProvider provider = new(honoursCancellation);
		KeyCountingElement root = new();
		UIApplication app = new(provider) { InterruptSource = new FakeInterruptSource() };
		app.Setup(root);

		Task first = app.RunAsync(TestContext.CancellationToken);
		await AssertCompletesAsync(provider.WaitForReadsStartedAsync(1), "The first run should start a read").ConfigureAwait(false);
		app.Shutdown();
		await AssertCompletesAsync(first, "Shutdown should end the first run").ConfigureAwait(false);

		Task second = app.RunAsync(TestContext.CancellationToken);
		await AssertCompletesAsync(provider.WaitForPendingReadAsync(), "The second run should be waiting for a key").ConfigureAwait(false);
		provider.Press(ConsoleKey.A);
		await AssertCompletesAsync(root.FirstKey, "The key should reach the root of the second run").ConfigureAwait(false);
		app.Shutdown();
		await AssertCompletesAsync(second, "Shutdown should end the second run").ConfigureAwait(false);

		Assert.AreEqual(1, root.KeyCount);
	}

	private async Task AssertCompletesAsync(Task task, string message)
	{
		Task finished = await Task.WhenAny(task, Task.Delay(StepTimeout, TestContext.CancellationToken)).ConfigureAwait(false);
		Assert.AreSame(task, finished, message);
		await task.ConfigureAwait(false);
	}

	/// <summary>
	/// An element that counts the keys it is given.
	/// </summary>
	private sealed class KeyCountingElement : UIElementBase
	{
		private readonly TaskCompletionSource firstKey = new(TaskCreationOptions.RunContinuationsAsynchronously);
		private int keyCount;

		public Task FirstKey => firstKey.Task;

		public int KeyCount => Volatile.Read(ref keyCount);

		public override bool HandleInput(InputResult input)
		{
			Interlocked.Increment(ref keyCount);
			firstKey.TrySetResult();
			return true;
		}

		protected override void OnRender(IConsoleProvider provider)
		{
		}
	}

	/// <summary>
	/// A provider whose keys go to the earliest read still waiting, as with Console.ReadKey.
	/// </summary>
	/// <param name="honoursCancellation">Whether a read ends when its token is cancelled, as
	/// SpectreConsoleProvider's does, or keeps waiting, as a provider written against the
	/// parameterless ReadInputAsync does.</param>
	private sealed class KeyboardConsoleProvider(bool honoursCancellation) : IConsoleProvider
	{
		private readonly Lock gate = new();
		private readonly List<TaskCompletionSource<InputResult>> pending = [];
		private readonly List<(int Count, TaskCompletionSource Signal)> startedWaiters = [];
		private TaskCompletionSource pendingWaiter = new(TaskCreationOptions.RunContinuationsAsynchronously);
		private int started;

		public int PendingReadCount
		{
			get
			{
				lock (gate)
				{
					return pending.Count(read => !read.Task.IsCompleted);
				}
			}
		}

		public Dimensions Dimensions => new(80, 24);

		public void Clear() { }

		public void Render(IUIElement element, Position position) => element?.Render(this);

		public void WriteAt(string text, Position position, TextStyle? style = null) { }

		public void SetCursorVisibility(bool visible) { }

		public void SetCursorPosition(Position position) { }

		public Task<InputResult> ReadInputAsync() => StartRead(CancellationToken.None);

		public Task<InputResult> ReadInputAsync(CancellationToken cancellationToken) =>
			StartRead(honoursCancellation ? cancellationToken : CancellationToken.None);

		public Task WaitForReadsStartedAsync(int count)
		{
			lock (gate)
			{
				if (started >= count)
				{
					return Task.CompletedTask;
				}

				TaskCompletionSource signal = new(TaskCreationOptions.RunContinuationsAsynchronously);
				startedWaiters.Add((count, signal));
				return signal.Task;
			}
		}

		/// <summary>
		/// Completes once a read is waiting that was started, or carried over, after any earlier
		/// one was given up — that is, once a key pressed now would be read.
		/// </summary>
		public Task WaitForPendingReadAsync()
		{
			lock (gate)
			{
				if (pending.Exists(read => !read.Task.IsCompleted))
				{
					return Task.CompletedTask;
				}

				if (pendingWaiter.Task.IsCompleted)
				{
					pendingWaiter = new(TaskCreationOptions.RunContinuationsAsynchronously);
				}

				return pendingWaiter.Task;
			}
		}

		public void Press(ConsoleKey key)
		{
			TaskCompletionSource<InputResult>? read;
			lock (gate)
			{
				read = pending.Find(candidate => !candidate.Task.IsCompleted);
			}

			read?.TrySetResult(InputResult.FromKey(key));
		}

		private Task<InputResult> StartRead(CancellationToken cancellationToken)
		{
			TaskCompletionSource<InputResult> read = new(TaskCreationOptions.RunContinuationsAsynchronously);
			cancellationToken.Register(() => read.TrySetCanceled(cancellationToken));

			lock (gate)
			{
				pending.Add(read);
				started++;
				foreach ((int count, TaskCompletionSource signal) in startedWaiters.Where(waiter => started >= waiter.Count))
				{
					signal.TrySetResult();
				}

				pendingWaiter.TrySetResult();
			}

			return read.Task;
		}
	}
}
