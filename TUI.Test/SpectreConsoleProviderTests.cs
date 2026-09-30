// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spectre.Console;

/// <summary>
/// Tests for the ANSI that <see cref="SpectreConsoleProvider"/> writes, captured from a Spectre
/// console over a <see cref="StringWriter"/> rather than the test runner's terminal.
/// </summary>
[TestClass]
public sealed class SpectreConsoleProviderTests
{
	private const int Width = 10;
	private const int Height = 5;

	private static (SpectreConsoleProvider Provider, StringWriter Output) CreateProvider()
	{
		StringWriter output = new();
		IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
		{
			Ansi = AnsiSupport.Yes,
			ColorSystem = ColorSystemSupport.NoColors,
			Interactive = InteractionSupport.No,
			Out = new AnsiConsoleOutput(output),
		});
		console.Profile.Width = Width;
		console.Profile.Height = Height;
		return (new SpectreConsoleProvider(console), output);
	}

	/// <summary>
	/// Tests that 0-based positions become 1-based ANSI cursor positions, so the first row and
	/// column stay distinct from the second and the last row and column are reachable
	/// (ktsu-dev/TUI#138).
	/// </summary>
	[TestMethod]
	public void WriteAtTranslatesToOneBasedCursorPositions()
	{
		(SpectreConsoleProvider provider, StringWriter output) = CreateProvider();

		provider.WriteAt("A", new Position(0, 0));
		provider.WriteAt("B", new Position(0, 1));
		provider.WriteAt("C", new Position(1, 0));
		provider.WriteAt("D", new Position(Width - 1, Height - 1));

		Assert.AreEqual("\u001b[1;1HA\u001b[2;1HB\u001b[1;2HC\u001b[5;10HD", output.ToString());
	}

	/// <summary>
	/// Tests that text on a row outside the screen is not written at all, rather than at the
	/// cursor position the previous write left behind (ktsu-dev/TUI#139).
	/// </summary>
	/// <param name="y">The off-screen row.</param>
	[TestMethod]
	[DataRow(-1)]
	[DataRow(Height)]
	[DataRow(7)]
	public void WriteAtSkipsOffScreenRows(int y)
	{
		(SpectreConsoleProvider provider, StringWriter output) = CreateProvider();

		provider.WriteAt("AB", new Position(2, 1));
		provider.WriteAt("XYZ", new Position(3, y));

		Assert.AreEqual("\u001b[2;3HAB", output.ToString());
	}

	/// <summary>
	/// Tests that text lying wholly left or right of the screen is not written.
	/// </summary>
	/// <param name="x">The start column.</param>
	[TestMethod]
	[DataRow(-3)]
	[DataRow(-10)]
	[DataRow(Width)]
	[DataRow(Width + 4)]
	public void WriteAtSkipsTextEntirelyOutsideTheColumns(int x)
	{
		(SpectreConsoleProvider provider, StringWriter output) = CreateProvider();

		provider.WriteAt("XYZ", new Position(x, 0));

		Assert.AreEqual(string.Empty, output.ToString());
	}

	/// <summary>
	/// Tests that text starting left of the screen is clipped to the columns that are visible.
	/// </summary>
	[TestMethod]
	public void WriteAtClipsTextStartingLeftOfTheScreen()
	{
		(SpectreConsoleProvider provider, StringWriter output) = CreateProvider();

		provider.WriteAt("abcdef", new Position(-2, 3));

		Assert.AreEqual("\u001b[4;1Hcdef", output.ToString());
	}

	/// <summary>
	/// Tests that text running past the right edge is clipped instead of wrapping onto the next row.
	/// </summary>
	[TestMethod]
	public void WriteAtClipsTextOverflowingTheRightEdge()
	{
		(SpectreConsoleProvider provider, StringWriter output) = CreateProvider();

		provider.WriteAt("abcdef", new Position(7, 2));

		Assert.AreEqual("\u001b[3;8Habc", output.ToString());
	}

	/// <summary>
	/// Tests that a printable key keeps its typed character as well as its key and modifiers, so
	/// Shift+1 can be told apart from 1 (ktsu-dev/TUI#152).
	/// </summary>
	/// <param name="keyChar">The character the key produced.</param>
	/// <param name="key">The key that was pressed.</param>
	/// <param name="shift">Whether Shift was held.</param>
	[TestMethod]
	[DataRow('!', ConsoleKey.D1, true)]
	[DataRow('1', ConsoleKey.D1, false)]
	[DataRow('A', ConsoleKey.A, true)]
	[DataRow('a', ConsoleKey.A, false)]
	[DataRow('?', ConsoleKey.Oem2, true)]
	[DataRow('é', ConsoleKey.Oem1, false)]
	public void ToInputResultKeepsThePrintableCharacter(char keyChar, ConsoleKey key, bool shift)
	{
		InputResult result = SpectreConsoleProvider.ToInputResult(new ConsoleKeyInfo(keyChar, key, shift, alt: false, control: false));

		Assert.AreEqual(InputType.Keyboard, result.Type);
		Assert.AreEqual(key, result.Key);
		Assert.AreEqual(shift ? ConsoleModifiers.Shift : default, result.Modifiers);
		Assert.AreEqual(keyChar, result.Character);
	}

	/// <summary>
	/// Tests that a key with no printable character, such as an arrow or Enter, carries no
	/// character.
	/// </summary>
	/// <param name="keyChar">The character the key produced.</param>
	/// <param name="key">The key that was pressed.</param>
	[TestMethod]
	[DataRow('\0', ConsoleKey.UpArrow)]
	[DataRow('\r', ConsoleKey.Enter)]
	[DataRow('\t', ConsoleKey.Tab)]
	[DataRow('\b', ConsoleKey.Backspace)]
	public void ToInputResultLeavesTheCharacterUnsetForNonPrintableKeys(char keyChar, ConsoleKey key)
	{
		InputResult result = SpectreConsoleProvider.ToInputResult(new ConsoleKeyInfo(keyChar, key, shift: false, alt: false, control: false));

		Assert.AreEqual(InputType.Keyboard, result.Type);
		Assert.AreEqual(key, result.Key);
		Assert.IsNull(result.Character);
	}

	/// <summary>
	/// Tests that Escape still asks the application to exit.
	/// </summary>
	[TestMethod]
	public void ToInputResultTreatsEscapeAsExit()
	{
		InputResult result = SpectreConsoleProvider.ToInputResult(new ConsoleKeyInfo('\u001b', ConsoleKey.Escape, shift: false, alt: false, control: false));

		Assert.IsTrue(result.IsExit);
	}

	/// <summary>
	/// Tests that Ctrl+C asks the application to exit, for a host that has set
	/// <see cref="Console.TreatControlCAsInput"/>.
	/// </summary>
	[TestMethod]
	public void ToInputResultTreatsControlCAsExit()
	{
		InputResult result = SpectreConsoleProvider.ToInputResult(new ConsoleKeyInfo('\u0003', ConsoleKey.C, shift: false, alt: false, control: true));

		Assert.IsTrue(result.IsExit);
	}

	/// <summary>
	/// Tests that C without Ctrl is ordinary typed input, not an exit request.
	/// </summary>
	[TestMethod]
	public void ToInputResultTreatsPlainCAsACharacter()
	{
		InputResult result = SpectreConsoleProvider.ToInputResult(new ConsoleKeyInfo('c', ConsoleKey.C, shift: false, alt: false, control: false));

		Assert.IsFalse(result.IsExit);
		Assert.AreEqual('c', result.Character);
	}

	/// <summary>
	/// Tests that showing or hiding the cursor writes the DECTCEM sequence to the injected console
	/// and does not move the cursor (ktsu-dev/TUI#153).
	/// </summary>
	/// <param name="visible">Whether the cursor is shown.</param>
	/// <param name="expected">The sequence expected in the captured output.</param>
	[TestMethod]
	[DataRow(false, "\u001b[?25l")]
	[DataRow(true, "\u001b[?25h")]
	public void SetCursorVisibilityWritesToTheInjectedConsoleWithoutMovingTheCursor(bool visible, string expected)
	{
		(SpectreConsoleProvider provider, StringWriter output) = CreateProvider();

		provider.SetCursorVisibility(visible);

		Assert.AreEqual(expected, output.ToString());
	}
}
