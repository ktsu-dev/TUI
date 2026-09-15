// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Core.Contracts;

using ktsu.TUI.Core.Models;

/// <summary>
/// Defines the contract for all UI elements
/// </summary>
public interface IUIElement
{
	/// <summary>
	/// Gets or sets the position of the element
	/// </summary>
	public Position Position { get; set; }

	/// <summary>
	/// Gets or sets the dimensions of the element
	/// </summary>
	public Dimensions Dimensions { get; set; }

	/// <summary>
	/// Gets or sets whether the element is visible
	/// </summary>
	public bool IsVisible { get; set; }

	/// <summary>
	/// Gets or sets the parent container
	/// </summary>
	public IUIContainer? Parent { get; set; }

	/// <summary>
	/// Event raised when the element is invalidated
	/// </summary>
	public event EventHandler? Invalidated;

	/// <summary>
	/// Renders the element using the provided console provider
	/// </summary>
	/// <param name="provider">The console provider to use for rendering</param>
	public void Render(IConsoleProvider provider);

	/// <summary>
	/// Handles input events
	/// </summary>
	/// <param name="input">The input to handle</param>
	/// <returns>True if the input was handled, false otherwise</returns>
	public bool HandleInput(InputResult input);

	/// <summary>
	/// Calculates the required dimensions for the element
	/// </summary>
	/// <returns>The calculated dimensions</returns>
	public Dimensions CalculateRequiredDimensions();

	/// <summary>
	/// Marks the element as changed since its last draw, notifies its parent, and raises the
	/// invalidated event
	/// </summary>
	/// <remarks>
	/// Rendering redraws every visible element on every pass, so this does not decide whether an
	/// element draws. It is the signal a host can use to know that something changed and that a
	/// render pass is worth running.
	/// </remarks>
	public void Invalidate();
}
