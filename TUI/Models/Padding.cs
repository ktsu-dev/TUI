// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Core.Models;

/// <summary>
/// Represents padding around an element
/// </summary>
/// <remarks>
/// A negative side is treated as 0, as <c>StackPanel.Spacing</c> treats a negative spacing.
/// Padding only ever moves content inward: a negative side would place content outside the slot
/// its parent arranged, over its siblings, and make <see cref="Dimensions.WithPadding"/> and
/// <see cref="Dimensions.WithoutPadding"/> report a size that does not fit the element. A computed
/// padding such as <c>width - contentWidth</c> goes negative whenever the content is the larger, so
/// it is clamped rather than refused.
/// </remarks>
/// <param name="Left">Left padding; a negative value is treated as 0</param>
/// <param name="Top">Top padding; a negative value is treated as 0</param>
/// <param name="Right">Right padding; a negative value is treated as 0</param>
/// <param name="Bottom">Bottom padding; a negative value is treated as 0</param>
public readonly record struct Padding(int Left, int Top, int Right, int Bottom)
{
	/// <summary>
	/// Gets the left padding, never negative
	/// </summary>
	public int Left { get; init => field = Math.Max(0, value); } = Math.Max(0, Left);

	/// <summary>
	/// Gets the top padding, never negative
	/// </summary>
	public int Top { get; init => field = Math.Max(0, value); } = Math.Max(0, Top);

	/// <summary>
	/// Gets the right padding, never negative
	/// </summary>
	public int Right { get; init => field = Math.Max(0, value); } = Math.Max(0, Right);

	/// <summary>
	/// Gets the bottom padding, never negative
	/// </summary>
	public int Bottom { get; init => field = Math.Max(0, value); } = Math.Max(0, Bottom);

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
