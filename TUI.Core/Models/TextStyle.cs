// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Core.Models;

using System.Drawing;
using System.Globalization;

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
	/// Gets or sets the foreground color as a string (convenience property).
	/// Accepts a known color name such as <c>"red"</c> (case-insensitive), or a hex value in
	/// <c>#RRGGBB</c>, <c>#AARRGGBB</c> or bare <c>RRGGBB</c>/<c>AARRGGBB</c> form.
	/// </summary>
	/// <exception cref="ArgumentException">The value is neither a known color name nor a hex value.</exception>
	public string? Foreground
	{
		get => ForegroundColor?.Name;
		init => ForegroundColor = ParseColor(value, nameof(Foreground));
	}

	/// <summary>
	/// Gets or sets the background color as a string (convenience property).
	/// Accepts a known color name such as <c>"blue"</c> (case-insensitive), or a hex value in
	/// <c>#RRGGBB</c>, <c>#AARRGGBB</c> or bare <c>RRGGBB</c>/<c>AARRGGBB</c> form.
	/// </summary>
	/// <exception cref="ArgumentException">The value is neither a known color name nor a hex value.</exception>
	public string? Background
	{
		get => BackgroundColor?.Name;
		init => BackgroundColor = ParseColor(value, nameof(Background));
	}

	/// <summary>
	/// Converts a color string to a <see cref="Color"/>, rejecting values that name no color.
	/// </summary>
	/// <param name="value">The color name or hex value. Null or empty yields no color.</param>
	/// <param name="paramName">The name of the property being assigned, used in the exception.</param>
	/// <returns>The parsed color, or <see langword="null"/> when <paramref name="value"/> is null or empty.</returns>
	/// <exception cref="ArgumentException">
	/// <paramref name="value"/> is neither a known color name nor a hex value.
	/// <see cref="Color.FromName(string)"/> returns a transparent black for any unrecognized name
	/// rather than failing, so an unvalidated typo would render as invisible black text.
	/// </exception>
	private static Color? ParseColor(string? value, string paramName)
	{
		if (string.IsNullOrEmpty(value))
		{
			return null;
		}

		Color named = Color.FromName(value);
		if (named.IsKnownColor)
		{
			return named;
		}

		return TryParseHexColor(value, out Color hex)
			? hex
			: throw new ArgumentException($"'{value}' is not a recognized color name or hex value.", paramName);
	}

	/// <summary>
	/// Parses <c>#RRGGBB</c>, <c>#AARRGGBB</c>, <c>RRGGBB</c> and <c>AARRGGBB</c> color values.
	/// The bare forms are what the <see cref="Foreground"/> and <see cref="Background"/> getters
	/// emit for a color that has no known name, so a value read from one can be assigned back.
	/// </summary>
	/// <param name="value">The candidate hex value.</param>
	/// <param name="color">The parsed color, when this method returns <see langword="true"/>.</param>
	/// <returns><see langword="true"/> if the value was a hex color.</returns>
	private static bool TryParseHexColor(string value, out Color color)
	{
		color = default;

		ReadOnlySpan<char> digits = value.AsSpan();
		if (digits.Length > 0 && digits[0] == '#')
		{
			digits = digits[1..];
		}

		if (digits.Length is not (6 or 8) ||
			!uint.TryParse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out uint packed))
		{
			return false;
		}

		if (digits.Length == 6)
		{
			packed |= 0xFF000000u;
		}

		color = Color.FromArgb(unchecked((int)packed));
		return true;
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
