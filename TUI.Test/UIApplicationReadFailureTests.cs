// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Contracts;
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
	/// A provider whose first <c>failures</c> reads fail the way <c>Console.ReadKey</c> does with
	/// redirected input, and whose next read is an exit key.
	/// </summary>
	private sealed class FailingReadConsoleProvider(int failures) : IConsoleProvider
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
			return count <= failures
				? Task.FromException<InputResult>(new InvalidOperationException("Cannot read keys when console input has been redirected."))
				: Task.FromResult(InputResult.Exit());
		}

		public void SetCursorVisibility(bool visible) => CursorVisible = visible;

		public void SetCursorPosition(Position position) { }
	}
}
