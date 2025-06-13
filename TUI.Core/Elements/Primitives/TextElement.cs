// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;

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
	public bool WordWrap { get; set; }

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
	public TextElement(string text) => Text = text;

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
		{
			return;
		}

		Dimensions contentArea = GetContentArea();
		Position contentPosition = GetContentPosition();

		if (contentArea.IsEmpty)
		{
			return;
		}

		string[] lines = WordWrap ? WrapText(Text, contentArea.Width) : [Text];

		for (int i = 0; i < lines.Length && i < contentArea.Height; i++)
		{
			string line = lines[i];
			if (string.IsNullOrEmpty(line))
			{
				continue;
			}

			int x = CalculateHorizontalPosition(line, contentArea.Width, contentPosition.X);
			int y = CalculateVerticalPosition(lines.Length, contentArea.Height, contentPosition.Y) + i;

			provider.WriteAt(line, new Position(x, y), Style);
		}
	}

	/// <inheritdoc />
	protected override Dimensions OnCalculateRequiredDimensions()
	{
		if (string.IsNullOrEmpty(Text))
		{
			return Padding.Horizontal > 0 || Padding.Vertical > 0
				? new Dimensions(Padding.Horizontal, Padding.Vertical)
				: Dimensions.Empty;
		}

		string[] lines = WordWrap && Dimensions.Width > 0
			? WrapText(Text, Math.Max(1, Dimensions.Width - Padding.Horizontal))
			: [Text];

		int maxWidth = lines.Max(line => line.Length);
		int height = lines.Length;

		return new Dimensions(maxWidth, height).WithPadding(Padding);
	}

	private int CalculateHorizontalPosition(string line, int availableWidth, int baseX)
	{
		return HorizontalAlignment switch
		{
			HorizontalAlignment.Center => baseX + Math.Max(0, (availableWidth - line.Length) / 2),
			HorizontalAlignment.Right => baseX + Math.Max(0, availableWidth - line.Length),
			HorizontalAlignment.Left => throw new NotImplementedException(),
			_ => baseX
		};
	}

	private int CalculateVerticalPosition(int totalLines, int availableHeight, int baseY)
	{
		return VerticalAlignment switch
		{
			VerticalAlignment.Center => baseY + Math.Max(0, (availableHeight - totalLines) / 2),
			VerticalAlignment.Bottom => baseY + Math.Max(0, availableHeight - totalLines),
			VerticalAlignment.Top => throw new NotImplementedException(),
			_ => baseY
		};
	}

	private static string[] WrapText(string text, int maxWidth)
	{
		if (maxWidth <= 0)
		{
			return [text];
		}

		List<string> lines = [];
		string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		string currentLine = string.Empty;

		foreach (string word in words)
		{
			string testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";

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
		{
			lines.Add(currentLine);
		}

		return [.. lines];
	}
}
