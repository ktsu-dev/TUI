// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.App;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Elements;
using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;

/// <summary>
/// Interactive demo showcasing user input and dynamic updates
/// </summary>
/// <remarks>
/// State is per-instance rather than static so the key handling can be exercised without one test
/// inheriting the counter another left behind.
/// </remarks>
internal sealed class InteractiveDemo
{
	/// <summary>
	/// The counter's default accent colour
	/// </summary>
	private const string CounterColour = "cyan";

	/// <summary>
	/// The colour the counter takes when the theme is toggled
	/// </summary>
	private const string CounterAccentColour = "yellow";

	/// <summary>
	/// Initializes a new instance of the <see cref="InteractiveDemo"/> class, building its UI
	/// </summary>
	internal InteractiveDemo()
	{
		StatusText = new TextElement
		{
			Text = "Ready - Waiting for input...",
			Style = new TextStyle { Foreground = "green" },
			HorizontalAlignment = HorizontalAlignment.Center
		};

		CounterText = new TextElement
		{
			Text = "0",
			Style = new TextStyle { IsBold = true, Foreground = CounterColour },
			HorizontalAlignment = HorizontalAlignment.Center
		};

		RootElement = CreateInteractiveUI();
	}

	/// <summary>
	/// Gets the root element of the demo's UI
	/// </summary>
	internal IUIElement RootElement { get; }

	/// <summary>
	/// Gets the text element showing the most recent status message
	/// </summary>
	internal TextElement StatusText { get; }

	/// <summary>
	/// Gets the text element showing the counter value
	/// </summary>
	internal TextElement CounterText { get; }

	/// <summary>
	/// Gets the current counter value
	/// </summary>
	internal int Counter { get; private set; }

	/// <summary>
	/// Runs the interactive demo
	/// </summary>
	internal static async Task RunAsync()
	{
		InteractiveDemo demo = new();
		SpectreConsoleProvider consoleProvider = new();
		UIApplication app = UIApplication.CreateBuilder(consoleProvider)
			.UseRootElement(demo.RootElement)
			.Build();

		Console.WriteLine("Starting TUI Interactive Demo. Use the controls shown on screen!");
		await app.RunAsync().ConfigureAwait(false);
		Console.WriteLine("Interactive Demo finished.");
	}

	private InputRoutingPanel CreateInteractiveUI()
	{
		InputRoutingPanel mainLayout = new(this)
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
			BorderStyle = BorderStyle.DoubleLine,
			Child = new TextElement
			{
				Text = "Real-time UI Updates & Input Handling",
				HorizontalAlignment = HorizontalAlignment.Center,
				Style = new TextStyle { IsBold = true, Foreground = "yellow" }
			}
		};

		// Status panel
		BorderElement statusPanel = CreateStatusPanel();

		// Counter panel
		BorderElement counterPanel = CreateCounterPanel();

		// Input instructions
		BorderElement instructionsPanel = CreateInstructionsPanel();

		mainLayout.AddChild(header);
		mainLayout.AddChild(statusPanel);
		mainLayout.AddChild(counterPanel);
		mainLayout.AddChild(instructionsPanel);

		return mainLayout;
	}

	private BorderElement CreateStatusPanel() =>
		new()
		{
			Title = "Status",
			BorderStyle = BorderStyle.SingleLine,
			Child = StatusText
		};

	private BorderElement CreateCounterPanel()
	{
		StackPanel counterLayout = new()
		{
			Orientation = Orientation.Horizontal,
			Spacing = 2
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
		counterLayout.AddChild(CounterText);
		counterLayout.AddChild(resetButton);

		return new BorderElement
		{
			Title = "Interactive Counter",
			BorderStyle = BorderStyle.Rounded,
			Child = counterLayout
		};
	}

	/// <summary>
	/// One control the instructions panel advertises
	/// </summary>
	/// <param name="Label">How the control is written on screen</param>
	/// <param name="Description">What the control does</param>
	/// <param name="Keys">The keys that trigger it</param>
	internal sealed record Control(string Label, string Description, IReadOnlyList<ConsoleKey> Keys);

	/// <summary>
	/// Gets the controls the instructions panel advertises
	/// </summary>
	/// <remarks>
	/// The panel is rendered from this list rather than from a separate literal, so a control cannot
	/// be advertised on screen without naming the key that <see cref="HandleInput"/> has to act on.
	/// Every key here was advertised and ignored before ktsu-dev/TUI#115.
	/// </remarks>
	internal static IReadOnlyList<Control> Controls { get; } =
	[
		new("↑/↓", "Increment/Decrement Counter", [ConsoleKey.UpArrow, ConsoleKey.DownArrow]),
		new("SPACE", "Add +10 to Counter", [ConsoleKey.Spacebar]),
		new("R", "Reset Counter", [ConsoleKey.R]),
		new("T", "Toggle Theme", [ConsoleKey.T])
	];

	/// <summary>
	/// Gets the text shown in the instructions panel
	/// </summary>
	/// <remarks>
	/// Exit is listed separately because it is not the demo's to handle: Escape reaches
	/// <see cref="UIApplication.ProcessInputAsync"/> as an exit and never reaches an element.
	/// </remarks>
	internal static string InstructionsText { get; } =
		"CONTROLS:\n" +
		string.Join("\n", Controls.Select(c => $"{c.Label,-5} - {c.Description}")) + "\n" +
		"ESC   - Exit Demo\n\n" +
		"Watch the status and counter update in real-time!";

	private static BorderElement CreateInstructionsPanel() =>
		new()
		{
			Title = "Instructions",
			BorderStyle = BorderStyle.Thick,
			Child = new TextElement
			{
				Text = InstructionsText,
				Style = new TextStyle { Foreground = "magenta" }
			}
		};

	/// <summary>
	/// Applies a key press to the demo's state
	/// </summary>
	/// <param name="input">The input to handle</param>
	/// <returns>True when the key was one of the advertised controls, false otherwise</returns>
	/// <remarks>
	/// Only <see cref="InputType.Keyboard"/> input is acted on, because that is all
	/// <see cref="SpectreConsoleProvider.ReadInputAsync"/> produces — every key arrives as
	/// <see cref="InputResult.FromKey"/>, and Escape arrives as an exit that
	/// <see cref="UIApplication.ProcessInputAsync"/> consumes before reaching any element.
	/// </remarks>
	internal bool HandleInput(InputResult input)
	{
		if (input.Type != InputType.Keyboard || input.Key is not ConsoleKey key)
		{
			return false;
		}

		switch (key)
		{
			case ConsoleKey.UpArrow:
				Counter++;
				UpdateCounter();
				UpdateStatus("Counter incremented!");
				return true;

			case ConsoleKey.DownArrow:
				Counter--;
				UpdateCounter();
				UpdateStatus("Counter decremented!");
				return true;

			case ConsoleKey.Spacebar:
				Counter += 10;
				UpdateCounter();
				UpdateStatus("Added +10 to counter!");
				return true;

			case ConsoleKey.R:
				Counter = 0;
				UpdateCounter();
				UpdateStatus("Counter reset to 0!");
				return true;

			case ConsoleKey.T:
				ToggleTheme();
				UpdateStatus("Theme toggled!");
				return true;

			default:
				// Reported on screen so an unrecognised key is visibly acknowledged, but still
				// unhandled as far as the application is concerned — nothing was acted on.
				UpdateStatus($"Unknown key: {key}");
				return false;
		}
	}

	private void UpdateCounter() => CounterText.Text = Counter.ToString();

	private void UpdateStatus(string message) =>
		StatusText.Text = $"{DateTime.Now:HH:mm:ss} - {message}";

	/// <summary>
	/// Swaps the counter between its two accent colours
	/// </summary>
	/// <remarks>
	/// The comparison is case-insensitive because <see cref="TextStyle.Foreground"/> round-trips
	/// through <see cref="System.Drawing.Color"/> and its getter returns the canonical name, so a
	/// style set from "cyan" reads back as "Cyan". Comparing against the lowercase literal never
	/// matched, which left the counter on one colour however many times T was pressed.
	/// </remarks>
	private void ToggleTheme()
	{
		TextStyle currentStyle = CounterText.Style;
		bool isCyan = string.Equals(currentStyle.Foreground, CounterColour, StringComparison.OrdinalIgnoreCase);

		CounterText.Style = new TextStyle
		{
			IsBold = currentStyle.IsBold,
			Foreground = isCyan ? CounterAccentColour : CounterColour
		};
	}

	/// <summary>
	/// The demo's root layout, which routes keys no element consumed to <see cref="HandleInput"/>
	/// </summary>
	/// <remarks>
	/// <see cref="UIApplication.ProcessInputAsync"/> offers each key to the root element and nothing
	/// else, and <see cref="UIContainerBase.HandleInput"/> passes it down to the children before
	/// offering it to the container itself. None of the shipped element types override
	/// <see cref="UIElementBase.OnHandleInput"/> — it returns false — so an application that wants
	/// keys supplies an element that does. Without one, every control the instructions panel
	/// advertises did nothing (ktsu-dev/TUI#115).
	/// </remarks>
	/// <param name="demo">The demo whose state the keys act on</param>
	private sealed class InputRoutingPanel(InteractiveDemo demo) : StackPanel
	{
		/// <inheritdoc />
		protected override bool OnHandleInput(InputResult input) => demo.HandleInput(input);
	}
}
