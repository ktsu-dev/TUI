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
