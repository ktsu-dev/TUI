// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core.Contracts;

using ktsu.TUI.Core.Models;

/// <summary>
/// Defines the contract for console providers that handle rendering and input
/// </summary>
public interface IConsoleProvider
{
	/// <summary>
	/// Gets the current console dimensions
	/// </summary>
	public Dimensions Dimensions { get; }

	/// <summary>
	/// Clears the console
	/// </summary>
	public void Clear();

	/// <summary>
	/// Renders a UI element at the specified position
	/// </summary>
	/// <param name="element">The UI element to render</param>
	/// <param name="position">The position to render at</param>
	public void Render(IUIElement element, Position position);

	/// <summary>
	/// Writes text at the specified position with optional styling
	/// </summary>
	/// <param name="text">The text to write</param>
	/// <param name="position">The position to write at</param>
	/// <param name="style">Optional text style</param>
	public void WriteAt(string text, Position position, TextStyle? style = null);

	/// <summary>
	/// Reads input from the console
	/// </summary>
	/// <returns>The input result</returns>
	public Task<InputResult> ReadInputAsync();

	/// <summary>
	/// Sets the cursor visibility
	/// </summary>
	/// <param name="visible">Whether the cursor should be visible</param>
	public void SetCursorVisibility(bool visible);

	/// <summary>
	/// Sets the cursor position
	/// </summary>
	/// <param name="position">The cursor position</param>
	public void SetCursorPosition(Position position);
}
