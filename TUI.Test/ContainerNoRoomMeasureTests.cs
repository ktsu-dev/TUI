// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for a container measured at a width its padding takes entirely (ktsu-dev/TUI#249).
/// </summary>
/// <remarks>
/// The container measured its children at width 0, which <see cref="TextElement"/> reads as "no
/// constraint", so a word-wrapped child reported its full unwrapped height for text that could
/// never be drawn, and the rows it claimed were taken from the siblings below.
/// </remarks>
[TestClass]
public sealed class ContainerNoRoomMeasureTests
{
	/// <summary>
	/// A border measured at width 1 or 2 reports the bare border, whatever its child is.
	/// </summary>
	/// <param name="availableWidth">A width no wider than the border's padding.</param>
	[TestMethod]
	[DataRow(1)]
	[DataRow(2)]
	public void BorderWithNoRoomForItsChildMeasuresAsTheBareBorder(int availableWidth)
	{
		BorderElement border = new() { Child = new TextElement("one\ntwo\nthree\nfour\nfive") { WordWrap = true } };

		Assert.AreEqual(new Dimensions(2, 2), border.CalculateRequiredDimensions(availableWidth));
	}

	/// <summary>
	/// A border given one more column than its padding still measures its child.
	/// </summary>
	[TestMethod]
	public void BorderWithOneColumnOfRoomStillMeasuresItsChild()
	{
		BorderElement border = new() { Child = new TextElement("ab") { WordWrap = true } };

		Assert.AreEqual(new Dimensions(3, 4), border.CalculateRequiredDimensions(3));
	}

	/// <summary>
	/// The issue's layout: a border squeezed to its padding width beside a label leaves the row
	/// below where the bare border ends, not where its unwrapped child would.
	/// </summary>
	[TestMethod]
	public void SqueezedBorderDoesNotPushTheNextRowDown()
	{
		BorderElement border = new() { Child = new TextElement("one\ntwo\nthree\nfour\nfive") { WordWrap = true } };
		StackPanel row = new(Orientation.Horizontal);
		row.AddChild(new TextElement("Label text here: "));
		row.AddChild(border);
		StackPanel panel = new(Orientation.Vertical);
		panel.AddChild(row);
		panel.AddChild(new TextElement("below"));
		panel.Position = Position.Origin;
		panel.Dimensions = new Dimensions(19, 20);
		RecordingConsoleProvider provider = new();

		panel.ArrangeChildren();
		panel.Render(provider);

		Assert.AreEqual(2, provider.WritesOf("below").Single().Position.Y);
	}
}
