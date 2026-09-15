// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Contracts;

/// <summary>
/// An <see cref="IInterruptSource"/> test double a test can raise on demand.
/// </summary>
/// <remarks>
/// The real source is the process's own Ctrl+C and SIGTERM handling, which a test cannot trigger
/// without signalling — and very likely terminating — the test host itself.
/// </remarks>
internal sealed class FakeInterruptSource : IInterruptSource
{
	private readonly TaskCompletionSource registered =
		new(TaskCreationOptions.RunContinuationsAsynchronously);

	private Action? handler;

	/// <summary>
	/// Gets a task that completes once the application has registered its interrupt handler.
	/// </summary>
	internal Task Registered => registered.Task;

	/// <summary>
	/// Gets the number of times the registration returned by <see cref="Register"/> was disposed.
	/// </summary>
	internal int DisposeCount { get; private set; }

	/// <inheritdoc />
	public IDisposable Register(Action onInterrupt)
	{
		handler = onInterrupt;
		registered.TrySetResult();
		return new Registration(this);
	}

	/// <summary>
	/// Invokes the registered handler, as Ctrl+C or SIGTERM would.
	/// </summary>
	internal void RaiseInterrupt() => handler?.Invoke();

	private sealed class Registration(FakeInterruptSource owner) : IDisposable
	{
		/// <inheritdoc />
		public void Dispose() => owner.DisposeCount++;
	}
}
