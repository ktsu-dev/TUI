// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;

/// <summary>
/// An <see cref="IConsoleProvider"/> test double whose <see cref="ReadInputAsync"/> does not
/// complete until a test releases it, standing in for a real provider parked in
/// <c>Console.ReadKey</c>.
/// </summary>
/// <remarks>
/// Lifecycle tests need the input loop to be genuinely blocked. <c>RecordingConsoleProvider</c>
/// returns input immediately, which spins the loop and hides whether a shutdown request can end a
/// run that is waiting on the keyboard — the case that left the cursor hidden after Ctrl+C.
/// </remarks>
internal sealed class BlockingConsoleProvider : IConsoleProvider
{
	private readonly TaskCompletionSource<InputResult> pendingRead =
		new(TaskCreationOptions.RunContinuationsAsynchronously);

	private readonly TaskCompletionSource readStarted =
		new(TaskCreationOptions.RunContinuationsAsynchronously);

	/// <summary>
	/// Gets a task that completes once the application has begun waiting for input.
	/// </summary>
	internal Task ReadStarted => readStarted.Task;

	/// <summary>
	/// Volatile because the application sets it from the thread running the app while the test
	/// reads it, and one assertion reads it mid-run rather than after the run has completed.
	/// </summary>
	private volatile bool cursorVisible = true;

	/// <summary>
	/// Gets the last cursor visibility set through <see cref="SetCursorVisibility"/>.
	/// </summary>
	internal bool CursorVisible => cursorVisible;

	private readonly Lock dimensionsLock = new();
	private int clearCount;

	/// <inheritdoc />
	/// <remarks>
	/// Guarded because a resize test writes it from the test thread while the application reads it
	/// from the thread running the loop — which is exactly the situation the resize poll exists for.
	/// </remarks>
	public Dimensions Dimensions
	{
		get
		{
			lock (dimensionsLock)
			{
				return field;
			}
		}

		set
		{
			lock (dimensionsLock)
			{
				field = value;
			}
		}
	} = new(80, 24);

	/// <summary>
	/// Gets the number of times <see cref="Clear"/> was called, which is once per render pass.
	/// </summary>
	internal int ClearCount => Volatile.Read(ref clearCount);

	/// <inheritdoc />
	public void Clear() => Interlocked.Increment(ref clearCount);

	/// <inheritdoc />
	public void Render(IUIElement element, Position position) => element?.Render(this);

	/// <inheritdoc />
	public void WriteAt(string text, Position position, TextStyle? style = null)
	{
		// Nothing to record: these tests assert on lifecycle, not on drawn output.
	}

	/// <inheritdoc />
	public Task<InputResult> ReadInputAsync()
	{
		readStarted.TrySetResult();
		return pendingRead.Task;
	}

	/// <inheritdoc />
	public void SetCursorVisibility(bool visible) => cursorVisible = visible;

	/// <inheritdoc />
	public void SetCursorPosition(Position position)
	{
		// Nothing to record: these tests assert on lifecycle, not on cursor placement.
	}

	/// <summary>
	/// Completes the pending read, as pressing a key would.
	/// </summary>
	/// <param name="input">The input to deliver.</param>
	internal void Release(InputResult input) => pendingRead.TrySetResult(input);
}
