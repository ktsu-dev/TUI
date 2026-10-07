// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Core.Services;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;

/// <summary>
/// Forwards to another console provider, shifting every position it is given by a fixed offset
/// </summary>
/// <remarks>
/// Elements draw at absolute coordinates taken from their own <see cref="IUIElement.Position"/>,
/// so drawing one somewhere else means moving what it writes rather than moving the cursor first:
/// every write places the cursor itself (ktsu-dev/TUI#156). Moving the writes leaves the element
/// and its layout untouched.
/// </remarks>
/// <param name="inner">The provider to draw through</param>
/// <param name="offset">How far to shift each position</param>
internal sealed class OffsetConsoleProvider(IConsoleProvider inner, Position offset) : IConsoleProvider
{
	/// <inheritdoc />
	public Dimensions Dimensions => inner.Dimensions;

	/// <inheritdoc />
	public void Clear() => inner.Clear();

	/// <inheritdoc />
	public void Render(IUIElement element, Position position) => inner.Render(element, position + offset);

	/// <inheritdoc />
	public void WriteAt(string text, Position position, TextStyle? style = null) => inner.WriteAt(text, position + offset, style);

	/// <inheritdoc />
	public Task<InputResult> ReadInputAsync() => inner.ReadInputAsync();

	/// <inheritdoc />
	public void SetCursorVisibility(bool visible) => inner.SetCursorVisibility(visible);

	/// <inheritdoc />
	public void SetCursorPosition(Position position) => inner.SetCursorPosition(position + offset);
}
