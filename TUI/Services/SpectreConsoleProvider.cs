// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Core.Services;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;
using Spectre.Console;

/// <summary>
/// Console provider implementation using Spectre.Console
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SpectreConsoleProvider"/> class
/// </remarks>
/// <param name="console">The Spectre.Console instance to use</param>
public class SpectreConsoleProvider(IAnsiConsole? console = null) : IConsoleProvider
{
	private readonly IAnsiConsole _console = console ?? AnsiConsole.Console;

	/// <inheritdoc />
	public Dimensions Dimensions => new(_console.Profile.Width, _console.Profile.Height);

	/// <inheritdoc />
	public void Clear() => _console.Clear();

	/// <inheritdoc />
	public void Render(IUIElement element, Position position)
	{
		Ensure.NotNull(element);

		if (!element.IsVisible)
		{
			return;
		}

		// Save current cursor position
		Position originalPosition = GetCursorPosition();

		// Set cursor to render position
		SetCursorPosition(position);

		// Let the element render itself
		element.Render(this);

		// Restore cursor position
		SetCursorPosition(originalPosition);
	}

	/// <inheritdoc />
	public void WriteAt(string text, Position position, TextStyle? style = null)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}

		// Clip to the screen before moving the cursor. SetCursorPosition ignores an off-screen
		// position, so writing anyway would put the text wherever the last write left the cursor,
		// and text running past the right edge would wrap onto the next line (ktsu-dev/TUI#139).
		string visibleText = ClipToScreen(text, position, out Position visiblePosition);
		if (visibleText.Length == 0)
		{
			return;
		}

		SetCursorPosition(visiblePosition);

		if (style.HasValue)
		{
			Markup markup = CreateStyledMarkup(visibleText, style.Value);
			_console.Write(markup);
		}
		else
		{
			_console.Write(visibleText);
		}
	}

	/// <inheritdoc />
	public async Task<InputResult> ReadInputAsync() =>
		await Task.Run(() => ToInputResult(Console.ReadKey(true))).ConfigureAwait(false);

	/// <summary>
	/// Converts a key read from the console into an input result.
	/// </summary>
	/// <param name="keyInfo">The key that was read.</param>
	/// <returns>The input result.</returns>
	internal static InputResult ToInputResult(ConsoleKeyInfo keyInfo)
	{
		// Handle special cases. Ctrl+C normally arrives as an interrupt signal rather than as
		// a key, and UIApplication handles it there; this branch only fires for a host that
		// has set Console.TreatControlCAsInput.
		if (keyInfo.Key == ConsoleKey.Escape ||
			(keyInfo.Key == ConsoleKey.C && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)))
		{
			return InputResult.Exit();
		}

		// Keep the typed character alongside the key. Only the character tells '!' from '1', and
		// only it reflects Caps Lock and the keyboard layout, so text entry needs it
		// (ktsu-dev/TUI#152). The key and modifiers stay, so key-based handlers are unaffected.
		InputResult result = InputResult.FromKey(keyInfo.Key, keyInfo.Modifiers);
		return keyInfo.KeyChar != '\0' && !char.IsControl(keyInfo.KeyChar)
			? result with { Character = keyInfo.KeyChar }
			: result;
	}

	/// <inheritdoc />
	/// <remarks>
	/// Writes to the injected console rather than <see cref="Console"/>, and leaves the cursor where
	/// it is (ktsu-dev/TUI#153).
	/// </remarks>
	public void SetCursorVisibility(bool visible) => _console.Cursor.Show(visible);

	/// <inheritdoc />
	public void SetCursorPosition(Position position)
	{
		if (position.X >= 0 && position.Y >= 0 &&
			position.X < Dimensions.Width && position.Y < Dimensions.Height)
		{
			// Spectre writes the ANSI CUP sequence without translating, and CUP is 1-based, so a
			// 0-based position has to be shifted or row 0 and row 1 collapse (ktsu-dev/TUI#138).
			_console.Cursor.SetPosition(position.X + 1, position.Y + 1);
		}
	}

	private string ClipToScreen(string text, Position position, out Position visiblePosition)
	{
		visiblePosition = position;
		Dimensions dimensions = Dimensions;
		if (position.Y < 0 || position.Y >= dimensions.Height)
		{
			return string.Empty;
		}

		long start = position.X;
		long end = start + text.Length;
		long visibleStart = Math.Max(start, 0);
		long visibleEnd = Math.Min(end, dimensions.Width);
		if (visibleStart >= visibleEnd)
		{
			return string.Empty;
		}

		visiblePosition = new Position((int)visibleStart, position.Y);
		return text.Substring((int)(visibleStart - start), (int)(visibleEnd - visibleStart));
	}

	private static Position GetCursorPosition() => new(Console.CursorLeft, Console.CursorTop);

	private static Markup CreateStyledMarkup(string text, TextStyle style)
	{
		string styleString = BuildStyleString(style);
		string escapedText = text.Replace("[", "[[").Replace("]", "]]");

		return string.IsNullOrEmpty(styleString)
			? new Markup(escapedText)
			: new Markup($"[{styleString}]{escapedText}[/]");
	}

	private static string BuildStyleString(TextStyle style)
	{
		List<string> parts = [];

		if (style.ForegroundColor.HasValue)
		{
			System.Drawing.Color color = style.ForegroundColor.Value;
			parts.Add($"#{color.R:X2}{color.G:X2}{color.B:X2}");
		}

		if (style.BackgroundColor.HasValue)
		{
			System.Drawing.Color color = style.BackgroundColor.Value;
			parts.Add($"on #{color.R:X2}{color.G:X2}{color.B:X2}");
		}

		if (style.IsBold)
		{
			parts.Add("bold");
		}

		if (style.IsItalic)
		{
			parts.Add("italic");
		}

		if (style.IsUnderlined)
		{
			parts.Add("underline");
		}

		if (style.IsStrikethrough)
		{
			parts.Add("strikethrough");
		}

		return string.Join(" ", parts);
	}
}
