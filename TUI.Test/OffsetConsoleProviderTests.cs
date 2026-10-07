// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spectre.Console;

/// <summary>
/// Tests for <see cref="OffsetConsoleProvider"/>, which draws an element somewhere other than its
/// own position by shifting what it writes (ktsu-dev/TUI#156).
/// </summary>
[TestClass]
public sealed class OffsetConsoleProviderTests
{
	private static readonly Position Offset = new(3, 2);

	/// <summary>
	/// Tests that writes and cursor moves are shifted by the offset.
	/// </summary>
	[TestMethod]
	public void PositionsAreShiftedByTheOffset()
	{
		RecordingConsoleProvider inner = new();
		OffsetConsoleProvider provider = new(inner, Offset);
		TextStyle style = TextStyle.Default with { IsBold = true };

		provider.WriteAt("x", new Position(1, 1), style);
		provider.SetCursorPosition(new Position(4, 0));

		Assert.AreEqual(new RecordingConsoleProvider.Write("x", new Position(4, 3), style), inner.Writes.Single());
		Assert.AreEqual(new Position(7, 2), inner.CursorPosition);
	}

	/// <summary>
	/// Tests that everything without a position passes through unchanged.
	/// </summary>
	[TestMethod]
	public async Task EverythingElsePassesThroughUnchanged()
	{
		RecordingConsoleProvider inner = new() { Dimensions = new Dimensions(12, 7) };
		OffsetConsoleProvider provider = new(inner, Offset);

		provider.Clear();
		provider.SetCursorVisibility(false);
		InputResult input = await provider.ReadInputAsync().ConfigureAwait(false);

		Assert.AreEqual(new Dimensions(12, 7), provider.Dimensions);
		Assert.AreEqual(1, inner.ClearCount);
		Assert.IsFalse(inner.CursorVisible);
		Assert.AreEqual(new InputResult(), input);
	}

	/// <summary>
	/// Tests that an element rendered through the provider at a position lands at that position
	/// plus the offset, so offsets compose.
	/// </summary>
	[TestMethod]
	public void NestedRenderAddsTheOffset()
	{
		StringWriter output = new();
		IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
		{
			Ansi = AnsiSupport.Yes,
			ColorSystem = ColorSystemSupport.NoColors,
			Interactive = InteractionSupport.No,
			Out = new AnsiConsoleOutput(output),
		});
		console.Profile.Width = 20;
		console.Profile.Height = 10;
		OffsetConsoleProvider provider = new(new SpectreConsoleProvider(console), Offset);

		provider.Render(new TextElement("Hi") { Dimensions = new Dimensions(5, 1) }, new Position(1, 1));

		Assert.AreEqual("\u001b[4;5HHi", output.ToString());
	}
}
