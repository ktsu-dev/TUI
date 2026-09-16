// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.App;
using ktsu.TUI.Core.Models;

/// <summary>
/// Covers the interactive demo's keyboard controls, which are advertised by its instructions panel
/// </summary>
/// <remarks>
/// These go through <see cref="InteractiveDemo.RootElement"/> rather than calling the handler
/// directly, because the bug (ktsu-dev/TUI#115) was not in the handler — it was that nothing in the
/// element tree ever reached it. A test that calls <c>HandleInput</c> itself passes either way.
/// </remarks>
[TestClass]
public class InteractiveDemoInputTests
{
	private static bool PressKey(InteractiveDemo demo, ConsoleKey key) =>
		demo.RootElement.HandleInput(InputResult.FromKey(key));

	[TestMethod]
	public void UpArrowIncrementsTheCounter()
	{
		InteractiveDemo demo = new();

		Assert.IsTrue(PressKey(demo, ConsoleKey.UpArrow));

		Assert.AreEqual(1, demo.Counter);
		Assert.AreEqual("1", demo.CounterText.Text);
		Assert.Contains("Counter incremented!", demo.StatusText.Text, StringComparison.Ordinal);
	}

	[TestMethod]
	public void DownArrowDecrementsTheCounter()
	{
		InteractiveDemo demo = new();

		Assert.IsTrue(PressKey(demo, ConsoleKey.DownArrow));

		Assert.AreEqual(-1, demo.Counter);
		Assert.AreEqual("-1", demo.CounterText.Text);
		Assert.Contains("Counter decremented!", demo.StatusText.Text, StringComparison.Ordinal);
	}

	[TestMethod]
	public void SpaceAddsTenToTheCounter()
	{
		InteractiveDemo demo = new();

		Assert.IsTrue(PressKey(demo, ConsoleKey.Spacebar));

		Assert.AreEqual(10, demo.Counter);
		Assert.AreEqual("10", demo.CounterText.Text);
		Assert.Contains("Added +10 to counter!", demo.StatusText.Text, StringComparison.Ordinal);
	}

	[TestMethod]
	public void RResetsTheCounter()
	{
		InteractiveDemo demo = new();
		PressKey(demo, ConsoleKey.Spacebar);

		Assert.IsTrue(PressKey(demo, ConsoleKey.R));

		Assert.AreEqual(0, demo.Counter);
		Assert.AreEqual("0", demo.CounterText.Text);
		Assert.Contains("Counter reset to 0!", demo.StatusText.Text, StringComparison.Ordinal);
	}

	[TestMethod]
	public void TTogglesTheCounterColourBackAndForth()
	{
		// TextStyle.Foreground round-trips through System.Drawing.Color, so it reads back in the
		// canonical casing rather than the literal the style was built from.
		InteractiveDemo demo = new();
		Assert.AreEqual("Cyan", demo.CounterText.Style.Foreground);

		Assert.IsTrue(PressKey(demo, ConsoleKey.T));
		Assert.AreEqual("Yellow", demo.CounterText.Style.Foreground);
		Assert.Contains("Theme toggled!", demo.StatusText.Text, StringComparison.Ordinal);

		Assert.IsTrue(PressKey(demo, ConsoleKey.T));
		Assert.AreEqual("Cyan", demo.CounterText.Style.Foreground);
	}

	[TestMethod]
	public void TKeepsTheCounterBold()
	{
		InteractiveDemo demo = new();

		PressKey(demo, ConsoleKey.T);

		Assert.IsTrue(demo.CounterText.Style.IsBold);
	}

	[TestMethod]
	public void RepeatedPressesAccumulate()
	{
		InteractiveDemo demo = new();

		PressKey(demo, ConsoleKey.UpArrow);
		PressKey(demo, ConsoleKey.UpArrow);
		PressKey(demo, ConsoleKey.Spacebar);
		PressKey(demo, ConsoleKey.DownArrow);

		Assert.AreEqual(11, demo.Counter);
		Assert.AreEqual("11", demo.CounterText.Text);
	}

	[TestMethod]
	public void AnUnadvertisedKeyIsReportedAndLeavesTheCounterAlone()
	{
		InteractiveDemo demo = new();

		Assert.IsFalse(PressKey(demo, ConsoleKey.X));

		Assert.AreEqual(0, demo.Counter);
		Assert.Contains("Unknown key: X", demo.StatusText.Text, StringComparison.Ordinal);
	}

	[TestMethod]
	public void NonKeyboardInputIsIgnored()
	{
		InteractiveDemo demo = new();
		string statusBefore = demo.StatusText.Text;

		Assert.IsFalse(demo.RootElement.HandleInput(InputResult.FromMouse(new Position(1, 1))));

		Assert.AreEqual(0, demo.Counter);
		Assert.AreEqual(statusBefore, demo.StatusText.Text);
	}

	[TestMethod]
	public void EachDemoInstanceStartsFromAFreshCounter()
	{
		InteractiveDemo first = new();
		PressKey(first, ConsoleKey.Spacebar);

		InteractiveDemo second = new();

		Assert.AreEqual(10, first.Counter);
		Assert.AreEqual(0, second.Counter);
	}

	[TestMethod]
	public void EveryControlTheInstructionsPanelAdvertisesIsHandled()
	{
		// The panel is the contract the user reads, so its claims and the handler are asserted
		// against each other rather than each being checked on its own.
		InteractiveDemo demo = new();

		Assert.IsNotEmpty(InteractiveDemo.Controls);

		foreach (InteractiveDemo.Control control in InteractiveDemo.Controls)
		{
			Assert.Contains(control.Label, InteractiveDemo.InstructionsText, StringComparison.Ordinal);
			Assert.Contains(control.Description, InteractiveDemo.InstructionsText, StringComparison.Ordinal);

			foreach (ConsoleKey key in control.Keys)
			{
				Assert.IsTrue(
					PressKey(demo, key),
					$"The instructions panel advertises '{control.Label}', but {key} was not handled.");
			}
		}
	}
}
