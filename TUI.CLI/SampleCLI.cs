// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;

namespace ktsu.TUI.CLI;

/// <summary>
/// A sample CLI application demonstrating the TUI library
/// </summary>
public static class SampleCLI
{
	/// <summary>
	/// Main entry point for the CLI application
	/// </summary>
	public static async Task Main()
	{
		// Create console provider
		var consoleProvider = new SpectreConsoleProvider();

		// Create application
		var app = new UIApplication(consoleProvider);

		// Create a demo UI
		var rootPanel = CreateDemoUI();
		app.Setup(rootPanel);

		// Run the application
		Console.WriteLine("Starting TUI CLI Demo. Press ESC to exit.");
		await app.RunAsync();

		Console.WriteLine("TUI CLI Demo finished.");
	}

	private static IUIElement CreateDemoUI()
	{
		// Create main container
		var mainPanel = new StackPanel
		{
			Orientation = Orientation.Vertical,
			Spacing = 1,
			Padding = Padding.Uniform(2)
		};

		// Add title
		var title = new BorderElement
		{
			Title = "TUI Library CLI Demo",
			TitleAlignment = HorizontalAlignment.Center,
							BorderStyle = BorderStyle.DoubleLine,
			Child = new TextElement
			{
				Text = "Welcome to the TUI Library!",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Style = new TextStyle { IsBold = true }
			}
		};

		// Add description
		var description = new TextElement
		{
			Text = "This demonstrates the TUI library's capabilities:\n" +
			       "• Text rendering with styling\n" +
			       "• Border elements with titles\n" +
			       "• Layout containers (StackPanel)\n" +
			       "• Padding and spacing\n" +
			       "• Input handling",
			Style = new TextStyle { Foreground = "cyan" }
		};

		// Add instructions
		var instructions = new BorderElement
		{
			Title = "Instructions",
							BorderStyle = BorderStyle.SingleLine,
			Child = new TextElement
			{
				Text = "Press ESC to exit the application",
				HorizontalAlignment = HorizontalAlignment.Center,
				Style = new TextStyle { Foreground = "yellow", IsBold = true }
			}
		};

		// Add all elements to main panel
		mainPanel.AddChild(title);
		mainPanel.AddChild(description);
		mainPanel.AddChild(instructions);

		return mainPanel;
	}
}
