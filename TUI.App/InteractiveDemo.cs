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
/// Interactive demo showcasing user input and dynamic updates
/// </summary>
internal static class InteractiveDemo
{
	private static TextElement? _statusText;
	private static TextElement? _counterText;
	private static int _counter;

	/// <summary>
	/// Runs the interactive demo
	/// </summary>
	public static async Task RunAsync()
	{
		SpectreConsoleProvider consoleProvider = new();
		UIApplication app = UIApplication.CreateBuilder(consoleProvider)
			.UseRootElement(CreateInteractiveUI())
			.Build();

		Console.WriteLine("Starting TUI Interactive Demo. Use the controls shown on screen!");
		await app.RunAsync();
		Console.WriteLine("Interactive Demo finished.");
	}

	private static IUIElement CreateInteractiveUI()
	{
		StackPanel mainLayout = new()
		{
			Orientation = Orientation.Vertical,
			Spacing = 1,
			Padding = Padding.Uniform(2)
		};

		// Header
		BorderElement header = new()
		{
			Title = "Interactive TUI Demo",
			TitleAlignment = HorizontalAlignment.Center,
			BorderStyle = BorderStyle.Double,
			Child = new TextElement
			{
				Text = "Real-time UI Updates & Input Handling",
				HorizontalAlignment = HorizontalAlignment.Center,
				Style = new TextStyle { IsBold = true, Foreground = "yellow" }
			}
		};

		// Status panel
		IUIElement statusPanel = CreateStatusPanel();

		// Counter panel
		IUIElement counterPanel = CreateCounterPanel();

		// Input instructions
		IUIElement instructionsPanel = CreateInstructionsPanel();

		mainLayout.AddChild(header);
		mainLayout.AddChild(statusPanel);
		mainLayout.AddChild(counterPanel);
		mainLayout.AddChild(instructionsPanel);

		return mainLayout;
	}

	private static IUIElement CreateStatusPanel()
	{
		_statusText = new TextElement
		{
			Text = "Ready - Waiting for input...",
			Style = new TextStyle { Foreground = "green" },
			HorizontalAlignment = HorizontalAlignment.Center
		};

		return new BorderElement
		{
			Title = "Status",
			BorderStyle = BorderStyle.Single,
			Child = _statusText
		};
	}

	private static IUIElement CreateCounterPanel()
	{
		StackPanel counterLayout = new()
		{
			Orientation = Orientation.Horizontal,
			Spacing = 2
		};

		_counterText = new TextElement
		{
			Text = "0",
			Style = new TextStyle { IsBold = true, Foreground = "cyan" },
			HorizontalAlignment = HorizontalAlignment.Center
		};

		TextElement counterLabel = new()
		{
			Text = "Counter:",
			Style = new TextStyle { Foreground = "white" }
		};

		TextElement resetButton = new()
		{
			Text = "[R] Reset",
			Style = new TextStyle { Foreground = "red" }
		};

		counterLayout.AddChild(counterLabel);
		counterLayout.AddChild(_counterText);
		counterLayout.AddChild(resetButton);

		return new BorderElement
		{
			Title = "Interactive Counter",
			BorderStyle = BorderStyle.Rounded,
			Child = counterLayout
		};
	}

	private static IUIElement CreateInstructionsPanel()
	{
		TextElement instructionsText = new()
		{
			Text = "CONTROLS:\n" +
				   "↑/↓  - Increment/Decrement Counter\n" +
				   "SPACE - Add +10 to Counter\n" +
				   "R     - Reset Counter\n" +
				   "T     - Toggle Theme\n" +
				   "ESC   - Exit Demo\n\n" +
				   "Watch the status and counter update in real-time!",
			Style = new TextStyle { Foreground = "magenta" }
		};

		return new BorderElement
		{
			Title = "Instructions",
			BorderStyle = BorderStyle.Thick,
			Child = instructionsText
		};
	}

	/// <summary>
	/// Simulates input handling (this would be connected to actual input in a full implementation)
	/// </summary>
	/// <param name="key">The key that was pressed</param>
	public static void HandleInput(string key)
	{
		switch (key.ToUpperInvariant())
		{
			case "UP":
				_counter++;
				UpdateCounter();
				UpdateStatus("Counter incremented!");
				break;

			case "DOWN":
				_counter--;
				UpdateCounter();
				UpdateStatus("Counter decremented!");
				break;

			case " ":
			case "SPACE":
				_counter += 10;
				UpdateCounter();
				UpdateStatus("Added +10 to counter!");
				break;

			case "R":
				_counter = 0;
				UpdateCounter();
				UpdateStatus("Counter reset to 0!");
				break;

			case "T":
				ToggleTheme();
				UpdateStatus("Theme toggled!");
				break;

			default:
				UpdateStatus($"Unknown key: {key}");
				break;
		}
	}

	private static void UpdateCounter()
	{
		if (_counterText != null)
		{
			_counterText.Text = _counter.ToString();
		}
	}

	private static void UpdateStatus(string message)
	{
		if (_statusText != null)
		{
			_statusText.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
		}
	}

	private static void ToggleTheme()
	{
		if (_counterText != null)
		{
			TextStyle currentStyle = _counterText.Style;
			_counterText.Style = new TextStyle
			{
				IsBold = currentStyle.IsBold,
				Foreground = currentStyle.Foreground == "cyan" ? "yellow" : "cyan"
			};
		}
	}
}
