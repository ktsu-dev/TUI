// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Drawing;
using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;
using Spectre.Console;

namespace ktsu.TUI.Core.Services;

/// <summary>
/// Console provider implementation using Spectre.Console
/// </summary>
public class SpectreConsoleProvider : IConsoleProvider
{
	private readonly IAnsiConsole _console;

	/// <summary>
	/// Initializes a new instance of the <see cref="SpectreConsoleProvider"/> class
	/// </summary>
	/// <param name="console">The Spectre.Console instance to use</param>
	public SpectreConsoleProvider(IAnsiConsole? console = null)
	{
		_console = console ?? AnsiConsole.Console;
	}

	/// <inheritdoc />
	public Dimensions Dimensions => new(_console.Profile.Width, _console.Profile.Height);

	/// <inheritdoc />
	public void Clear()
	{
		_console.Clear();
	}

	/// <inheritdoc />
	public void Render(IUIElement element, Position position)
	{
		if (!element.IsVisible)
			return;

		// Save current cursor position
		var originalPosition = GetCursorPosition();

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
			return;

		SetCursorPosition(position);

		if (style.HasValue)
		{
			var markup = CreateStyledMarkup(text, style.Value);
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
			var keyInfo = Console.ReadKey(true);

			// Handle special cases
			if (keyInfo.Key == ConsoleKey.Escape ||
				(keyInfo.Key == ConsoleKey.C && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)))
			{
				return InputResult.Exit();
			}

			// Return keyboard input
			return InputResult.FromKey(keyInfo.Key, keyInfo.Modifiers);
		});
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

	private Position GetCursorPosition()
	{
		return new Position(Console.CursorLeft, Console.CursorTop);
	}

	private static Markup CreateStyledMarkup(string text, TextStyle style)
	{
		var styleString = BuildStyleString(style);
		var escapedText = text.Replace("[", "[[").Replace("]", "]]");

		return string.IsNullOrEmpty(styleString)
			? new Markup(escapedText)
			: new Markup($"[{styleString}]{escapedText}[/]");
	}

	private static string BuildStyleString(TextStyle style)
	{
		var parts = new List<string>();

		if (style.ForegroundColor.HasValue)
		{
			var color = style.ForegroundColor.Value;
			parts.Add($"#{color.R:X2}{color.G:X2}{color.B:X2}");
		}

		if (style.BackgroundColor.HasValue)
		{
			var color = style.BackgroundColor.Value;
			parts.Add($"on #{color.R:X2}{color.G:X2}{color.B:X2}");
		}

		if (style.IsBold)
			parts.Add("bold");

		if (style.IsItalic)
			parts.Add("italic");

		if (style.IsUnderlined)
			parts.Add("underline");

		if (style.IsStrikethrough)
			parts.Add("strikethrough");

		return string.Join(" ", parts);
	}
}
