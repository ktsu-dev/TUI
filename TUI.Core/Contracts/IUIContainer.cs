// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core.Contracts;

/// <summary>
/// Defines the contract for UI containers that can hold child elements
/// </summary>
public interface IUIContainer : IUIElement
{
	/// <summary>
	/// Gets the collection of child elements
	/// </summary>
	IReadOnlyCollection<IUIElement> Children { get; }

	/// <summary>
	/// Adds a child element to the container
	/// </summary>
	/// <param name="child">The child element to add</param>
	void AddChild(IUIElement child);

	/// <summary>
	/// Removes a child element from the container
	/// </summary>
	/// <param name="child">The child element to remove</param>
	/// <returns>True if the child was removed, false otherwise</returns>
	bool RemoveChild(IUIElement child);

	/// <summary>
	/// Clears all child elements from the container
	/// </summary>
	void ClearChildren();

	/// <summary>
	/// Arranges the child elements within the container
	/// </summary>
	void ArrangeChildren();
}
