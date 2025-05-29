// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;

namespace ktsu.TUI.App;

/// <summary>
/// A comprehensive sample application demonstrating advanced TUI library features
/// </summary>
public static class SampleApp
{
	/// <summary>
	/// Main entry point for the application
	/// </summary>
	public static async Task Main()
	{
		// Create console provider
		var consoleProvider = new SpectreConsoleProvider();

		// Create application using builder pattern
		var app = UIApplication.CreateBuilder(consoleProvider)
			.UseRootElement(CreateAdvancedDemoUI())
			.Build();

		// Run the application
		Console.WriteLine("Starting TUI Advanced Demo. Press ESC to exit.");
		await app.RunAsync();

		Console.WriteLine("TUI Advanced Demo finished.");
	}

	private static IUIElement CreateAdvancedDemoUI()
	{
		// Create main layout with horizontal orientation
		var mainLayout = new StackPanel
		{
			Orientation = Orientation.Horizontal,
			Spacing = 2,
			Padding = Padding.Uniform(1)
		};

		// Left panel - Information
		var leftPanel = CreateInfoPanel();

		// Right panel - Features showcase
		var rightPanel = CreateFeaturesPanel();

		mainLayout.AddChild(leftPanel);
		mainLayout.AddChild(rightPanel);

		return mainLayout;
	}

	private static IUIElement CreateInfoPanel()
	{
		var panel = new StackPanel
		{
			Orientation = Orientation.Vertical,
			Spacing = 1
		};

		// Header
		var header = new BorderElement
		{
			Title = "TUI Library",
			TitleAlignment = HorizontalAlignment.Center,
			BorderStyle = BorderStyle.Double,
			Child = new TextElement
			{
				Text = "Advanced Demo",
				HorizontalAlignment = HorizontalAlignment.Center,
				Style = new TextStyle
				{
					IsBold = true,
					Foreground = "green"
				}
			}
		};

		// Features list
		var features = new BorderElement
		{
			Title = "Features",
			BorderStyle = BorderStyle.Single,
			Child = new TextElement
			{
				Text = "✓ Modular Architecture\n" +
				       "✓ SOLID Principles\n" +
				       "✓ DRY Implementation\n" +
				       "✓ Extensible Design\n" +
				       "✓ Event-Driven Rendering\n" +
				       "✓ Input Handling\n" +
				       "✓ Layout System\n" +
				       "✓ Styling Support",
				Style = new TextStyle { Foreground = "cyan" }
			}
		};

		// Architecture info
		var architecture = new BorderElement
		{
			Title = "Architecture",
			BorderStyle = BorderStyle.Rounded,
			Child = new TextElement
			{
				Text = "Contracts → Models → Services\n" +
				       "Elements → Layouts → Primitives\n" +
				       "Provider Abstraction\n" +
				       "Dependency Injection Ready",
				Style = new TextStyle { Foreground = "magenta" }
			}
		};

		panel.AddChild(header);
		panel.AddChild(features);
		panel.AddChild(architecture);

		return panel;
	}

	private static IUIElement CreateFeaturesPanel()
	{
		var panel = new StackPanel
		{
			Orientation = Orientation.Vertical,
			Spacing = 1
		};

		// Styling showcase
		var stylingDemo = new BorderElement
		{
			Title = "Text Styling",
			BorderStyle = BorderStyle.Thick,
			Child = new StackPanel
			{
				Orientation = Orientation.Vertical,
				Children = new List<IUIElement>
				{
					new TextElement
					{
						Text = "Bold Text",
						Style = new TextStyle { IsBold = true }
					},
					new TextElement
					{
						Text = "Italic Text",
						Style = new TextStyle { IsItalic = true }
					},
					new TextElement
					{
						Text = "Underlined Text",
						Style = new TextStyle { IsUnderline = true }
					},
					new TextElement
					{
						Text = "Colored Text",
						Style = new TextStyle { Foreground = "red", Background = "yellow" }
					}
				}
			}
		};

		// Border styles showcase
		var borderDemo = new StackPanel
		{
			Orientation = Orientation.Horizontal,
			Spacing = 1,
			Children = new List<IUIElement>
			{
				new BorderElement
				{
					Title = "Single",
					BorderStyle = BorderStyle.Single,
					Child = new TextElement { Text = "Content" }
				},
				new BorderElement
				{
					Title = "Double",
					BorderStyle = BorderStyle.Double,
					Child = new TextElement { Text = "Content" }
				}
			}
		};

		// Instructions
		var instructions = new BorderElement
		{
			Title = "Controls",
			BorderStyle = BorderStyle.Ascii,
			Child = new TextElement
			{
				Text = "ESC - Exit Application\n" +
				       "Arrow Keys - Navigate (future)\n" +
				       "Enter - Select (future)\n" +
				       "Tab - Focus Next (future)",
				Style = new TextStyle
				{
					Foreground = "yellow",
					IsBold = true
				}
			}
		};

		panel.AddChild(stylingDemo);
		panel.AddChild(borderDemo);
		panel.AddChild(instructions);

		return panel;
	}
}
