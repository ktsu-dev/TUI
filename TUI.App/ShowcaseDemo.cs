// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.App;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;

/// <summary>
/// Comprehensive showcase of all TUI library visual features
/// </summary>
internal static class ShowcaseDemo
{
	/// <summary>
	/// Runs the showcase demo
	/// </summary>
	internal static async Task RunAsync()
	{
		SpectreConsoleProvider consoleProvider = new();
		UIApplication app = UIApplication.CreateBuilder(consoleProvider)
			.UseRootElement(CreateShowcaseUI())
			.Build();

		Console.WriteLine("Starting TUI Showcase Demo. Displaying all visual features...");
		await app.RunAsync().ConfigureAwait(false);
		Console.WriteLine("Showcase Demo finished.");
	}

	internal static StackPanel CreateShowcaseUI()
	{
		StackPanel mainLayout = new()
		{
			Orientation = Orientation.Vertical,
			Spacing = 1,
			Padding = Padding.Uniform(1)
		};

		// Header
		BorderElement header = CreateHeader();

		// Content area with three columns
		StackPanel contentArea = CreateContentArea();

		// Footer
		BorderElement footer = CreateFooter();

		mainLayout.AddChild(header);
		mainLayout.AddChild(contentArea);
		mainLayout.AddChild(footer);

		return mainLayout;
	}

	private static BorderElement CreateHeader()
	{
		BorderElement header = new()
		{
			Title = "TUI Library Showcase",
			TitleAlignment = HorizontalAlignment.Center,
			BorderStyle = BorderStyle.DoubleLine,
			Child = new StackPanel
			{
				Orientation = Orientation.Vertical
			}
		};

		// Add children to header panel
		StackPanel headerPanel = (StackPanel)header.Child;
		headerPanel.AddChild(new TextElement
		{
			Text = "Complete Visual Features Demonstration",
			HorizontalAlignment = HorizontalAlignment.Center,
			Style = new TextStyle { IsBold = true, Foreground = "yellow" }
		});
		headerPanel.AddChild(new TextElement
		{
			Text = "Text Styles • Borders • Layouts • Alignments • Colors",
			HorizontalAlignment = HorizontalAlignment.Center,
			Style = new TextStyle { Foreground = "gray" }
		});

		return header;
	}

	private static StackPanel CreateContentArea()
	{
		StackPanel contentLayout = new()
		{
			Orientation = Orientation.Horizontal,
			Spacing = 2
		};

		// Left column - Text styles
		StackPanel leftColumn = CreateTextStylesColumn();

		// Middle column - Border styles
		StackPanel middleColumn = CreateBorderStylesColumn();

		// Right column - Alignment and layouts
		StackPanel rightColumn = CreateAlignmentColumn();

		contentLayout.AddChild(leftColumn);
		contentLayout.AddChild(middleColumn);
		contentLayout.AddChild(rightColumn);

		return contentLayout;
	}

	private static StackPanel CreateTextStylesColumn()
	{
		StackPanel column = new()
		{
			Orientation = Orientation.Vertical,
			Spacing = 1
		};

		// Text styles panel
		BorderElement textStylesPanel = new()
		{
			Title = "Text Styles",
			BorderStyle = BorderStyle.SingleLine,
			Child = Panel(Orientation.Vertical, 0,
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
					})
		};

		// Color showcase panel
		BorderElement colorPanel = new()
		{
			Title = "Colors",
			BorderStyle = BorderStyle.Rounded,
			Child = Panel(Orientation.Vertical, 0,
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
					})
		};

		column.AddChild(textStylesPanel);
		column.AddChild(colorPanel);

		return column;
	}

	private static StackPanel CreateBorderStylesColumn()
	{
		StackPanel column = new()
		{
			Orientation = Orientation.Vertical,
			Spacing = 1
		};

		// Create examples of each border style
		(BorderStyle Style, string Name)[] borderStyles =
		[
			(BorderStyle.None, "None"),
			(BorderStyle.SingleLine, "Single"),
			(BorderStyle.DoubleLine, "Double"),
			(BorderStyle.Rounded, "Rounded"),
			(BorderStyle.Thick, "Thick"),
			(BorderStyle.Ascii, "ASCII")
		];

		foreach ((BorderStyle style, string name) in borderStyles)
		{
			BorderElement borderExample = new()
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

	private static StackPanel CreateAlignmentColumn()
	{
		StackPanel column = new()
		{
			Orientation = Orientation.Vertical,
			Spacing = 1
		};

		// Horizontal alignment demo
		BorderElement horizontalAlignmentPanel = new()
		{
			Title = "Horizontal Alignment",
			BorderStyle = BorderStyle.SingleLine,
			Child = Panel(Orientation.Vertical, 0,
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
					})
		};

		// Layout demo
		BorderElement layoutPanel = new()
		{
			Title = "Layout Demo",
			BorderStyle = BorderStyle.Thick,
			Child = Panel(Orientation.Vertical, 1,
					new TextElement
					{
						Text = "Vertical Layout:",
						Style = new TextStyle { IsBold = true, Foreground = "white" }
					},
					Panel(Orientation.Horizontal, 1,
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
							}),
					new TextElement
					{
						Text = "Horizontal Layout ↑",
						Style = new TextStyle { Foreground = "gray" }
					})
		};

		// Padding demo
		BorderElement paddingPanel = new()
		{
			Title = "Padding Demo",
			BorderStyle = BorderStyle.DoubleLine,
			Child = new BorderElement
			{
				BorderStyle = BorderStyle.SingleLine,
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

	private static BorderElement CreateFooter()
	{
		return new BorderElement
		{
			Title = "System Information",
			BorderStyle = BorderStyle.Ascii,
			Child = Panel(Orientation.Horizontal, 3,
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
					})
		};
	}

	private static StackPanel Panel(Orientation orientation, int spacing, params IUIElement[] children)
	{
		StackPanel panel = new(orientation) { Spacing = spacing };
		foreach (IUIElement child in children)
		{
			panel.AddChild(child);
		}

		return panel;
	}
}
