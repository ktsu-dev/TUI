// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core.Models;

/// <summary>
/// Type alias for ConsoleModifiers to provide backward compatibility
/// </summary>
public static class InputModifiers
{
	/// <summary>
	/// No modifiers
	/// </summary>
	public static readonly ConsoleModifiers None = ConsoleModifiers.None;

	/// <summary>
	/// Alt modifier
	/// </summary>
	public static readonly ConsoleModifiers Alt = ConsoleModifiers.Alt;

	/// <summary>
	/// Control modifier
	/// </summary>
	public static readonly ConsoleModifiers Control = ConsoleModifiers.Control;

	/// <summary>
	/// Shift modifier
	/// </summary>
	public static readonly ConsoleModifiers Shift = ConsoleModifiers.Shift;
}

/// <summary>
/// Represents the result of input processing
/// </summary>
public readonly record struct InputResult
{
	/// <summary>
	/// Gets the type of input
	/// </summary>
	public InputType Type { get; init; }

	/// <summary>
	/// Gets the key that was pressed (for keyboard input)
	/// </summary>
	public ConsoleKey? Key { get; init; }

	/// <summary>
	/// Gets the character that was typed (for character input)
	/// </summary>
	public char? Character { get; init; }

	/// <summary>
	/// Gets the modifiers that were held (Ctrl, Alt, Shift)
	/// </summary>
	public ConsoleModifiers Modifiers { get; init; }

	/// <summary>
	/// Gets the mouse position (for mouse input)
	/// </summary>
	public Position? MousePosition { get; init; }

	/// <summary>
	/// Gets whether the input represents an exit request
	/// </summary>
	public bool IsExit { get; init; }

	/// <summary>
	/// Creates a keyboard input result
	/// </summary>
	/// <param name="key">The key that was pressed</param>
	/// <param name="modifiers">The modifiers that were held</param>
	/// <returns>The input result</returns>
	public static InputResult FromKey(ConsoleKey key, ConsoleModifiers modifiers = ConsoleModifiers.None) =>
		new() { Type = InputType.Keyboard, Key = key, Modifiers = modifiers };

	/// <summary>
	/// Creates a character input result
	/// </summary>
	/// <param name="character">The character that was typed</param>
	/// <returns>The input result</returns>
	public static InputResult FromCharacter(char character) =>
		new() { Type = InputType.Character, Character = character };

	/// <summary>
	/// Creates a mouse input result
	/// </summary>
	/// <param name="position">The mouse position</param>
	/// <returns>The input result</returns>
	public static InputResult FromMouse(Position position) =>
		new() { Type = InputType.Mouse, MousePosition = position };

	/// <summary>
	/// Creates an exit input result
	/// </summary>
	/// <returns>The exit input result</returns>
	public static InputResult Exit() =>
		new() { Type = InputType.Exit, IsExit = true };
}

/// <summary>
/// Defines the types of input
/// </summary>
public enum InputType
{
	/// <summary>
	/// No input
	/// </summary>
	None,

	/// <summary>
	/// Keyboard input
	/// </summary>
	Keyboard,

	/// <summary>
	/// Character input
	/// </summary>
	Character,

	/// <summary>
	/// Mouse input
	/// </summary>
	Mouse,

	/// <summary>
	/// Exit request
	/// </summary>
	Exit
}
