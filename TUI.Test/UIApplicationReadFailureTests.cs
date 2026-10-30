// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Elements;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for how <see cref="UIApplication"/> treats a console provider whose reads fail, such as
/// <see cref="SpectreConsoleProvider"/> when stdin is redirected (ktsu-dev/TUI#143).
/// </summary>
[TestClass]
public sealed class UIApplicationReadFailureTests
{
	/// <summary>
	/// How long a run is given to end before it is declared stuck.
	/// </summary>
	private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(10);

	/// <summary>
	/// Gets or sets the test context MSTest injects.
	/// </summary>
	public TestContext TestContext { get; set; } = null!;

	/// <summary>
	/// A read that can never succeed ends the run with the provider's exception after a bounded
	/// number of attempts, instead of retrying it in a tight loop forever.
	/// </summary>
	[TestMethod]
	public async Task AReadThatAlwaysFailsEndsTheRun()
	{
		FailingReadConsoleProvider provider = new(failures: int.MaxValue);
		UIApplication app = new(provider) { InterruptSource = new FakeInterruptSource() };

		// Started on the thread pool because a read that fails synchronously never yields, and the
		// run is cancelled once the timeout passes, so a regression fails the test rather than
		// hanging it or leaving a run spinning in the test host.
		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
		Task run = Task.Run(() => app.RunAsync(timeout.Token), TestContext.CancellationToken);
		Task finished = await Task.WhenAny(run, Task.Delay(StepTimeout, TestContext.CancellationToken)).ConfigureAwait(false);
		await timeout.CancelAsync().ConfigureAwait(false);

		Assert.AreSame(run, finished, "A read that always fails should end the run");
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => run).ConfigureAwait(false);
		Assert.IsLessThanOrEqualTo(100, provider.ReadCount, "The failing read should be retried a bounded number of times");
		Assert.IsTrue(provider.CursorVisible, "The cursor must be restored when the run ends on a read failure");
	}

	/// <summary>
	/// The other exception the loop treats as recoverable is bounded the same way.
	/// </summary>
	[TestMethod]
	public async Task AReadThatAlwaysThrowsArgumentExceptionEndsTheRun()
	{
		FailingReadConsoleProvider provider = new(failures: int.MaxValue, () => new ArgumentException("Unreadable input."));
		UIApplication app = new(provider) { InterruptSource = new FakeInterruptSource() };

		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
		Task run = Task.Run(() => app.RunAsync(timeout.Token), TestContext.CancellationToken);
		Task finished = await Task.WhenAny(run, Task.Delay(StepTimeout, TestContext.CancellationToken)).ConfigureAwait(false);
		await timeout.CancelAsync().ConfigureAwait(false);

		Assert.AreSame(run, finished, "A read that always fails should end the run");
		await Assert.ThrowsExactlyAsync<ArgumentException>(() => run).ConfigureAwait(false);
		Assert.AreEqual(UIApplication.MaxConsecutiveReadFailures, provider.ReadCount);
	}

	/// <summary>
	/// An occasional failed read is still tolerated: the run carries on and ends normally.
	/// </summary>
	[TestMethod]
	public async Task AFewFailedReadsDoNotEndTheRun()
	{
		FailingReadConsoleProvider provider = new(failures: 2);
		UIApplication app = new(provider) { InterruptSource = new FakeInterruptSource() };

		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
		Task run = Task.Run(() => app.RunAsync(timeout.Token), TestContext.CancellationToken);
		Task finished = await Task.WhenAny(run, Task.Delay(StepTimeout, TestContext.CancellationToken)).ConfigureAwait(false);
		await timeout.CancelAsync().ConfigureAwait(false);

		Assert.AreSame(run, finished, "The run should end on the exit key that follows the failed reads");
		await run.ConfigureAwait(false);
		Assert.AreEqual(3, provider.ReadCount);
	}

	/// <summary>
	/// A read that fails with an OperationCanceledException of its own, such as a provider's
	/// internal timeout, is a failed read like any other rather than the run being cancelled
	/// (ktsu-dev/TUI#216).
	/// </summary>
	[TestMethod]
	public async Task AFewReadsFailingWithAForeignCancellationDoNotEndTheRun()
	{
		FailingReadConsoleProvider provider = new(failures: 2, () => new TaskCanceledException("Read timed out."));
		UIApplication app = new(provider) { InterruptSource = new FakeInterruptSource() };

		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
		Task run = Task.Run(() => app.RunAsync(timeout.Token), TestContext.CancellationToken);
		Task finished = await Task.WhenAny(run, Task.Delay(StepTimeout, TestContext.CancellationToken)).ConfigureAwait(false);
		await timeout.CancelAsync().ConfigureAwait(false);

		Assert.AreSame(run, finished, "The run should end on the exit key that follows the failed reads");
		await run.ConfigureAwait(false);
		Assert.AreEqual(3, provider.ReadCount);
	}

	/// <summary>
	/// A read that always fails with a foreign OperationCanceledException faults the run once the
	/// retries run out, instead of ending it as though it had been cancelled.
	/// </summary>
	[TestMethod]
	public async Task AReadThatAlwaysFailsWithAForeignCancellationFaultsTheRun()
	{
		FailingReadConsoleProvider provider = new(failures: int.MaxValue, () => new TaskCanceledException("Read timed out."));
		UIApplication app = new(provider) { InterruptSource = new FakeInterruptSource() };

		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
		Task run = Task.Run(() => app.RunAsync(timeout.Token), TestContext.CancellationToken);
		Task finished = await Task.WhenAny(run, Task.Delay(StepTimeout, TestContext.CancellationToken)).ConfigureAwait(false);
		await timeout.CancelAsync().ConfigureAwait(false);

		Assert.AreSame(run, finished, "A read that always fails should end the run");
		Assert.IsTrue(run.IsFaulted, $"The run should fault, not end as {run.Status}");
		InvalidOperationException ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => run).ConfigureAwait(false);
		Assert.IsInstanceOfType<TaskCanceledException>(ex.InnerException);
		Assert.AreEqual(UIApplication.MaxConsecutiveReadFailures, provider.ReadCount);
	}

	/// <summary>
	/// An async provider that throws an OperationCanceledException of its own finishes its read
	/// Canceled rather than Faulted. That read is not the one carried over from an earlier run, so
	/// it counts as a failed read and the run still faults once the retries run out, instead of
	/// spinning forever re-reading (ktsu-dev/TUI#242).
	/// </summary>
	[TestMethod]
	public async Task AnAsyncReadThatAlwaysFailsWithAForeignCancellationFaultsTheRun()
	{
		FailingReadConsoleProvider provider = new(failures: int.MaxValue, () => new TaskCanceledException("Read timed out."), failAsynchronously: true);
		UIApplication app = new(provider) { InterruptSource = new FakeInterruptSource() };

		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
		Task run = Task.Run(() => app.RunAsync(timeout.Token), TestContext.CancellationToken);
		Task finished = await Task.WhenAny(run, Task.Delay(StepTimeout, TestContext.CancellationToken)).ConfigureAwait(false);
		await timeout.CancelAsync().ConfigureAwait(false);

		Assert.AreSame(run, finished, "A read that always fails should end the run");
		Assert.IsTrue(run.IsFaulted, $"The run should fault, not end as {run.Status}");
		InvalidOperationException ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => run).ConfigureAwait(false);
		Assert.IsInstanceOfType<TaskCanceledException>(ex.InnerException);
		Assert.AreEqual(UIApplication.MaxConsecutiveReadFailures, provider.ReadCount);
	}

	/// <summary>
	/// A few async reads failing with a foreign cancellation are tolerated like any other failed read.
	/// </summary>
	[TestMethod]
	public async Task AFewAsyncReadsFailingWithAForeignCancellationDoNotEndTheRun()
	{
		FailingReadConsoleProvider provider = new(failures: 2, () => new TaskCanceledException("Read timed out."), failAsynchronously: true);
		UIApplication app = new(provider) { InterruptSource = new FakeInterruptSource() };

		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
		Task run = Task.Run(() => app.RunAsync(timeout.Token), TestContext.CancellationToken);
		Task finished = await Task.WhenAny(run, Task.Delay(StepTimeout, TestContext.CancellationToken)).ConfigureAwait(false);
		await timeout.CancelAsync().ConfigureAwait(false);

		Assert.AreSame(run, finished, "The run should end on the exit key that follows the failed reads");
		await run.ConfigureAwait(false);
		Assert.AreEqual(3, provider.ReadCount);
	}

	/// <summary>
	/// An input handler that throws an OperationCanceledException of its own, such as an
	/// HttpClient timeout, does not end the run as if the user had quit (ktsu-dev/TUI#216).
	/// </summary>
	[TestMethod]
	public async Task AHandlerThrowingAForeignCancellationDoesNotEndTheRun()
	{
		ScriptedConsoleProvider provider = new(InputResult.FromKey(ConsoleKey.A), InputResult.Exit());
		CancellingHandlerElement root = new();
		UIApplication app = new(provider) { InterruptSource = new FakeInterruptSource() };
		app.Setup(root);

		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
		Task run = Task.Run(() => app.RunAsync(timeout.Token), TestContext.CancellationToken);
		Task finished = await Task.WhenAny(run, Task.Delay(StepTimeout, TestContext.CancellationToken)).ConfigureAwait(false);
		await timeout.CancelAsync().ConfigureAwait(false);

		Assert.AreSame(run, finished, "The run should end on the exit key that follows the failing handler");
		await run.ConfigureAwait(false);
		Assert.AreEqual(1, root.HandledCount);
		Assert.AreEqual(2, provider.ReadCount, "The run should read on past the handler's failure to the exit key");
	}

	/// <summary>
	/// An element whose input handler fails the way a timed-out HttpClient call does.
	/// </summary>
	private sealed class CancellingHandlerElement : UIElementBase
	{
		public int HandledCount { get; private set; }

		protected override void OnRender(IConsoleProvider provider) { }

		protected override bool OnHandleInput(InputResult input)
		{
			HandledCount++;
			throw new TaskCanceledException("HttpClient timeout");
		}
	}

	/// <summary>
	/// A provider that returns the given inputs in order, then exit keys.
	/// </summary>
	private sealed class ScriptedConsoleProvider(params InputResult[] inputs) : IConsoleProvider
	{
		private int readCount;

		public int ReadCount => Volatile.Read(ref readCount);

		public Dimensions Dimensions => new(80, 24);

		public void Clear() { }

		public void Render(IUIElement element, Position position) => element?.Render(this);

		public void WriteAt(string text, Position position, TextStyle? style = null) { }

		public Task<InputResult> ReadInputAsync()
		{
			int count = Interlocked.Increment(ref readCount);
			return Task.FromResult(count <= inputs.Length ? inputs[count - 1] : InputResult.Exit());
		}

		public void SetCursorVisibility(bool visible) { }

		public void SetCursorPosition(Position position) { }
	}

	/// <summary>
	/// A provider whose first <c>failures</c> reads fail the way <c>Console.ReadKey</c> does with
	/// redirected input, and whose next read is an exit key.
	/// </summary>
	private sealed class FailingReadConsoleProvider(int failures, Func<Exception>? failure = null, bool failAsynchronously = false) : IConsoleProvider
	{
		private int readCount;

		public int ReadCount => Volatile.Read(ref readCount);

		public bool CursorVisible { get; private set; } = true;

		public Dimensions Dimensions => new(80, 24);

		public void Clear() { }

		public void Render(IUIElement element, Position position) => element?.Render(this);

		public void WriteAt(string text, Position position, TextStyle? style = null) { }

		public Task<InputResult> ReadInputAsync()
		{
			int count = Interlocked.Increment(ref readCount);
			if (count > failures)
			{
				return Task.FromResult(InputResult.Exit());
			}

			return failAsynchronously
				? FailAfterYieldAsync()
				: Task.FromException<InputResult>(CreateFailure());
		}

		/// <summary>
		/// Fails the way an async method does: an OperationCanceledException thrown here leaves the
		/// task Canceled, where Task.FromException leaves it Faulted.
		/// </summary>
		private async Task<InputResult> FailAfterYieldAsync()
		{
			await Task.Yield();
			throw CreateFailure();
		}

		private Exception CreateFailure() =>
			failure?.Invoke() ?? new InvalidOperationException("Cannot read keys when console input has been redirected.");

		public void SetCursorVisibility(bool visible) => CursorVisible = visible;

		public void SetCursorPosition(Position position) { }
	}
}
