// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;

namespace ktsu.TUI.Core.Elements.Primitives;

/// <summary>
/// A UI element that displays text
/// </summary>
public class TextElement : UIElementBase
{
	private string _text = string.Empty;
	private TextStyle _style = TextStyle.Default;

	/// <summary>
	/// Gets or sets the text to display
	/// </summary>
	public string Text
	{
		get => _text;
		set
		{
			if (_text != value)
			{
				_text = value ?? string.Empty;
				Invalidate();
			}
		}
	}

	/// <summary>
	/// Gets or sets the text style
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
	/// Gets or sets the horizontal alignment
	/// </summary>
	public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;

	/// <summary>
	/// Gets or sets the vertical alignment
	/// </summary>
	public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;

	/// <summary>
	/// Gets or sets whether text wrapping is enabled
	/// </summary>
	public bool WordWrap { get; set; } = false;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextElement"/> class
	/// </summary>
	public TextElement()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TextElement"/> class with the specified text
	/// </summary>
	/// <param name="text">The text to display</param>
	public TextElement(string text)
	{
		Text = text;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TextElement"/> class with the specified text and style
	/// </summary>
	/// <param name="text">The text to display</param>
	/// <param name="style">The text style</param>
	public TextElement(string text, TextStyle style)
	{
		Text = text;
		Style = style;
	}

	/// <inheritdoc />
	protected override void OnRender(IConsoleProvider provider)
	{
		if (string.IsNullOrEmpty(Text))
			return;

		var contentArea = GetContentArea();
		var contentPosition = GetContentPosition();

		if (contentArea.IsEmpty)
			return;

		var lines = WordWrap ? WrapText(Text, contentArea.Width) : [Text];

		for (int i = 0; i < lines.Length && i < contentArea.Height; i++)
		{
			var line = lines[i];
			if (string.IsNullOrEmpty(line))
				continue;

			var x = CalculateHorizontalPosition(line, contentArea.Width, contentPosition.X);
			var y = CalculateVerticalPosition(lines.Length, contentArea.Height, contentPosition.Y) + i;

			provider.WriteAt(line, new Position(x, y), Style);
		}
	}

	/// <inheritdoc />
	protected override Dimensions OnCalculateRequiredDimensions()
	{
		if (string.IsNullOrEmpty(Text))
			return Padding.Horizontal > 0 || Padding.Vertical > 0
				? new Dimensions(Padding.Horizontal, Padding.Vertical)
				: Dimensions.Empty;

		var lines = WordWrap && Dimensions.Width > 0
			? WrapText(Text, Math.Max(1, Dimensions.Width - Padding.Horizontal))
			: [Text];

		var maxWidth = lines.Max(line => line.Length);
		var height = lines.Length;

		return new Dimensions(maxWidth, height).WithPadding(Padding);
	}

	private int CalculateHorizontalPosition(string line, int availableWidth, int baseX)
	{
		return HorizontalAlignment switch
		{
			HorizontalAlignment.Center => baseX + Math.Max(0, (availableWidth - line.Length) / 2),
			HorizontalAlignment.Right => baseX + Math.Max(0, availableWidth - line.Length),
			_ => baseX
		};
	}

	private int CalculateVerticalPosition(int totalLines, int availableHeight, int baseY)
	{
		return VerticalAlignment switch
		{
			VerticalAlignment.Center => baseY + Math.Max(0, (availableHeight - totalLines) / 2),
			VerticalAlignment.Bottom => baseY + Math.Max(0, availableHeight - totalLines),
			_ => baseY
		};
	}

	private static string[] WrapText(string text, int maxWidth)
	{
		if (maxWidth <= 0)
			return [text];

		var lines = new List<string>();
		var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		var currentLine = string.Empty;

		foreach (var word in words)
		{
			var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";

			if (testLine.Length <= maxWidth)
			{
				currentLine = testLine;
			}
			else
			{
				if (!string.IsNullOrEmpty(currentLine))
				{
					lines.Add(currentLine);
					currentLine = word;
				}
				else
				{
					// Word is longer than max width, break it
					lines.Add(word[..maxWidth]);
					currentLine = word.Length > maxWidth ? word[maxWidth..] : string.Empty;
				}
			}
		}

		if (!string.IsNullOrEmpty(currentLine))
			lines.Add(currentLine);

		return lines.ToArray();
	}
}

/// <summary>
/// Defines horizontal alignment options
/// </summary>
public enum HorizontalAlignment
{
	/// <summary>
	/// Align to the left
	/// </summary>
	Left,

	/// <summary>
	/// Align to the center
	/// </summary>
	Center,

	/// <summary>
	/// Align to the right
	/// </summary>
	Right
}

/// <summary>
/// Defines vertical alignment options
/// </summary>
public enum VerticalAlignment
{
	/// <summary>
	/// Align to the top
	/// </summary>
	Top,

	/// <summary>
	/// Align to the center
	/// </summary>
	Center,

	/// <summary>
	/// Align to the bottom
	/// </summary>
	Bottom
}
