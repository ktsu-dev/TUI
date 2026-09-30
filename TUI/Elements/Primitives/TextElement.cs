// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Core.Elements.Primitives;

using System.Globalization;
using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;
using Spectre.Console;

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

		string[] lines = SplitIntoLines(Text, WordWrap ? contentArea.Width : 0);

		for (int i = 0; i < lines.Length && i < contentArea.Height; i++)
		{
			string line = lines[i];
			if (string.IsNullOrEmpty(line))
			{
				continue;
			}

			int x = CalculateHorizontalPosition(line, contentArea.Width, contentPosition.X);
			int y = CalculateVerticalPosition(lines.Length, contentArea.Height, contentPosition.Y) + i;

			// Clip after the alignment offset, so nothing is drawn past the right edge of the
			// content area whichever way the line is aligned (ktsu-dev/TUI#134). The clip counts
			// terminal cells, not chars, so wide text cannot overrun it (ktsu-dev/TUI#141)
			int visibleWidth = contentPosition.X + contentArea.Width - x;
			if (visibleWidth <= 0)
			{
				continue;
			}

			line = TakeCells(line, visibleWidth);
			if (line.Length == 0)
			{
				continue;
			}

			provider.WriteAt(line, new Position(x, y), Style);
		}
	}

	/// <inheritdoc />
	protected override Dimensions OnCalculateRequiredDimensions() => OnCalculateRequiredDimensions(Dimensions.Width);

	/// <inheritdoc />
	/// <remarks>
	/// Wrapped text is measured against <paramref name="availableWidth"/> rather than the element's
	/// current width, which is still zero before its first arrange and would report the unwrapped
	/// text as a single line (ktsu-dev/TUI#131).
	/// </remarks>
	protected override Dimensions OnCalculateRequiredDimensions(int availableWidth)
	{
		if (string.IsNullOrEmpty(Text))
		{
			return Padding.Horizontal > 0 || Padding.Vertical > 0
				? new Dimensions(Padding.Horizontal, Padding.Vertical)
				: Dimensions.Empty;
		}

		string[] lines = SplitIntoLines(
			Text,
			WordWrap && availableWidth > 0 ? Math.Max(1, availableWidth - Padding.Horizontal) : 0);

		int maxWidth = lines.Max(MeasureCells);
		int height = lines.Length;

		return new Dimensions(maxWidth, height).WithPadding(Padding);
	}

	private int CalculateHorizontalPosition(string line, int availableWidth, int baseX)
	{
		int lineWidth = MeasureCells(line);
		return HorizontalAlignment switch
		{
			HorizontalAlignment.Center => baseX + Math.Max(0, (availableWidth - lineWidth) / 2),
			HorizontalAlignment.Right => baseX + Math.Max(0, availableWidth - lineWidth),
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

	/// <summary>
	/// Breaks text into the lines that are measured and drawn: one per embedded line break, each
	/// then wrapped to <paramref name="wrapWidth"/> when it is positive (ktsu-dev/TUI#135)
	/// </summary>
	/// <param name="text">The text to break</param>
	/// <param name="wrapWidth">The width to wrap each line to, or 0 to leave lines unwrapped</param>
	/// <returns>The lines, in order; a blank line in the text stays as an empty line</returns>
	private static string[] SplitIntoLines(string text, int wrapWidth)
	{
		string[] paragraphs = text.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
		if (wrapWidth <= 0)
		{
			return paragraphs;
		}

		// A blank paragraph wraps to no lines at all, so keep its row as one empty line
		return [.. paragraphs.SelectMany(paragraph =>
		{
			string[] wrapped = WrapText(paragraph, wrapWidth);
			return wrapped.Length == 0 ? [string.Empty] : wrapped;
		})];
	}

	/// <summary>
	/// Wraps one paragraph to <paramref name="maxWidth"/> cells. Spaces are kept as written,
	/// including leading indentation and runs between words, and are dropped only where a line
	/// breaks; a paragraph that already fits comes back unchanged (ktsu-dev/TUI#150).
	/// </summary>
	/// <param name="text">The paragraph to wrap, with no line breaks in it</param>
	/// <param name="maxWidth">The width to wrap to, in cells</param>
	/// <returns>The wrapped lines, in order</returns>
	private static string[] WrapText(string text, int maxWidth)
	{
		if (maxWidth <= 0 || MeasureCells(text) <= maxWidth)
		{
			return [text];
		}

		List<string> lines = [];
		string currentLine = string.Empty;
		string pendingSpaces = string.Empty;

		foreach (string word in SplitKeepingSpaces(text))
		{
			if (word[0] == ' ')
			{
				// Spaces at the start of a wrapped line are what was left over from the break
				if (currentLine.Length > 0 || lines.Count == 0)
				{
					pendingSpaces = word;
				}

				continue;
			}

			string testLine = currentLine + pendingSpaces + word;
			pendingSpaces = string.Empty;

			if (MeasureCells(testLine) <= maxWidth)
			{
				currentLine = testLine;
				continue;
			}

			// The word does not fit alongside what is already buffered, so flush that first.
			if (!string.IsNullOrEmpty(currentLine))
			{
				lines.Add(currentLine);
			}

			currentLine = BreakOverlongWord(word, maxWidth, lines);
		}

		if (!string.IsNullOrEmpty(currentLine))
		{
			lines.Add(currentLine);
		}

		return [.. lines];
	}

	/// <summary>
	/// Adds full-width slices of a word longer than <paramref name="maxWidth"/> to
	/// <paramref name="lines"/>, and returns the tail that fits, to start the next line with
	/// </summary>
	/// <param name="word">The word to break</param>
	/// <param name="maxWidth">The width to break it to, in cells</param>
	/// <param name="lines">The lines to add the full slices to</param>
	/// <returns>What is left of the word once it fits</returns>
	private static string BreakOverlongWord(string word, int maxWidth, List<string> lines)
	{
		// A word longer than max width has to be broken, and one slice is not enough:
		// keep slicing until what is left actually fits, or the tail overflows the line.
		string remainder = word;
		while (MeasureCells(remainder) > maxWidth)
		{
			string slice = TakeCells(remainder, maxWidth);
			if (slice.Length == 0)
			{
				// A single character wider than the whole line still has to go somewhere,
				// or the loop would never shrink the remainder.
				slice = StringInfo.GetNextTextElement(remainder);
			}

			lines.Add(slice);
			remainder = remainder[slice.Length..];
		}

		return remainder;
	}

	/// <summary>
	/// Splits text into alternating runs of spaces and runs of everything else, so that joining
	/// the pieces gives back the original text
	/// </summary>
	/// <param name="text">The text to split</param>
	/// <returns>The runs, in order; none is empty</returns>
	private static IEnumerable<string> SplitKeepingSpaces(string text)
	{
		int start = 0;
		for (int i = 1; i <= text.Length; i++)
		{
			bool runIsSpaces = text[start] == ' ';
			if (i == text.Length || text[i] == ' ' != runIsSpaces)
			{
				yield return text[start..i];
				start = i;
			}
		}
	}

	/// <summary>
	/// Measures text in terminal cells, the unit the console lays it out in. A <see cref="string.Length"/>
	/// count is wrong for wide characters such as CJK ideographs, which take two cells for one char,
	/// and for characters outside the BMP, which take two chars (ktsu-dev/TUI#141).
	/// </summary>
	/// <param name="text">The text to measure.</param>
	/// <returns>The number of cells the text occupies.</returns>
	internal static int MeasureCells(string text)
	{
		int cells = 0;
		TextElementEnumerator elements = StringInfo.GetTextElementEnumerator(text);
		while (elements.MoveNext())
		{
			cells += elements.GetTextElement().GetCellWidth();
		}

		return cells;
	}

	/// <summary>
	/// Returns the longest prefix of <paramref name="text"/> that fits in <paramref name="maxCells"/>
	/// terminal cells, cutting only between text elements so a surrogate pair or combining sequence is
	/// never split. A wide character that would only half fit is dropped rather than overflowing.
	/// </summary>
	/// <param name="text">The text to cut.</param>
	/// <param name="maxCells">The number of cells available.</param>
	/// <returns>The prefix that fits, which may be empty.</returns>
	internal static string TakeCells(string text, int maxCells)
	{
		int cells = 0;
		TextElementEnumerator elements = StringInfo.GetTextElementEnumerator(text);
		while (elements.MoveNext())
		{
			int width = elements.GetTextElement().GetCellWidth();
			if (cells + width > maxCells)
			{
				return text[..elements.ElementIndex];
			}

			cells += width;
		}

		return text;
	}
}
