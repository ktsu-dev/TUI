// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.TUI.Core.Models;

namespace ktsu.TUI.Core.Contracts;

/// <summary>
/// Defines the contract for all UI elements
/// </summary>
public interface IUIElement
{
	/// <summary>
	/// Gets or sets the position of the element
	/// </summary>
	Position Position { get; set; }

	/// <summary>
	/// Gets or sets the dimensions of the element
	/// </summary>
	Dimensions Dimensions { get; set; }

	/// <summary>
	/// Gets or sets whether the element is visible
	/// </summary>
	bool IsVisible { get; set; }

	/// <summary>
	/// Gets or sets the parent container
	/// </summary>
	IUIContainer? Parent { get; set; }

	/// <summary>
	/// Renders the element using the provided console provider
	/// </summary>
	/// <param name="provider">The console provider to use for rendering</param>
	void Render(IConsoleProvider provider);

	/// <summary>
	/// Handles input events
	/// </summary>
	/// <param name="input">The input to handle</param>
	/// <returns>True if the input was handled, false otherwise</returns>
	bool HandleInput(InputResult input);

	/// <summary>
	/// Calculates the required dimensions for the element
	/// </summary>
	/// <returns>The calculated dimensions</returns>
	Dimensions CalculateRequiredDimensions();

	/// <summary>
	/// Invalidates the element, marking it for re-rendering
	/// </summary>
	void Invalidate();
}
