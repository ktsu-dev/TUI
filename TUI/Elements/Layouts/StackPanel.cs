// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Core.Elements.Layouts;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;

/// <summary>
/// A container that arranges child elements in a stack (horizontal or vertical)
/// </summary>
public class StackPanel : UIContainerBase
{
	/// <summary>
	/// Gets or sets the orientation of the stack
	/// </summary>
	public Orientation Orientation
	{
		get;
		set
		{
			if (field != value)
			{
				field = value;
				ArrangeChildren();
				Invalidate();
			}
		}
	} = Orientation.Vertical;

	/// <summary>
	/// Gets or sets the spacing between child elements
	/// </summary>
	public int Spacing
	{
		get;
		set
		{
			if (field != value)
			{
				field = Math.Max(0, value);
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
	public StackPanel(Orientation orientation) => Orientation = orientation;

	/// <inheritdoc />
	protected override void OnRender(IConsoleProvider provider)
	{
		// StackPanel itself doesn't render anything, just arranges children
	}

	/// <inheritdoc />
	protected override void OnArrangeChildren()
	{
		Dimensions contentArea = GetContentArea();
		Position contentPosition = GetContentPosition();

		if (Children.Count == 0)
		{
			return;
		}

		// A child left out of this pass would keep the geometry of an earlier, larger arrange, and
		// Render draws every visible child, so it would be drawn outside the panel. Children that
		// get no space are therefore given none (ktsu-dev/TUI#140).
		List<IUIElement> unplaced = [.. GetVisibleChildren()];

		if (contentArea.IsEmpty)
		{
			CollapseAll(unplaced);
			return;
		}

		int currentOffset = 0;

		foreach (IUIElement child in GetVisibleChildren())
		{
			unplaced.Remove(child);

			// A child is measured against the width it can have: the panel's width when stacking
			// vertically, and the width not yet taken when stacking horizontally. Word-wrapped text
			// needs this to report one row per wrapped line (ktsu-dev/TUI#131, ktsu-dev/TUI#161).
			Dimensions childDimensions = Orientation == Orientation.Vertical
				? child.CalculateRequiredDimensions(contentArea.Width)
				: child.CalculateRequiredDimensions(contentArea.Width - currentOffset);

			if (Orientation == Orientation.Vertical)
			{
				// Vertical stacking. The child spans the panel's width rather than its own measured
				// width, so it has room to apply its own horizontal alignment (ktsu-dev/TUI#158).
				child.Position = contentPosition.Offset(0, currentOffset);
				child.Dimensions = new Dimensions(
					contentArea.Width,
					Math.Min(childDimensions.Height, contentArea.Height - currentOffset)
				);

				currentOffset += child.Dimensions.Height + Spacing;

				// Stop if we've run out of vertical space
				if (currentOffset >= contentArea.Height)
				{
					break;
				}
			}
			else
			{
				// Horizontal stacking. The child spans the panel's height, matching the vertical case.
				child.Position = contentPosition.Offset(currentOffset, 0);
				child.Dimensions = new Dimensions(
					Math.Min(childDimensions.Width, contentArea.Width - currentOffset),
					contentArea.Height
				);

				currentOffset += child.Dimensions.Width + Spacing;

				// Stop if we've run out of horizontal space
				if (currentOffset >= contentArea.Width)
				{
					break;
				}
			}
		}

		CollapseAll(unplaced);
	}

	private static void CollapseAll(IEnumerable<IUIElement> children)
	{
		foreach (IUIElement child in children)
		{
			child.Dimensions = Dimensions.Empty;
		}
	}

	/// <inheritdoc />
	protected override Dimensions OnCalculateRequiredDimensionsForChildren() => CalculateRequiredDimensionsForChildren(availableWidth: null);

	/// <inheritdoc />
	protected override Dimensions OnCalculateRequiredDimensionsForChildren(int availableWidth) => CalculateRequiredDimensionsForChildren(availableWidth);

	private Dimensions CalculateRequiredDimensionsForChildren(int? availableWidth)
	{
		if (Children.Count == 0)
		{
			return Dimensions.Empty;
		}

		int totalWidth = 0;
		int totalHeight = 0;
		int maxWidth = 0;
		int maxHeight = 0;

		IUIElement[] visibleChildren = [.. GetVisibleChildren()];
		int totalSpacing = Math.Max(0, (visibleChildren.Length - 1) * Spacing);
		int consumedWidth = 0;

		foreach (IUIElement? child in visibleChildren)
		{
			// A horizontal child gets the width its earlier siblings and the spacing between them
			// leave, as it does when arranged (ktsu-dev/TUI#161).
			Dimensions childDimensions = availableWidth switch
			{
				int width when Orientation == Orientation.Vertical => child.CalculateRequiredDimensions(width),
				int width => child.CalculateRequiredDimensions(Math.Max(0, width - consumedWidth)),
				_ => child.CalculateRequiredDimensions(),
			};

			if (Orientation == Orientation.Vertical)
			{
				totalHeight += childDimensions.Height;
				maxWidth = Math.Max(maxWidth, childDimensions.Width);
			}
			else
			{
				totalWidth += childDimensions.Width;
				consumedWidth += childDimensions.Width + Spacing;
				maxHeight = Math.Max(maxHeight, childDimensions.Height);
			}
		}

		return Orientation == Orientation.Vertical
			? new Dimensions(maxWidth, totalHeight + totalSpacing)
			: new Dimensions(totalWidth + totalSpacing, maxHeight);
	}
}
