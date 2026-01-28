// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core.Models;

/// <summary>
/// Represents dimensions (width and height)
/// </summary>
/// <param name="Width">The width</param>
/// <param name="Height">The height</param>
public readonly record struct Dimensions(int Width, int Height)
{
	/// <summary>
	/// Gets empty dimensions (0, 0)
	/// </summary>
	public static Dimensions Empty => new(0, 0);

	/// <summary>
	/// Gets the area (width * height)
	/// </summary>
	public int Area => Width * Height;

	/// <summary>
	/// Gets whether the dimensions are empty (width or height is 0)
	/// </summary>
	public bool IsEmpty => Width <= 0 || Height <= 0;

	/// <summary>
	/// Adds two dimensions together
	/// </summary>
	/// <param name="a">First dimensions</param>
	/// <param name="b">Second dimensions</param>
	/// <returns>The sum of the dimensions</returns>
	public static Dimensions operator +(Dimensions a, Dimensions b) => Add(a, b);

	/// <summary>
	/// Subtracts one dimensions from another
	/// </summary>
	/// <param name="a">First dimensions</param>
	/// <param name="b">Second dimensions</param>
	/// <returns>The difference of the dimensions</returns>
	public static Dimensions operator -(Dimensions a, Dimensions b) => Subtract(a, b);

	/// <summary>
	/// Adds two dimensions together
	/// </summary>
	/// <param name="a">First dimensions</param>
	/// <param name="b">Second dimensions</param>
	/// <returns>The sum of the dimensions</returns>
	public static Dimensions Add(Dimensions a, Dimensions b) => new(a.Width + b.Width, a.Height + b.Height);

	/// <summary>
	/// Subtracts one dimensions from another
	/// </summary>
	/// <param name="a">First dimensions</param>
	/// <param name="b">Second dimensions</param>
	/// <returns>The difference of the dimensions</returns>
	public static Dimensions Subtract(Dimensions a, Dimensions b) => new(a.Width - b.Width, a.Height - b.Height);

	/// <summary>
	/// Creates new dimensions with the specified padding
	/// </summary>
	/// <param name="padding">The padding to apply</param>
	/// <returns>The padded dimensions</returns>
	public Dimensions WithPadding(Padding padding) => new(Width + padding.Horizontal, Height + padding.Vertical);

	/// <summary>
	/// Creates new dimensions without the specified padding
	/// </summary>
	/// <param name="padding">The padding to remove</param>
	/// <returns>The unpadded dimensions</returns>
	public Dimensions WithoutPadding(Padding padding) => new(Math.Max(0, Width - padding.Horizontal), Math.Max(0, Height - padding.Vertical));
}
