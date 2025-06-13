// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core.Models;

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
