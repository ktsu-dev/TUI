// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core.Models;

/// <summary>
/// Represents padding around an element
/// </summary>
/// <param name="Left">Left padding</param>
/// <param name="Top">Top padding</param>
/// <param name="Right">Right padding</param>
/// <param name="Bottom">Bottom padding</param>
public readonly record struct Padding(int Left, int Top, int Right, int Bottom)
{
	/// <summary>
	/// Gets no padding (0, 0, 0, 0)
	/// </summary>
	public static Padding None => new(0, 0, 0, 0);

	/// <summary>
	/// Creates uniform padding with the same value for all sides
	/// </summary>
	/// <param name="value">The padding value for all sides</param>
	/// <returns>The uniform padding</returns>
	public static Padding Uniform(int value) => new(value, value, value, value);

	/// <summary>
	/// Creates horizontal padding (left and right only)
	/// </summary>
	/// <param name="horizontal">The horizontal padding value</param>
	/// <returns>The horizontal padding</returns>
	public static Padding CreateHorizontal(int horizontal) => new(horizontal, 0, horizontal, 0);

	/// <summary>
	/// Creates vertical padding (top and bottom only)
	/// </summary>
	/// <param name="vertical">The vertical padding value</param>
	/// <returns>The vertical padding</returns>
	public static Padding CreateVertical(int vertical) => new(0, vertical, 0, vertical);

	/// <summary>
	/// Gets the total horizontal padding (left + right)
	/// </summary>
	public int Horizontal => Left + Right;

	/// <summary>
	/// Gets the total vertical padding (top + bottom)
	/// </summary>
	public int Vertical => Top + Bottom;
}
