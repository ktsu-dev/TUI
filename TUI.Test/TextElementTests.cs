// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for <see cref="TextElement"/>, covering the render path as well as property behaviour.
/// </summary>
/// <remarks>
/// The render cases exist because every alignment combination once threw
/// <see cref="NotImplementedException"/> from the default configuration
/// (ktsu-dev/TUI#102), and the suite was green throughout because nothing called
/// <c>Render</c>.
/// </remarks>
[TestClass]
public sealed class TextElementTests
{
	private static TextElement CreateElement(string text, int width = 40, int height = 5)
	{
		return new TextElement
		{
			Text = text,
			Position = Position.Origin,
			Dimensions = new Dimensions(width, height),
		};
	}

	/// <summary>
	/// The default configuration must render. This is the exact case that used to throw.
	/// </summary>
	[TestMethod]
	public void RenderWithDefaultAlignmentDrawsTheText()
	{
		// Arrange
		TextElement element = CreateElement("hello world");
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.AreEqual(HorizontalAlignment.Left, element.HorizontalAlignment, "Left must remain the default");
		Assert.AreEqual(VerticalAlignment.Top, element.VerticalAlignment, "Top must remain the default");
		Assert.ContainsSingle(provider.WritesOf("hello world"));
	}

	/// <summary>
	/// Every alignment combination must render rather than throw.
	/// </summary>
	/// <param name="horizontal">The horizontal alignment under test.</param>
	/// <param name="vertical">The vertical alignment under test.</param>
	[TestMethod]
	[DynamicData(nameof(AlignmentCombinations))]
	public void RenderSucceedsForEveryAlignmentCombination(HorizontalAlignment horizontal, VerticalAlignment vertical)
	{
		// Arrange
		TextElement element = CreateElement("x", width: 20, height: 4);
		element.HorizontalAlignment = horizontal;
		element.VerticalAlignment = vertical;
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.ContainsSingle(provider.WritesOf("x"));
	}

	/// <summary>
	/// Gets every horizontal/vertical alignment pairing.
	/// </summary>
	public static IEnumerable<object[]> AlignmentCombinations =>
		from horizontal in Enum.GetValues<HorizontalAlignment>()
		from vertical in Enum.GetValues<VerticalAlignment>()
		select new object[] { horizontal, vertical };

	/// <summary>
	/// Left alignment must place text at the content origin, not merely avoid throwing.
	/// </summary>
	[TestMethod]
	public void LeftAlignmentPlacesTextAtTheContentOrigin()
	{
		// Arrange
		TextElement element = CreateElement("abc", width: 20, height: 1);
		element.HorizontalAlignment = HorizontalAlignment.Left;
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.AreEqual(0, provider.WritesOf("abc").Single().Position.X);
	}

	/// <summary>
	/// Top alignment must place the first line at the content origin.
	/// </summary>
	[TestMethod]
	public void TopAlignmentPlacesTextAtTheContentOrigin()
	{
		// Arrange
		TextElement element = CreateElement("abc", width: 20, height: 5);
		element.VerticalAlignment = VerticalAlignment.Top;
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.AreEqual(0, provider.WritesOf("abc").Single().Position.Y);
	}

	/// <summary>
	/// Centre alignment must shift the text right of the left-aligned position.
	/// </summary>
	[TestMethod]
	public void CenterAlignmentShiftsTextRightOfLeftAlignment()
	{
		// Arrange
		RecordingConsoleProvider left = new();
		RecordingConsoleProvider center = new();

		TextElement leftElement = CreateElement("abc", width: 21, height: 1);
		leftElement.HorizontalAlignment = HorizontalAlignment.Left;

		TextElement centerElement = CreateElement("abc", width: 21, height: 1);
		centerElement.HorizontalAlignment = HorizontalAlignment.Center;

		// Act
		leftElement.Render(left);
		centerElement.Render(center);

		// Assert
		Assert.IsGreaterThan(
			left.WritesOf("abc").Single().Position.X,
			center.WritesOf("abc").Single().Position.X);
	}

	/// <summary>
	/// Right alignment must place the text flush against the right edge of the content area.
	/// </summary>
	[TestMethod]
	public void RightAlignmentPlacesTextAgainstTheRightEdge()
	{
		// Arrange
		TextElement element = CreateElement("abc", width: 20, height: 1);
		element.HorizontalAlignment = HorizontalAlignment.Right;
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.AreEqual(20 - 3, provider.WritesOf("abc").Single().Position.X);
	}

	/// <summary>
	/// Empty text must draw nothing at all.
	/// </summary>
	[TestMethod]
	public void RenderWithEmptyTextDrawsNothing()
	{
		// Arrange
		TextElement element = CreateElement(string.Empty);
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.IsEmpty(provider.Writes);
	}

	/// <summary>
	/// Padding must inset the drawn text on both axes.
	/// </summary>
	[TestMethod]
	public void PaddingInsetsTheDrawnText()
	{
		// Arrange
		// Height must exceed the 6 rows of vertical padding, or the content area is empty
		// and TextElement returns before drawing anything.
		TextElement element = CreateElement("abc", width: 20, height: 10);
		element.Padding = new Padding(2, 3, 2, 3);
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		RecordingConsoleProvider.Write write = provider.WritesOf("abc").Single();
		Assert.AreEqual(2, write.Position.X);
		Assert.AreEqual(3, write.Position.Y);
	}

	/// <summary>
	/// Word wrapping must split long text across several lines.
	/// </summary>
	[TestMethod]
	public void WordWrapSplitsTextAcrossLines()
	{
		// Arrange
		TextElement element = CreateElement("aaa bbb ccc ddd", width: 7, height: 5);
		element.WordWrap = true;
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.IsGreaterThan(1, provider.Writes.Count, "Wrapped text should produce more than one line");
		Assert.IsTrue(
			provider.Writes.All(w => w.Text.Length <= 7),
			"No wrapped line should exceed the content width");
	}

	/// <summary>
	/// A single word longer than twice the wrap width must be broken into as many lines as it
	/// takes, not sliced once and left overflowing (ktsu-dev/TUI#114).
	/// </summary>
	[TestMethod]
	public void WordWrapFullyBreaksAWordLongerThanTwiceTheWidth()
	{
		// Arrange
		// Ten characters into a three-wide box needs four lines; the old code sliced once and
		// emitted the seven-character remainder as a single oversized line.
		TextElement element = CreateElement("abcdefghij", width: 3, height: 10);
		element.WordWrap = true;
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.IsTrue(
			provider.Writes.All(w => w.Text.Length <= 3),
			$"No wrapped line may exceed the content width, but got [{string.Join(", ", provider.Writes.Select(w => w.Text))}]");
		Assert.AreEqual(
			"abcdefghij",
			string.Concat(provider.Writes.Select(w => w.Text)),
			"Breaking the word must not drop or reorder any of its characters");
	}

	/// <summary>
	/// A word longer than the wrap width must still be broken when an earlier word is already
	/// buffered on the current line (ktsu-dev/TUI#114).
	/// </summary>
	[TestMethod]
	public void WordWrapBreaksAnOverlongWordThatFollowsAShortOne()
	{
		// Arrange
		// The buffered "ab" is flushed first, which used to leave the overlong word to be carried
		// whole into the next line and never broken at all.
		TextElement element = CreateElement("ab abcdefghij", width: 3, height: 10);
		element.WordWrap = true;
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.IsTrue(
			provider.Writes.All(w => w.Text.Length <= 3),
			$"No wrapped line may exceed the content width, but got [{string.Join(", ", provider.Writes.Select(w => w.Text))}]");
	}

	/// <summary>
	/// A word that divides exactly into the wrap width must not leave a trailing empty line.
	/// </summary>
	[TestMethod]
	public void WordWrapLeavesNoEmptyLineWhenAWordDividesExactly()
	{
		// Arrange
		TextElement element = CreateElement("abcdef", width: 3, height: 10);
		element.WordWrap = true;
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.HasCount(2, provider.Writes, "Six characters at width three is exactly two lines");
		Assert.AreEqual("abc", provider.Writes[0].Text);
		Assert.AreEqual("def", provider.Writes[1].Text);
	}

	/// <summary>
	/// An element that is not visible must draw nothing.
	/// </summary>
	[TestMethod]
	public void InvisibleElementDrawsNothing()
	{
		// Arrange
		TextElement element = CreateElement("hello");
		element.IsVisible = false;
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.IsEmpty(provider.Writes);
	}

	/// <summary>
	/// Text must not be drawn beyond the available height.
	/// </summary>
	[TestMethod]
	public void RenderDoesNotDrawMoreLinesThanTheContentHeight()
	{
		// Arrange
		TextElement element = CreateElement("aaa bbb ccc ddd eee fff", width: 5, height: 2);
		element.WordWrap = true;
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.IsLessThanOrEqualTo(2, provider.Writes.Count);
	}

	/// <summary>
	/// An embedded line break must be measured as a second line, not as a character of the first
	/// (ktsu-dev/TUI#135).
	/// </summary>
	/// <param name="text">The text under test, with a line break in each supported style.</param>
	[TestMethod]
	[DataRow("ab\ncd")]
	[DataRow("ab\r\ncd")]
	[DataRow("ab\rcd")]
	public void EmbeddedLineBreakIsMeasuredAsTwoLines(string text)
	{
		// Arrange
		TextElement element = new(text);

		// Act
		Dimensions required = element.CalculateRequiredDimensions();

		// Assert
		Assert.AreEqual(new Dimensions(2, 2), required);
	}

	/// <summary>
	/// An embedded line break must be drawn as two writes on consecutive rows, and the break
	/// itself must never reach the console (ktsu-dev/TUI#135).
	/// </summary>
	[TestMethod]
	public void EmbeddedLineBreakRendersOnConsecutiveRows()
	{
		// Arrange
		TextElement element = CreateElement("ab\r\ncd", width: 10, height: 3);
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.HasCount(2, provider.Writes);
		Assert.AreEqual(new Position(0, 0), provider.WritesOf("ab").Single().Position);
		Assert.AreEqual(new Position(0, 1), provider.WritesOf("cd").Single().Position);
	}

	/// <summary>
	/// A blank line in the text must keep its row when word wrapping is on (ktsu-dev/TUI#135).
	/// </summary>
	[TestMethod]
	public void WordWrapKeepsABlankLineBetweenParagraphs()
	{
		// Arrange
		TextElement element = CreateElement("ab\n\ncd", width: 10, height: 3);
		element.WordWrap = true;
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.AreEqual(0, provider.WritesOf("ab").Single().Position.Y);
		Assert.AreEqual(2, provider.WritesOf("cd").Single().Position.Y);
	}

	/// <summary>
	/// A line wider than the content area must be clipped to it, whichever way it is aligned, so
	/// that it never draws over a neighbour or past the screen edge (ktsu-dev/TUI#134).
	/// </summary>
	/// <param name="alignment">The horizontal alignment under test.</param>
	[TestMethod]
	[DataRow(HorizontalAlignment.Left)]
	[DataRow(HorizontalAlignment.Center)]
	[DataRow(HorizontalAlignment.Right)]
	public void LineWiderThanTheContentAreaIsClippedToIt(HorizontalAlignment alignment)
	{
		// Arrange
		TextElement element = new()
		{
			Text = "abcdef",
			Position = new Position(4, 0),
			Dimensions = new Dimensions(3, 1),
			HorizontalAlignment = alignment,
		};
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		RecordingConsoleProvider.Write write = provider.Writes.Single();
		Assert.AreEqual("abc", write.Text);
		Assert.AreEqual(4, write.Position.X);
	}

	/// <summary>
	/// The reported case: the second of two texts in a six-wide horizontal panel is given two
	/// columns and must draw only those two (ktsu-dev/TUI#134).
	/// </summary>
	[TestMethod]
	public void TextInAHorizontalPanelDoesNotDrawPastItsAllottedWidth()
	{
		// Arrange
		StackPanel panel = new()
		{
			Orientation = Orientation.Horizontal,
			Position = Position.Origin,
			Dimensions = new Dimensions(6, 1),
		};
		panel.AddChild(new TextElement("abcd"));
		panel.AddChild(new TextElement("xyz"));
		panel.ArrangeChildren();
		RecordingConsoleProvider provider = new() { Dimensions = new Dimensions(6, 1) };

		// Act
		panel.Render(provider);

		// Assert
		Assert.IsTrue(
			provider.Writes.All(w => w.Position.X + w.Text.Length <= 6),
			"No write may extend past the six-column console");
		Assert.ContainsSingle(provider.WritesOf("xy"));
	}
}
