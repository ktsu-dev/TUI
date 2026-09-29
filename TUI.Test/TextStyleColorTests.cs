// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using System.Drawing;
using ktsu.TUI.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for the string color properties on <see cref="TextStyle"/>.
/// </summary>
/// <remarks>
/// <see cref="Color.FromName(string)"/> never fails: an unrecognized name comes back as a
/// transparent black with <see cref="Color.IsKnownColor"/> false, which the console provider
/// then renders as <c>#000000</c>. A typo therefore used to produce invisible text with no
/// error, no warning and nothing to debug from (ktsu-dev/TUI#116).
/// </remarks>
[TestClass]
public sealed class TextStyleColorTests
{
	/// <summary>
	/// Tests that a name no color answers to is rejected rather than silently becoming black.
	/// </summary>
	/// <param name="value">The unrecognized color value.</param>
	[TestMethod]
	[DataRow("cyann")]
	[DataRow("bleu")]
	[DataRow("not a color")]
	[DataRow("#GGGGGG")]
	[DataRow("#FF00")]
	public void ForegroundRejectsUnrecognizedColor(string value)
	{
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(
			() => _ = new TextStyle { Foreground = value },
			$"'{value}' names no color, so assigning it should throw rather than render as black");

		Assert.AreEqual("Foreground", exception.ParamName);
		Assert.Contains(value, exception.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// Tests that <see cref="TextStyle.Background"/> validates on the same terms as the foreground.
	/// </summary>
	[TestMethod]
	public void BackgroundRejectsUnrecognizedColor()
	{
		ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(
			() => _ = new TextStyle { Background = "purpel" });

		Assert.AreEqual("Background", exception.ParamName);
		Assert.Contains("purpel", exception.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// Tests that known color names still resolve, case-insensitively.
	/// </summary>
	/// <param name="value">The color name to assign.</param>
	/// <param name="expectedName">The canonical name the getter should report back.</param>
	[TestMethod]
	[DataRow("red", "Red")]
	[DataRow("Red", "Red")]
	[DataRow("CYAN", "Cyan")]
	[DataRow("gray", "Gray")]
	[DataRow("transparent", "Transparent")]
	public void KnownColorNamesAreAccepted(string value, string expectedName)
	{
		TextStyle style = new() { Foreground = value, Background = value };

		Assert.AreEqual(expectedName, style.Foreground);
		Assert.AreEqual(expectedName, style.Background);
		Assert.AreEqual(Color.FromName(value).ToArgb(), style.ForegroundColor!.Value.ToArgb());
	}

	/// <summary>
	/// Tests that a null or empty value clears the color instead of throwing.
	/// </summary>
	/// <param name="value">The empty color value.</param>
	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	public void EmptyColorClearsTheColor(string? value)
	{
		TextStyle style = new() { Foreground = value, Background = value };

		Assert.IsNull(style.ForegroundColor);
		Assert.IsNull(style.BackgroundColor);
	}

	/// <summary>
	/// Tests the hex forms, which were previously rendered as black along with every typo.
	/// </summary>
	/// <param name="value">The hex color value to assign.</param>
	/// <param name="expectedArgb">The ARGB the value should resolve to.</param>
	[TestMethod]
	[DataRow("#FF0000", unchecked((int)0xFFFF0000))]
	[DataRow("#ff0000", unchecked((int)0xFFFF0000))]
	[DataRow("00FF00", unchecked((int)0xFF00FF00))]
	[DataRow("#8000FF00", unchecked((int)0x8000FF00))]
	[DataRow("ff010203", unchecked((int)0xFF010203))]
	public void HexColorsAreAccepted(string value, int expectedArgb)
	{
		TextStyle style = new() { Foreground = value };

		Assert.AreEqual(expectedArgb, style.ForegroundColor!.Value.ToArgb());
	}

	/// <summary>
	/// Tests that a color with no known name survives a getter/setter round trip.
	/// </summary>
	/// <remarks>
	/// Copying a style with <c>style with { Foreground = other.Foreground }</c> has to keep
	/// working, so validation cannot reject what the getter itself produces. The low-alpha rows
	/// used to fail because <see cref="Color.Name"/> drops leading zeros: alpha <c>0x0A</c> gave a
	/// seven-digit name the setter rejected, and alpha <c>0x00</c> gave six digits the setter read
	/// as an opaque color (ktsu-dev/TUI#142).
	/// </remarks>
	/// <param name="argb">The packed ARGB value of the color to round trip.</param>
	[TestMethod]
	[DataRow(unchecked((int)0xFF010203))]
	[DataRow(unchecked((int)0x80123456))]
	[DataRow(0x10123456)]
	[DataRow(0x0F123456)]
	[DataRow(0x0A123456)]
	[DataRow(0x01123456)]
	[DataRow(0x00123456)]
	[DataRow(0x00000001)]
	[DataRow(0)]
	public void UnnamedColorRoundTripsThroughTheStringProperty(int argb)
	{
		Color original = Color.FromArgb(argb);
		TextStyle source = new() { ForegroundColor = original, BackgroundColor = original };

		TextStyle copy = new() { Foreground = source.Foreground, Background = source.Background };

		Assert.AreEqual(original.ToArgb(), copy.ForegroundColor!.Value.ToArgb());
		Assert.AreEqual(original.ToArgb(), copy.BackgroundColor!.Value.ToArgb());
	}

	/// <summary>
	/// Tests that an unnamed color reads back as a fixed-width <c>#AARRGGBB</c> value.
	/// </summary>
	[TestMethod]
	public void UnnamedColorReadsBackAsFixedWidthHex()
	{
		TextStyle style = new() { ForegroundColor = Color.FromArgb(0x0A, 0x12, 0x34, 0x56) };

		Assert.AreEqual("#0A123456", style.Foreground);
	}
}
