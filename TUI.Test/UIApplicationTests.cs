// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for <see cref="UIApplication"/>'s run lifecycle: how a run ends, and whether the terminal
/// state it changes on the way in is put back on the way out.
/// </summary>
[TestClass]
public sealed class UIApplicationTests
{
	/// <summary>
	/// How long any single step of a run is given before it is declared stuck. Generous, because
	/// the assertion being made is "this happens at all", not "this happens quickly".
	/// </summary>
	/// <remarks>
	/// Every wait in this class is bounded by it. The behaviour under test is an application that
	/// fails to notice a shutdown request, so an unbounded wait would hang the suite on a
	/// regression instead of reporting one.
	/// </remarks>
	private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(10);

	/// <summary>
	/// Tests that an interrupt signal ends a run that is blocked waiting for a key, and leaves the
	/// cursor visible. Ctrl+C used to terminate the process at the runtime level instead, skipping
	/// the teardown that restores the cursor and leaving the user's terminal without one.
	/// </summary>
	[TestMethod]
	public async Task InterruptSignalEndsTheRunAndRestoresTheCursor()
	{
		// Arrange
		BlockingConsoleProvider provider = new();
		FakeInterruptSource interrupts = new();
		UIApplication app = new(provider) { InterruptSource = interrupts };

		Task run = app.RunAsync();
		await AssertCompletesAsync(interrupts.Registered, "The application should register an interrupt handler when it starts").ConfigureAwait(false);
		await AssertCompletesAsync(provider.ReadStarted, "The application should start waiting for input").ConfigureAwait(false);
		Assert.IsFalse(provider.CursorVisible, "The application should hide the cursor while running");

		// Act
		interrupts.RaiseInterrupt();

		// Assert
		await AssertCompletesAsync(run, "An interrupt should end the run even while it is blocked waiting for a key").ConfigureAwait(false);
		Assert.IsTrue(provider.CursorVisible, "The cursor must be visible again once the application has exited");
		Assert.IsFalse(app.IsRunning, "The application should not report itself as running after an interrupt");
	}

	/// <summary>
	/// Tests that the interrupt registration is released when the run ends, so the application
	/// stops taking Ctrl+C once it is no longer the one owning the terminal.
	/// </summary>
	[TestMethod]
	public async Task TheInterruptRegistrationIsReleasedWhenTheRunEnds()
	{
		// Arrange
		BlockingConsoleProvider provider = new();
		FakeInterruptSource interrupts = new();
		UIApplication app = new(provider) { InterruptSource = interrupts };

		Task run = app.RunAsync();
		await AssertCompletesAsync(interrupts.Registered, "The application should register an interrupt handler when it starts").ConfigureAwait(false);
		Assert.AreEqual(0, interrupts.DisposeCount, "The registration should stay live while the application runs");

		// Act
		interrupts.RaiseInterrupt();
		await AssertCompletesAsync(run, "An interrupt should end the run").ConfigureAwait(false);

		// Assert
		Assert.AreEqual(1, interrupts.DisposeCount, "The interrupt registration should be disposed exactly once");
	}

	/// <summary>
	/// Tests that cancelling the token passed to <see cref="UIApplication.RunAsync"/> ends a run
	/// that is blocked waiting for a key. The input loop used to await the provider's read
	/// directly, so a cancelled run kept waiting until the user pressed an unrelated key.
	/// </summary>
	[TestMethod]
	public async Task CancellingTheRunTokenEndsARunBlockedOnInput()
	{
		// Arrange
		BlockingConsoleProvider provider = new();
		FakeInterruptSource interrupts = new();
		using CancellationTokenSource cancellation = new();
		UIApplication app = new(provider) { InterruptSource = interrupts };

		Task run = app.RunAsync(cancellation.Token);
		await AssertCompletesAsync(provider.ReadStarted, "The application should start waiting for input").ConfigureAwait(false);

		// Act
		await cancellation.CancelAsync().ConfigureAwait(false);

		// Assert
		await AssertCompletesAsync(run, "Cancelling the run token should end a run blocked waiting for a key").ConfigureAwait(false);
		Assert.IsTrue(provider.CursorVisible, "The cursor must be visible again once the application has exited");
	}

	/// <summary>
	/// Tests that <see cref="UIApplication.Shutdown"/> ends a run blocked waiting for a key, which
	/// is the path an interrupt takes and the path an element requesting exit takes.
	/// </summary>
	[TestMethod]
	public async Task ShutdownEndsARunBlockedOnInput()
	{
		// Arrange
		BlockingConsoleProvider provider = new();
		FakeInterruptSource interrupts = new();
		UIApplication app = new(provider) { InterruptSource = interrupts };

		Task run = app.RunAsync();
		await AssertCompletesAsync(provider.ReadStarted, "The application should start waiting for input").ConfigureAwait(false);

		// Act
		app.Shutdown();

		// Assert
		await AssertCompletesAsync(run, "Shutdown should end a run blocked waiting for a key").ConfigureAwait(false);
		Assert.IsTrue(provider.CursorVisible, "The cursor must be visible again once the application has exited");
	}

	/// <summary>
	/// Tests that the ordinary exit path still works: input flagged as an exit request ends the
	/// run and restores the cursor.
	/// </summary>
	[TestMethod]
	public async Task ExitInputEndsTheRunAndRestoresTheCursor()
	{
		// Arrange
		BlockingConsoleProvider provider = new();
		FakeInterruptSource interrupts = new();
		UIApplication app = new(provider) { InterruptSource = interrupts };

		Task run = app.RunAsync();
		await AssertCompletesAsync(provider.ReadStarted, "The application should start waiting for input").ConfigureAwait(false);

		// Act
		provider.Release(InputResult.Exit());

		// Assert
		await AssertCompletesAsync(run, "Exit input should end the run").ConfigureAwait(false);
		Assert.IsTrue(provider.CursorVisible, "The cursor must be visible again once the application has exited");
		Assert.IsFalse(app.IsRunning, "The application should not report itself as running after exiting");
	}

	/// <summary>
	/// Tests that the real interrupt source hooks and unhooks the process signals without
	/// throwing on whichever platform the suite is running on, and that disposing twice is safe.
	/// </summary>
	[TestMethod]
	public void TheConsoleInterruptSourceHooksAndUnhooksTheProcessSignals()
	{
		// Arrange
		ConsoleInterruptSource source = new();

		// Act
		IDisposable registration = source.Register(() => { });

		// Assert
		Assert.IsNotNull(registration, "Registering should return a registration to dispose");
		registration.Dispose();
		registration.Dispose();
	}

	/// <summary>
	/// Tests that the real interrupt source rejects a missing callback rather than hooking a
	/// signal it cannot act on.
	/// </summary>
	[TestMethod]
	public void TheConsoleInterruptSourceRejectsAMissingCallback()
	{
		// Arrange
		ConsoleInterruptSource source = new();

		// Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() => source.Register(null!));
	}

	/// <summary>
	/// Awaits <paramref name="task"/> and fails with <paramref name="because"/> if it does not
	/// finish within <see cref="StepTimeout"/>.
	/// </summary>
	/// <param name="task">The task to await.</param>
	/// <param name="because">The assertion message to report on a timeout.</param>
	private static async Task AssertCompletesAsync(Task task, string because)
	{
		Task finished = await Task.WhenAny(task, Task.Delay(StepTimeout)).ConfigureAwait(false);
		Assert.AreSame(task, finished, because);

		// Observed separately so a task that failed reports its own exception, not the timeout.
		await task.ConfigureAwait(false);
	}
}
