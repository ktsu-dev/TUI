// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Render-path tests for <see cref="BorderElement"/>.
/// </summary>
/// <remarks>
/// <see cref="BorderElementTests"/> covers property round-tripping and invalidation. This class
/// covers what the element actually draws, which is where ktsu-dev/TUI#102 lived: a titled
/// border with the default <see cref="BorderElement.TitleAlignment"/>, and every
/// <see cref="BorderStyle.None"/> element, threw <see cref="NotImplementedException"/>.
/// </remarks>
[TestClass]
public sealed class BorderElementRenderTests
{
	private static BorderElement CreateElement(int width = 30, int height = 5)
	{
		BorderElement element = [];
		element.Position = Position.Origin;
		element.Dimensions = new Dimensions(width, height);
		return element;
	}

	/// <summary>
	/// A titled border with default alignment must render. This is the case that used to throw.
	/// </summary>
	[TestMethod]
	public void RenderTitledBorderWithDefaultAlignmentDrawsTheTitle()
	{
		// Arrange
		BorderElement element = CreateElement();
		element.Title = "Title";
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.AreEqual(HorizontalAlignment.Left, element.TitleAlignment, "Left must remain the default");
		Assert.ContainsSingle(provider.WritesOf(" Title "));
	}

	/// <summary>
	/// Every border style and title alignment pairing must render rather than throw.
	/// </summary>
	/// <param name="style">The border style under test.</param>
	/// <param name="alignment">The title alignment under test.</param>
	[TestMethod]
	[DynamicData(nameof(StyleAlignmentCombinations))]
	public void RenderSucceedsForEveryStyleAndTitleAlignment(BorderStyle style, HorizontalAlignment alignment)
	{
		// Arrange
		BorderElement element = CreateElement();
		element.BorderStyle = style;
		element.Title = "Title";
		element.TitleAlignment = alignment;
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		int expectedTitles = style == BorderStyle.None ? 0 : 1;
		Assert.HasCount(expectedTitles, provider.WritesOf(" Title ").ToList());
	}

	/// <summary>
	/// Gets every border style and title alignment pairing.
	/// </summary>
	public static IEnumerable<object[]> StyleAlignmentCombinations =>
		from style in Enum.GetValues<BorderStyle>()
		from alignment in Enum.GetValues<HorizontalAlignment>()
		select new object[] { style, alignment };

	/// <summary>
	/// <see cref="BorderStyle.None"/> must draw nothing rather than throwing or drawing a
	/// single-line border, which is what the fallback arm would otherwise have produced.
	/// </summary>
	[TestMethod]
	public void BorderStyleNoneDrawsNothing()
	{
		// Arrange
		BorderElement element = CreateElement();
		element.BorderStyle = BorderStyle.None;
		element.Title = "Title";
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.IsEmpty(provider.Writes);
	}

	/// <summary>
	/// <see cref="BorderStyle.None"/> must still render children, so it remains a usable container.
	/// </summary>
	[TestMethod]
	public void BorderStyleNoneStillRendersChildren()
	{
		// Arrange
		BorderElement element = CreateElement();
		element.BorderStyle = BorderStyle.None;
		element.Child = new TextElement("child")
		{
			Position = Position.Origin,
			Dimensions = new Dimensions(20, 1),
		};
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.ContainsSingle(provider.WritesOf("child"));
	}

	/// <summary>
	/// Each visible border style must draw its own corner glyph.
	/// </summary>
	/// <param name="style">The border style under test.</param>
	/// <param name="expectedTopLeft">The corner glyph that style is expected to draw.</param>
	[TestMethod]
	[DataRow(BorderStyle.SingleLine, "┌")]
	[DataRow(BorderStyle.DoubleLine, "╔")]
	[DataRow(BorderStyle.Rounded, "╭")]
	[DataRow(BorderStyle.Thick, "┏")]
	[DataRow(BorderStyle.Ascii, "+")]
	public void EachStyleDrawsItsOwnCornerGlyph(BorderStyle style, string expectedTopLeft)
	{
		// Arrange
		BorderElement element = CreateElement();
		element.BorderStyle = style;
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.AreEqual(expectedTopLeft, provider.Writes[0].Text);
		Assert.AreEqual(Position.Origin, provider.Writes[0].Position);
	}

	/// <summary>
	/// Left title alignment must sit left of centre alignment, which must sit left of right.
	/// </summary>
	[TestMethod]
	public void TitleAlignmentOrdersLeftThenCenterThenRight()
	{
		// Arrange & Act
		static int Render(HorizontalAlignment alignment)
		{
			BorderElement element = CreateElement(width: 40);
			element.Title = "Title";
			element.TitleAlignment = alignment;
			RecordingConsoleProvider provider = new();
			element.Render(provider);
			return provider.WritesOf(" Title ").Single().Position.X;
		}

		int left = Render(HorizontalAlignment.Left);
		int center = Render(HorizontalAlignment.Center);
		int right = Render(HorizontalAlignment.Right);

		// Assert
		Assert.IsGreaterThan(left, center);
		Assert.IsGreaterThan(center, right);
	}

	/// <summary>
	/// A border smaller than 2x2 has no room to draw and must draw nothing.
	/// </summary>
	[TestMethod]
	public void BorderSmallerThanTwoByTwoDrawsNothing()
	{
		// Arrange
		BorderElement element = CreateElement(width: 1, height: 1);
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.IsEmpty(provider.Writes);
	}

	/// <summary>
	/// The title is only drawn once the border is wide enough to hold it.
	/// </summary>
	[TestMethod]
	public void TitleIsSuppressedWhenTheBorderIsTooNarrow()
	{
		// Arrange
		BorderElement element = CreateElement(width: 4, height: 3);
		element.Title = "Title";
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);

		// Assert
		Assert.IsEmpty(provider.WritesOf(" Title "));
	}
}
