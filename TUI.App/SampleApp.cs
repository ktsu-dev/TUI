// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.App;

using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;

/// <summary>
/// A comprehensive sample application demonstrating advanced TUI library features
/// </summary>
internal static class SampleApp
{
	/// <summary>
	/// Run the sample application
	/// </summary>
	internal static async Task RunAsync()
	{
		// Create console provider
		SpectreConsoleProvider consoleProvider = new();

		// Create application using builder pattern
		UIApplication app = UIApplication.CreateBuilder(consoleProvider)
			.UseRootElement(CreateAdvancedDemoUI())
			.Build();

		// Run the application
		Console.WriteLine("Starting TUI Advanced Demo. Press ESC to exit.");
		await app.RunAsync().ConfigureAwait(false);

		Console.WriteLine("TUI Advanced Demo finished.");
	}

	private static StackPanel CreateAdvancedDemoUI()
	{
		// Create main layout with horizontal orientation
		StackPanel mainLayout = new()
		{
			Orientation = Orientation.Horizontal,
			Spacing = 2,
			Padding = Padding.Uniform(1)
		};

		// Left panel - Information
		StackPanel leftPanel = CreateInfoPanel();

		// Right panel - Features showcase
		StackPanel rightPanel = CreateFeaturesPanel();

		mainLayout.AddChild(leftPanel);
		mainLayout.AddChild(rightPanel);

		return mainLayout;
	}

	private static StackPanel CreateInfoPanel()
	{
		StackPanel panel = new()
		{
			Orientation = Orientation.Vertical,
			Spacing = 1
		};

		// Header
		BorderElement header = new()
		{
			Title = "TUI Library",
			TitleAlignment = HorizontalAlignment.Center,
			BorderStyle = BorderStyle.DoubleLine,
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
		BorderElement features = new()
		{
			Title = "Features",
			BorderStyle = BorderStyle.SingleLine,
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
		BorderElement architecture = new()
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

	private static StackPanel CreateFeaturesPanel()
	{
		StackPanel panel = new()
		{
			Orientation = Orientation.Vertical,
			Spacing = 1
		};

		// Styling showcase
		BorderElement stylingDemo = new()
		{
			Title = "Text Styling",
			BorderStyle = BorderStyle.Thick,
			Child = new StackPanel
			{
				Orientation = Orientation.Vertical
			}
		};

		// Add children to the styling demo
		StackPanel stylingPanel = (StackPanel)stylingDemo.Child;
		stylingPanel.AddChild(new TextElement
		{
			Text = "Bold Text",
			Style = new TextStyle { IsBold = true }
		});
		stylingPanel.AddChild(new TextElement
		{
			Text = "Italic Text",
			Style = new TextStyle { IsItalic = true }
		});
		stylingPanel.AddChild(new TextElement
		{
			Text = "Underlined Text",
			Style = new TextStyle { IsUnderline = true }
		});
		stylingPanel.AddChild(new TextElement
		{
			Text = "Colored Text",
			Style = new TextStyle { Foreground = "red", Background = "yellow" }
		});

		// Border styles showcase
		StackPanel borderDemo = new()
		{
			Orientation = Orientation.Horizontal,
			Spacing = 1
		};

		borderDemo.AddChild(new BorderElement
		{
			Title = "Single",
			BorderStyle = BorderStyle.SingleLine,
			Child = new TextElement { Text = "Content" }
		});
		borderDemo.AddChild(new BorderElement
		{
			Title = "Double",
			BorderStyle = BorderStyle.DoubleLine,
			Child = new TextElement { Text = "Content" }
		});

		// Instructions
		BorderElement instructions = new()
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
