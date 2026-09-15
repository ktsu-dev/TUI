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
	/// Gets the last cursor visibility set through <see cref="SetCursorVisibility"/>.
	/// </summary>
	internal bool CursorVisible => Volatile.Read(ref cursorVisible);

	private bool cursorVisible = true;

	/// <inheritdoc />
	public Dimensions Dimensions { get; set; } = new(80, 24);

	/// <inheritdoc />
	public void Clear()
	{
		// Nothing to record: these tests assert on lifecycle, not on drawn output.
	}

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
	public void SetCursorVisibility(bool visible) => Volatile.Write(ref cursorVisible, visible);

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
