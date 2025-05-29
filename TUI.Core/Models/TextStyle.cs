// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Drawing;

namespace ktsu.TUI.Core.Models;

/// <summary>
/// Represents text styling options
/// </summary>
public readonly record struct TextStyle
{
	/// <summary>
	/// Gets or sets the foreground color
	/// </summary>
	public Color? ForegroundColor { get; init; }

	/// <summary>
	/// Gets or sets the background color
	/// </summary>
	public Color? BackgroundColor { get; init; }

	/// <summary>
	/// Gets or sets whether the text is bold
	/// </summary>
	public bool IsBold { get; init; }

	/// <summary>
	/// Gets or sets whether the text is italic
	/// </summary>
	public bool IsItalic { get; init; }

	/// <summary>
	/// Gets or sets whether the text is underlined
	/// </summary>
	public bool IsUnderlined { get; init; }

	/// <summary>
	/// Gets or sets whether the text is strikethrough
	/// </summary>
	public bool IsStrikethrough { get; init; }

	/// <summary>
	/// Gets or sets the foreground color as a string (convenience property)
	/// </summary>
	public string? Foreground
	{
		get => ForegroundColor?.Name;
		init => ForegroundColor = string.IsNullOrEmpty(value) ? null : Color.FromName(value);
	}

	/// <summary>
	/// Gets or sets the background color as a string (convenience property)
	/// </summary>
	public string? Background
	{
		get => BackgroundColor?.Name;
		init => BackgroundColor = string.IsNullOrEmpty(value) ? null : Color.FromName(value);
	}

	/// <summary>
	/// Gets or sets whether the text is underlined (alias for IsUnderlined)
	/// </summary>
	public bool IsUnderline
	{
		get => IsUnderlined;
		init => IsUnderlined = value;
	}

	/// <summary>
	/// Gets the default text style
	/// </summary>
	public static TextStyle Default => new();

	/// <summary>
	/// Creates a new text style with the specified foreground color
	/// </summary>
	/// <param name="color">The foreground color</param>
	/// <returns>The styled text</returns>
	public static TextStyle WithForeground(Color color) => new() { ForegroundColor = color };

	/// <summary>
	/// Creates a new text style with the specified background color
	/// </summary>
	/// <param name="color">The background color</param>
	/// <returns>The styled text</returns>
	public static TextStyle WithBackground(Color color) => new() { BackgroundColor = color };

	/// <summary>
	/// Creates a bold text style
	/// </summary>
	/// <returns>The bold text style</returns>
	public static TextStyle Bold => new() { IsBold = true };

	/// <summary>
	/// Creates an italic text style
	/// </summary>
	/// <returns>The italic text style</returns>
	public static TextStyle Italic => new() { IsItalic = true };

	/// <summary>
	/// Creates an underlined text style
	/// </summary>
	/// <returns>The underlined text style</returns>
	public static TextStyle Underlined => new() { IsUnderlined = true };
}
