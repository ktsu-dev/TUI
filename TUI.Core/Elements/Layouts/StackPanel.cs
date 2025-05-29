// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;

namespace ktsu.TUI.Core.Elements.Layouts;

/// <summary>
/// A container that arranges child elements in a stack (horizontal or vertical)
/// </summary>
public class StackPanel : UIContainerBase
{
	private Orientation _orientation = Orientation.Vertical;
	private int _spacing = 0;

	/// <summary>
	/// Gets or sets the orientation of the stack
	/// </summary>
	public Orientation Orientation
	{
		get => _orientation;
		set
		{
			if (_orientation != value)
			{
				_orientation = value;
				ArrangeChildren();
				Invalidate();
			}
		}
	}

	/// <summary>
	/// Gets or sets the spacing between child elements
	/// </summary>
	public int Spacing
	{
		get => _spacing;
		set
		{
			if (_spacing != value)
			{
				_spacing = Math.Max(0, value);
				ArrangeChildren();
				Invalidate();
			}
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="StackPanel"/> class
	/// </summary>
	public StackPanel()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="StackPanel"/> class with the specified orientation
	/// </summary>
	/// <param name="orientation">The stack orientation</param>
	public StackPanel(Orientation orientation)
	{
		Orientation = orientation;
	}

	/// <inheritdoc />
	protected override void OnRender(IConsoleProvider provider)
	{
		// StackPanel itself doesn't render anything, just arranges children
	}

	/// <inheritdoc />
	protected override void OnArrangeChildren()
	{
		var contentArea = GetContentArea();
		var contentPosition = GetContentPosition();

		if (contentArea.IsEmpty || Children.Count == 0)
			return;

		var currentOffset = 0;

		foreach (var child in GetVisibleChildren())
		{
			var childDimensions = child.CalculateRequiredDimensions();

			if (Orientation == Orientation.Vertical)
			{
				// Vertical stacking
				child.Position = contentPosition.Offset(0, currentOffset);
				child.Dimensions = new Dimensions(
					Math.Min(childDimensions.Width, contentArea.Width),
					Math.Min(childDimensions.Height, contentArea.Height - currentOffset)
				);

				currentOffset += child.Dimensions.Height + Spacing;

				// Stop if we've run out of vertical space
				if (currentOffset >= contentArea.Height)
					break;
			}
			else
			{
				// Horizontal stacking
				child.Position = contentPosition.Offset(currentOffset, 0);
				child.Dimensions = new Dimensions(
					Math.Min(childDimensions.Width, contentArea.Width - currentOffset),
					Math.Min(childDimensions.Height, contentArea.Height)
				);

				currentOffset += child.Dimensions.Width + Spacing;

				// Stop if we've run out of horizontal space
				if (currentOffset >= contentArea.Width)
					break;
			}
		}
	}

	/// <inheritdoc />
	protected override Dimensions OnCalculateRequiredDimensionsForChildren()
	{
		if (Children.Count == 0)
			return Dimensions.Empty;

		var totalWidth = 0;
		var totalHeight = 0;
		var maxWidth = 0;
		var maxHeight = 0;

		var visibleChildren = GetVisibleChildren().ToArray();
		var totalSpacing = Math.Max(0, (visibleChildren.Length - 1) * Spacing);

		foreach (var child in visibleChildren)
		{
			var childDimensions = child.CalculateRequiredDimensions();

			if (Orientation == Orientation.Vertical)
			{
				totalHeight += childDimensions.Height;
				maxWidth = Math.Max(maxWidth, childDimensions.Width);
			}
			else
			{
				totalWidth += childDimensions.Width;
				maxHeight = Math.Max(maxHeight, childDimensions.Height);
			}
		}

		return Orientation == Orientation.Vertical
			? new Dimensions(maxWidth, totalHeight + totalSpacing)
			: new Dimensions(totalWidth + totalSpacing, maxHeight);
	}
}

/// <summary>
/// Defines orientation options for layout containers
/// </summary>
public enum Orientation
{
	/// <summary>
	/// Horizontal orientation (left to right)
	/// </summary>
	Horizontal,

	/// <summary>
	/// Vertical orientation (top to bottom)
	/// </summary>
	Vertical
}
