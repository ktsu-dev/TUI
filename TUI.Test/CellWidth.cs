// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using Spectre.Console;

/// <summary>
/// Measures text in terminal cells with Spectre.Console's own cell-width function, independently
/// of the measure the elements under test use.
/// </summary>
internal static class CellWidth
{
	/// <summary>
	/// Gets the number of terminal cells <paramref name="text"/> occupies.
	/// </summary>
	/// <param name="text">The text to measure.</param>
	/// <returns>The cell width.</returns>
	internal static int Of(string text) => text.GetCellWidth();
}
