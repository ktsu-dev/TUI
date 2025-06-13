// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;

/// <summary>
/// A UI element that draws a border
/// </summary>
public class BorderElement : UIContainerBase
{
	private BorderStyle _borderStyle = BorderStyle.SingleLine;
	private TextStyle _style = TextStyle.Default;
	private string _title = string.Empty;
	private HorizontalAlignment _titleAlignment = HorizontalAlignment.Left;

	/// <summary>
	/// Gets or sets the border style
	/// </summary>
	public BorderStyle BorderStyle
	{
		get => _borderStyle;
		set
		{
			if (_borderStyle != value)
			{
				_borderStyle = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// Gets or sets the border style (color, etc.)
	/// </summary>
	public TextStyle Style
	{
		get => _style;
		set
		{
			if (_style != value)
			{
				_style = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// Gets or sets the title to display in the border
	/// </summary>
	public string Title
	{
		get => _title;
		set
		{
			if (_title != value)
			{
				_title = value ?? string.Empty;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// Gets or sets the title alignment
	/// </summary>
	public HorizontalAlignment TitleAlignment
	{
		get => _titleAlignment;
		set
		{
			if (_titleAlignment != value)
			{
				_titleAlignment = value;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// Gets or sets the child element (convenience property for single child)
	/// </summary>
	public IUIElement? Child
	{
		get => Children.FirstOrDefault();
		set
		{
			ClearChildren();
			if (value != null)
			{
				AddChild(value);
			}
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="BorderElement"/> class
	/// </summary>
	public BorderElement() =>
		// Border elements need at least 2x2 to be visible
		Padding = new Padding(1, 1, 1, 1);

	/// <summary>
	/// Initializes a new instance of the <see cref="BorderElement"/> class with the specified style
	/// </summary>
	/// <param name="borderStyle">The border style</param>
	public BorderElement(BorderStyle borderStyle) : this() => BorderStyle = borderStyle;

	/// <inheritdoc />
	protected override void OnRender(IConsoleProvider provider)
	{
		ArgumentNullException.ThrowIfNull(provider);

		Dimensions dimensions = Dimensions;
		Position position = Position;

		if (dimensions.Width < 2 || dimensions.Height < 2)
		{
			return;
		}

		BorderCharacters chars = GetBorderCharacters(BorderStyle);

		// Draw corners
		provider.WriteAt(chars.TopLeft.ToString(), position, Style);
		provider.WriteAt(chars.TopRight.ToString(), position.Offset(dimensions.Width - 1, 0), Style);
		provider.WriteAt(chars.BottomLeft.ToString(), position.Offset(0, dimensions.Height - 1), Style);
		provider.WriteAt(chars.BottomRight.ToString(), position.Offset(dimensions.Width - 1, dimensions.Height - 1), Style);

		// Draw horizontal lines
		string horizontalLine = new(chars.Horizontal, dimensions.Width - 2);
		if (horizontalLine.Length > 0)
		{
			provider.WriteAt(horizontalLine, position.Offset(1, 0), Style);
			provider.WriteAt(horizontalLine, position.Offset(1, dimensions.Height - 1), Style);
		}

		// Draw vertical lines
		for (int y = 1; y < dimensions.Height - 1; y++)
		{
			provider.WriteAt(chars.Vertical.ToString(), position.Offset(0, y), Style);
			provider.WriteAt(chars.Vertical.ToString(), position.Offset(dimensions.Width - 1, y), Style);
		}

		// Draw title if present
		if (!string.IsNullOrEmpty(Title) && dimensions.Width > 4)
		{
			int maxTitleWidth = dimensions.Width - 4; // Leave space for border and padding
			string displayTitle = Title.Length > maxTitleWidth ? Title[..maxTitleWidth] : Title;
			string titleWithPadding = $" {displayTitle} ";

			int titleX = TitleAlignment switch
			{
				HorizontalAlignment.Center => position.X + Math.Max(1, (dimensions.Width - titleWithPadding.Length) / 2),
				HorizontalAlignment.Right => position.X + Math.Max(1, dimensions.Width - titleWithPadding.Length - 1),
				HorizontalAlignment.Left => throw new NotImplementedException(),
				_ => position.X + 1
			};

			provider.WriteAt(titleWithPadding, new Position(titleX, position.Y), Style);
		}
	}

	/// <inheritdoc />
	protected override void OnArrangeChildren()
	{
		Dimensions contentArea = GetContentArea();
		Position contentPosition = GetContentPosition();

		// For a simple border, we'll just position the first child to fill the content area
		if (Children.Count > 0)
		{
			IUIElement child = Children.First();
			child.Position = contentPosition;
			child.Dimensions = contentArea;
		}
	}

	/// <inheritdoc />
	protected override Dimensions OnCalculateRequiredDimensionsForChildren()
	{
		if (Children.Count == 0)
		{
			return new Dimensions(2, 2); // Minimum size for border
		}

		Dimensions childDimensions = Children.First().CalculateRequiredDimensions();
		return new Dimensions(
			Math.Max(2, childDimensions.Width + 2), // +2 for left and right border
			Math.Max(2, childDimensions.Height + 2)  // +2 for top and bottom border
		);
	}

	private static BorderCharacters GetBorderCharacters(BorderStyle style)
	{
		return style switch
		{
			BorderStyle.DoubleLine => new BorderCharacters('╔', '╗', '╚', '╝', '═', '║'),
			BorderStyle.Rounded => new BorderCharacters('╭', '╮', '╰', '╯', '─', '│'),
			BorderStyle.Thick => new BorderCharacters('┏', '┓', '┗', '┛', '━', '┃'),
			BorderStyle.Ascii => new BorderCharacters('+', '+', '+', '+', '-', '|'),
			BorderStyle.SingleLine => new BorderCharacters('┌', '┐', '└', '┘', '─', '│'),
			BorderStyle.None => throw new NotImplementedException(),
			_ => new BorderCharacters('┌', '┐', '└', '┘', '─', '│') // SingleLine
		};
	}

	private readonly record struct BorderCharacters(
		char TopLeft,
		char TopRight,
		char BottomLeft,
		char BottomRight,
		char Horizontal,
		char Vertical);
}
