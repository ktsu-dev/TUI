// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests that a negative padding side is treated as 0, so an element never draws outside the slot
/// its parent arranged (ktsu-dev/TUI#255).
/// </summary>
[TestClass]
public sealed class NegativePaddingTests
{
	/// <summary>
	/// Tests that each negative side reads back as 0 and a positive side is kept.
	/// </summary>
	[TestMethod]
	public void NegativeSidesAreTreatedAsZero()
	{
		Padding padding = new(-3, 2, -1, -7);

		Assert.AreEqual(0, padding.Left);
		Assert.AreEqual(2, padding.Top);
		Assert.AreEqual(0, padding.Right);
		Assert.AreEqual(0, padding.Bottom);
		Assert.AreEqual(0, padding.Horizontal);
		Assert.AreEqual(2, padding.Vertical);
		Assert.AreEqual(new Padding(0, 2, 0, 0), padding);
	}

	/// <summary>
	/// Tests that a <c>with</c> expression and the factory methods clamp as the constructor does.
	/// </summary>
	[TestMethod]
	public void WithExpressionAndFactoriesClampToo()
	{
		Padding padding = Padding.None with { Left = -4, Bottom = 1 };

		Assert.AreEqual(new Padding(0, 0, 0, 1), padding);
		Assert.AreEqual(Padding.None, Padding.Uniform(-2));
		Assert.AreEqual(Padding.None, Padding.CreateHorizontal(-2));
		Assert.AreEqual(Padding.None, Padding.CreateVertical(-2));
	}

	/// <summary>
	/// Tests that padding never makes <see cref="Dimensions.WithPadding"/> smaller or
	/// <see cref="Dimensions.WithoutPadding"/> larger than the dimensions it starts from.
	/// </summary>
	[TestMethod]
	public void PaddedAndUnpaddedSizesStayWithinTheElement()
	{
		Dimensions size = new(10, 4);
		Padding padding = new(-3, -2, -1, -5);

		Assert.AreEqual(size, size.WithPadding(padding));
		Assert.AreEqual(size, size.WithoutPadding(padding));
	}

	/// <summary>
	/// Tests the issue's scenario: in a horizontal panel, a child with a negative left padding draws
	/// only inside its arranged bounds and leaves its sibling's text alone.
	/// </summary>
	[TestMethod]
	public void NegativeLeftPaddingDoesNotDrawOverASibling()
	{
		TextElement first = new("AAAA");
		TextElement second = new("BBBB") { Padding = new Padding(-3, 0, 0, 0) };
		StackPanel panel = new(Orientation.Horizontal) { Dimensions = new Dimensions(20, 1) };
		panel.AddChild(first);
		panel.AddChild(second);
		panel.ArrangeChildren();

		RecordingConsoleProvider provider = new();
		panel.Render(provider);

		RecordingConsoleProvider.Write[] secondWrites = [.. provider.WritesOf("BBBB")];
		Assert.IsNotEmpty(secondWrites, "the padded element should still draw its text");
		foreach (RecordingConsoleProvider.Write write in secondWrites)
		{
			Assert.IsGreaterThanOrEqualTo(second.Position.X, write.Position.X, $"BBBB was written at X={write.Position.X}, left of its slot at X={second.Position.X}");
			Assert.IsLessThanOrEqualTo(second.Position.X + second.Dimensions.Width, write.Position.X + write.Text.Length, "BBBB ran past the right edge of its slot");
		}

		Assert.IsNotEmpty(provider.WritesOf("AAAA"), "the first element should still draw its text");
		Assert.AreEqual(0, first.Position.X);
	}
}
