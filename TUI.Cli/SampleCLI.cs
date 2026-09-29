// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Cli;

using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;

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
		SpectreConsoleProvider consoleProvider = new();

		// Create application
		UIApplication app = new(consoleProvider);

		// Create a demo UI
		StackPanel rootPanel = CreateDemoUI();
		app.Setup(rootPanel);

		// Run the application
		Console.WriteLine("Starting TUI CLI Demo. Press ESC to exit.");
		await app.RunAsync().ConfigureAwait(false);

		Console.WriteLine("TUI CLI Demo finished.");
	}

	private static StackPanel CreateDemoUI()
	{
		// Create main container
		StackPanel mainPanel = new()
		{
			Orientation = Orientation.Vertical,
			Spacing = 1,
			Padding = Padding.Uniform(2)
		};

		// Add title
		BorderElement title = new()
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
		TextElement description = new()
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
		BorderElement instructions = new()
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
