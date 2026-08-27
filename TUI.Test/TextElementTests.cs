// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

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
}
