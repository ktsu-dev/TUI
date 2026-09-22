// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Core.Elements.Primitives;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;

/// <summary>
/// A UI element that displays text
/// </summary>
public class TextElement : UIElementBase
{
	/// <summary>
	/// Gets or sets the text to display
	/// </summary>
	public string Text
	{
		get;
		set
		{
			if (field != value)
			{
				field = value ?? string.Empty;
				Invalidate();
			}
		}
	} = string.Empty;

	/// <summary>
	/// Gets or sets the text style
	/// </summary>
	public TextStyle Style
	{
		get;
		set
		{
			if (field != value)
			{
				field = value;
				Invalidate();
			}
		}
	} = TextStyle.Default;

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
		Ensure.NotNull(provider);

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
			HorizontalAlignment.Left => baseX,
			_ => baseX
		};
	}

	private int CalculateVerticalPosition(int totalLines, int availableHeight, int baseY)
	{
		return VerticalAlignment switch
		{
			VerticalAlignment.Center => baseY + Math.Max(0, (availableHeight - totalLines) / 2),
			VerticalAlignment.Bottom => baseY + Math.Max(0, availableHeight - totalLines),
			VerticalAlignment.Top => baseY,
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
				continue;
			}

			// The word does not fit alongside what is already buffered, so flush that first.
			if (!string.IsNullOrEmpty(currentLine))
			{
				lines.Add(currentLine);
			}

			// A word longer than max width has to be broken, and one slice is not enough:
			// keep slicing until what is left actually fits, or the tail overflows the line.
			string remainder = word;
			while (remainder.Length > maxWidth)
			{
				lines.Add(remainder[..maxWidth]);
				remainder = remainder[maxWidth..];
			}

			currentLine = remainder;
		}

		if (!string.IsNullOrEmpty(currentLine))
		{
			lines.Add(currentLine);
		}

		return [.. lines];
	}
}
