// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using System.Collections.ObjectModel;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;

/// <summary>
/// An <see cref="IConsoleProvider"/> test double that records every <see cref="WriteAt"/> call
/// instead of drawing, so a test can assert what an element drew and where it drew it.
/// </summary>
/// <remarks>
/// Rendering to the real <c>SpectreConsoleProvider</c> would write to the test runner's console
/// and would make the drawn output unobservable. Recording the calls keeps render tests
/// headless and lets them assert on positions rather than only on "did not throw".
/// </remarks>
internal sealed class RecordingConsoleProvider : IConsoleProvider
{
	/// <summary>
	/// One recorded <see cref="WriteAt"/> call.
	/// </summary>
	/// <param name="Text">The text that was written.</param>
	/// <param name="Position">The position it was written at.</param>
	/// <param name="Style">The style it was written with, if any.</param>
	internal sealed record Write(string Text, Position Position, TextStyle? Style);

	private readonly Collection<Write> writes = [];

	/// <summary>
	/// Gets every <see cref="WriteAt"/> call recorded so far, in call order.
	/// </summary>
	internal IReadOnlyList<Write> Writes => writes;

	/// <summary>
	/// Gets the number of times <see cref="Clear"/> was called.
	/// </summary>
	internal int ClearCount { get; private set; }

	/// <inheritdoc />
	public Dimensions Dimensions { get; set; } = new(80, 24);

	/// <inheritdoc />
	public void Clear() => ClearCount++;

	/// <inheritdoc />
	public void Render(IUIElement element, Position position) => element?.Render(this);

	/// <inheritdoc />
	public void WriteAt(string text, Position position, TextStyle? style = null) =>
		writes.Add(new Write(text, position, style));

	/// <inheritdoc />
	public Task<InputResult> ReadInputAsync() => Task.FromResult(new InputResult());

	/// <inheritdoc />
	public void SetCursorVisibility(bool visible) => CursorVisible = visible;

	/// <inheritdoc />
	public void SetCursorPosition(Position position) => CursorPosition = position;

	/// <summary>
	/// Gets the last cursor visibility set through <see cref="SetCursorVisibility"/>.
	/// </summary>
	internal bool CursorVisible { get; private set; } = true;

	/// <summary>
	/// Gets the last cursor position set through <see cref="SetCursorPosition"/>.
	/// </summary>
	internal Position CursorPosition { get; private set; }

	/// <summary>
	/// Returns every recorded write whose text is exactly <paramref name="text"/>.
	/// </summary>
	/// <param name="text">The text to match.</param>
	/// <returns>The matching writes, in call order.</returns>
	internal IEnumerable<Write> WritesOf(string text) =>
		writes.Where(w => string.Equals(w.Text, text, StringComparison.Ordinal));
}
