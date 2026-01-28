// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core.Models;

/// <summary>
/// Represents a position in 2D space
/// </summary>
/// <param name="X">The X coordinate</param>
/// <param name="Y">The Y coordinate</param>
public readonly record struct Position(int X, int Y)
{
	/// <summary>
	/// Gets a position at the origin (0, 0)
	/// </summary>
	public static Position Origin => new(0, 0);

	/// <summary>
	/// Adds two positions together
	/// </summary>
	/// <param name="a">First position</param>
	/// <param name="b">Second position</param>
	/// <returns>The sum of the positions</returns>
	public static Position operator +(Position a, Position b) => Add(a, b);

	/// <summary>
	/// Subtracts one position from another
	/// </summary>
	/// <param name="a">First position</param>
	/// <param name="b">Second position</param>
	/// <returns>The difference of the positions</returns>
	public static Position operator -(Position a, Position b) => Subtract(a, b);

	/// <summary>
	/// Adds two positions together
	/// </summary>
	/// <param name="a">First position</param>
	/// <param name="b">Second position</param>
	/// <returns>The sum of the positions</returns>
	public static Position Add(Position a, Position b) => new(a.X + b.X, a.Y + b.Y);

	/// <summary>
	/// Subtracts one position from another
	/// </summary>
	/// <param name="a">First position</param>
	/// <param name="b">Second position</param>
	/// <returns>The difference of the positions</returns>
	public static Position Subtract(Position a, Position b) => new(a.X - b.X, a.Y - b.Y);

	/// <summary>
	/// Creates a new position offset by the specified amounts
	/// </summary>
	/// <param name="offsetX">X offset</param>
	/// <param name="offsetY">Y offset</param>
	/// <returns>The offset position</returns>
	public Position Offset(int offsetX, int offsetY) => new(X + offsetX, Y + offsetY);
}
