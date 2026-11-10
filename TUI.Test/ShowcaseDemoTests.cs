// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.App;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;

/// <summary>
/// Smoke tests for the showcase demo that <c>--showcase</c> runs.
/// </summary>
/// <remarks>
/// The demo was excluded from the build because it no longer compiled, and <c>--showcase</c> quietly
/// ran the sample app instead (ktsu-dev/TUI#179). Building and drawing its tree here keeps it
/// type-checked against the library, so it can't drift out of the build again unnoticed.
/// </remarks>
[TestClass]
public sealed class ShowcaseDemoTests
{
	[TestMethod]
	public void ShowcaseTreeRendersEveryPanel()
	{
		RecordingConsoleProvider provider = new() { Dimensions = new Dimensions(160, 60) };
		UIApplication app = new(provider) { RootElement = ShowcaseDemo.CreateShowcaseUI() };

		app.Render();

		string drawn = string.Join('\n', provider.Writes.Select(w => w.Text));
		foreach (string expected in new[]
		{
			"TUI Library Showcase",
			"Complete Visual Features Demonstration",
			"Bold Text",
			"Red Text",
			"Center Aligned",
			"[A]",
			"Press ESC to exit",
		})
		{
			Assert.Contains(expected, drawn, StringComparison.Ordinal, $"the showcase should draw \"{expected}\"");
		}
	}
}
