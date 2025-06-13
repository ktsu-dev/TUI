// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.App;
using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;

/// <summary>
/// Comprehensive showcase of all TUI library visual features
/// </summary>
public static class ShowcaseDemo
{
	/// <summary>
	/// Runs the showcase demo
	/// </summary>
	public static async Task RunAsync()
	{
		var consoleProvider = new SpectreConsoleProvider();
		var app = UIApplication.CreateBuilder(consoleProvider)
			.UseRootElement(CreateShowcaseUI())
			.Build();

		Console.WriteLine("Starting TUI Showcase Demo. Displaying all visual features...");
		await app.RunAsync();
		Console.WriteLine("Showcase Demo finished.");
	}

	private static IUIElement CreateShowcaseUI()
	{
		var mainLayout = new StackPanel
		{
			Orientation = Orientation.Vertical,
			Spacing = 1,
			Padding = Padding.Uniform(1)
		};

		// Header
		var header = CreateHeader();

		// Content area with three columns
		var contentArea = CreateContentArea();

		// Footer
		var footer = CreateFooter();

		mainLayout.AddChild(header);
		mainLayout.AddChild(contentArea);
		mainLayout.AddChild(footer);

		return mainLayout;
	}

	private static IUIElement CreateHeader()
	{
		return new BorderElement
		{
			Title = "TUI Library Showcase",
			TitleAlignment = HorizontalAlignment.Center,
			BorderStyle = BorderStyle.Double,
			Child = new StackPanel
			{
				Orientation = Orientation.Vertical,
				Children =
				[
					new TextElement
					{
						Text = "Complete Visual Features Demonstration",
						HorizontalAlignment = HorizontalAlignment.Center,
						Style = new TextStyle { IsBold = true, Foreground = "yellow" }
					},
					new TextElement
					{
						Text = "Text Styles • Borders • Layouts • Alignments • Colors",
						HorizontalAlignment = HorizontalAlignment.Center,
						Style = new TextStyle { Foreground = "gray" }
					}
				]
			}
		};
	}

	private static IUIElement CreateContentArea()
	{
		var contentLayout = new StackPanel
		{
			Orientation = Orientation.Horizontal,
			Spacing = 2
		};

		// Left column - Text styles
		var leftColumn = CreateTextStylesColumn();

		// Middle column - Border styles
		var middleColumn = CreateBorderStylesColumn();

		// Right column - Alignment and layouts
		var rightColumn = CreateAlignmentColumn();

		contentLayout.AddChild(leftColumn);
		contentLayout.AddChild(middleColumn);
		contentLayout.AddChild(rightColumn);

		return contentLayout;
	}

	private static IUIElement CreateTextStylesColumn()
	{
		var column = new StackPanel
		{
			Orientation = Orientation.Vertical,
			Spacing = 1
		};

		// Text styles panel
		var textStylesPanel = new BorderElement
		{
			Title = "Text Styles",
			BorderStyle = BorderStyle.Single,
			Child = new StackPanel
			{
				Orientation = Orientation.Vertical,
				Children =
				[
					new TextElement
					{
						Text = "Normal Text",
						Style = new TextStyle { Foreground = "white" }
					},
					new TextElement
					{
						Text = "Bold Text",
						Style = new TextStyle { IsBold = true, Foreground = "white" }
					},
					new TextElement
					{
						Text = "Italic Text",
						Style = new TextStyle { IsItalic = true, Foreground = "white" }
					},
					new TextElement
					{
						Text = "Underlined Text",
						Style = new TextStyle { IsUnderline = true, Foreground = "white" }
					},
					new TextElement
					{
						Text = "Bold Italic",
						Style = new TextStyle { IsBold = true, IsItalic = true, Foreground = "cyan" }
					}
				]
			}
		};

		// Color showcase panel
		var colorPanel = new BorderElement
		{
			Title = "Colors",
			BorderStyle = BorderStyle.Rounded,
			Child = new StackPanel
			{
				Orientation = Orientation.Vertical,
				Children =
				[
					new TextElement
					{
						Text = "Red Text",
						Style = new TextStyle { Foreground = "red" }
					},
					new TextElement
					{
						Text = "Green Text",
						Style = new TextStyle { Foreground = "green" }
					},
					new TextElement
					{
						Text = "Blue Text",
						Style = new TextStyle { Foreground = "blue" }
					},
					new TextElement
					{
						Text = "Yellow on Black",
						Style = new TextStyle { Foreground = "yellow", Background = "black" }
					},
					new TextElement
					{
						Text = "White on Red",
						Style = new TextStyle { Foreground = "white", Background = "red" }
					}
				]
			}
		};

		column.AddChild(textStylesPanel);
		column.AddChild(colorPanel);

		return column;
	}

	private static IUIElement CreateBorderStylesColumn()
	{
		var column = new StackPanel
		{
			Orientation = Orientation.Vertical,
			Spacing = 1
		};

		// Create examples of each border style
		var borderStyles = new[]
		{
			(BorderStyle.None, "None"),
			(BorderStyle.Single, "Single"),
			(BorderStyle.Double, "Double"),
			(BorderStyle.Rounded, "Rounded"),
			(BorderStyle.Thick, "Thick"),
			(BorderStyle.Ascii, "ASCII")
		};

		foreach (var (style, name) in borderStyles)
		{
			var borderExample = new BorderElement
			{
				Title = $"{name} Border",
				BorderStyle = style,
				Child = new TextElement
				{
					Text = $"Content with {name.ToLower(System.Globalization.CultureInfo.CurrentCulture)} border",
					HorizontalAlignment = HorizontalAlignment.Center,
					Style = new TextStyle { Foreground = "cyan" }
				}
			};

			column.AddChild(borderExample);
		}

		return column;
	}

	private static IUIElement CreateAlignmentColumn()
	{
		var column = new StackPanel
		{
			Orientation = Orientation.Vertical,
			Spacing = 1
		};

		// Horizontal alignment demo
		var horizontalAlignmentPanel = new BorderElement
		{
			Title = "Horizontal Alignment",
			BorderStyle = BorderStyle.Single,
			Child = new StackPanel
			{
				Orientation = Orientation.Vertical,
				Children =
				[
					new TextElement
					{
						Text = "Left Aligned",
						HorizontalAlignment = HorizontalAlignment.Left,
						Style = new TextStyle { Foreground = "green" }
					},
					new TextElement
					{
						Text = "Center Aligned",
						HorizontalAlignment = HorizontalAlignment.Center,
						Style = new TextStyle { Foreground = "yellow" }
					},
					new TextElement
					{
						Text = "Right Aligned",
						HorizontalAlignment = HorizontalAlignment.Right,
						Style = new TextStyle { Foreground = "red" }
					}
				]
			}
		};

		// Layout demo
		var layoutPanel = new BorderElement
		{
			Title = "Layout Demo",
			BorderStyle = BorderStyle.Thick,
			Child = new StackPanel
			{
				Orientation = Orientation.Vertical,
				Spacing = 1,
				Children =
				[
					new TextElement
					{
						Text = "Vertical Layout:",
						Style = new TextStyle { IsBold = true, Foreground = "white" }
					},
					new StackPanel
					{
						Orientation = Orientation.Horizontal,
						Spacing = 1,
						Children =
						[
							new TextElement
							{
								Text = "[A]",
								Style = new TextStyle { Background = "blue", Foreground = "white" }
							},
							new TextElement
							{
								Text = "[B]",
								Style = new TextStyle { Background = "red", Foreground = "white" }
							},
							new TextElement
							{
								Text = "[C]",
								Style = new TextStyle { Background = "green", Foreground = "white" }
							}
						]
					},
					new TextElement
					{
						Text = "Horizontal Layout ↑",
						Style = new TextStyle { Foreground = "gray" }
					}
				]
			}
		};

		// Padding demo
		var paddingPanel = new BorderElement
		{
			Title = "Padding Demo",
			BorderStyle = BorderStyle.Double,
			Child = new BorderElement
			{
				BorderStyle = BorderStyle.Single,
				Child = new TextElement
				{
					Text = "Nested with padding",
					HorizontalAlignment = HorizontalAlignment.Center,
					Style = new TextStyle { Foreground = "magenta" }
				},
				Padding = Padding.Uniform(2)
			}
		};

		column.AddChild(horizontalAlignmentPanel);
		column.AddChild(layoutPanel);
		column.AddChild(paddingPanel);

		return column;
	}

	private static IUIElement CreateFooter()
	{
		return new BorderElement
		{
			Title = "System Information",
			BorderStyle = BorderStyle.Ascii,
			Child = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Spacing = 3,
				Children =
				[
					new TextElement
					{
						Text = $"Platform: {Environment.OSVersion.Platform}",
						Style = new TextStyle { Foreground = "cyan" }
					},
					new TextElement
					{
						Text = $"Framework: {Environment.Version}",
						Style = new TextStyle { Foreground = "yellow" }
					},
					new TextElement
					{
						Text = $"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
						Style = new TextStyle { Foreground = "green" }
					},
					new TextElement
					{
						Text = "Press ESC to exit",
						Style = new TextStyle { Foreground = "red", IsBold = true }
					}
				]
			}
		};
	}
}
