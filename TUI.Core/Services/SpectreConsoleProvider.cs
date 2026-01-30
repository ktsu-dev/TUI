// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

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

		SetCursorPosition(position);

		if (style.HasValue)
		{
			Markup markup = CreateStyledMarkup(text, style.Value);
			_console.Write(markup);
		}
		else
		{
			_console.Write(text);
		}
	}

	/// <inheritdoc />
	public async Task<InputResult> ReadInputAsync()
	{
		return await Task.Run(() =>
		{
			ConsoleKeyInfo keyInfo = Console.ReadKey(true);

			// Handle special cases
			if (keyInfo.Key == ConsoleKey.Escape ||
				(keyInfo.Key == ConsoleKey.C && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)))
			{
				return InputResult.Exit();
			}

			// Return keyboard input
			return InputResult.FromKey(keyInfo.Key, keyInfo.Modifiers);
		}).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public void SetCursorVisibility(bool visible)
	{
		_console.Cursor.SetPosition(Console.CursorLeft, Console.CursorTop);
		Console.CursorVisible = visible;
	}

	/// <inheritdoc />
	public void SetCursorPosition(Position position)
	{
		if (position.X >= 0 && position.Y >= 0 &&
			position.X < Dimensions.Width && position.Y < Dimensions.Height)
		{
			_console.Cursor.SetPosition(position.X, position.Y);
		}
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
