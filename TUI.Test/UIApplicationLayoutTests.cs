// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests that <see cref="UIApplication"/> lays the tree out on every render pass.
/// </summary>
/// <remarks>
/// Layout used to run only when the root had no size yet or the terminal was resized, so a change
/// to the tree after the first frame kept its stale geometry (ktsu-dev/TUI#133).
/// </remarks>
[TestClass]
public sealed class UIApplicationLayoutTests
{
	/// <summary>
	/// An element made visible after the first frame must be laid out and drawn on the next one.
	/// It used to keep its default position and empty size, so it was never drawn.
	/// </summary>
	[TestMethod]
	public void RenderAfterShowingAHiddenElementDrawsIt()
	{
		// Arrange
		RecordingConsoleProvider provider = new();
		TextElement a = new("a");
		TextElement b = new("b") { IsVisible = false };
		StackPanel root = [a, b];
		UIApplication app = new(provider) { RootElement = root };
		app.Render();

		// Act
		b.IsVisible = true;
		app.Render();

		// Assert
		Assert.Contains(
			w => w.Text == "b" && w.Position == new Position(0, 1),
			provider.Writes,
			"The newly shown element should be drawn on the row below its sibling");
	}

	/// <summary>
	/// A root the host sized itself must still have its children laid out. The arrange used to be
	/// skipped because the root already had a size, so its children had none and nothing was drawn.
	/// </summary>
	[TestMethod]
	public void RenderWithAHostAssignedRootSizeDrawsItsChildren()
	{
		// Arrange
		RecordingConsoleProvider provider = new();
		StackPanel root = [new TextElement("abc")];
		root.Dimensions = new Dimensions(20, 5);
		UIApplication app = new(provider) { RootElement = root };

		// Act
		app.Render();

		// Assert
		Assert.Contains(w => w.Text == "abc", provider.Writes, "The child of a host-sized root should be drawn");
		Assert.AreEqual(new Dimensions(20, 5), root.Dimensions, "The host-assigned size should be kept");
	}

	/// <summary>
	/// Text that grows after the first frame must push its siblings along on the next one.
	/// </summary>
	[TestMethod]
	public void RenderAfterTextGrowsMovesTheFollowingSibling()
	{
		// Arrange
		RecordingConsoleProvider provider = new();
		TextElement first = new("ab");
		TextElement second = new("cd");
		StackPanel root = new(Orientation.Horizontal) { first, second };
		UIApplication app = new(provider) { RootElement = root };
		app.Render();
		int before = second.Position.X;

		// Act
		first.Text = "abcdef";
		app.Render();

		// Assert
		Assert.AreEqual(before + 4, second.Position.X, "The sibling should move right by the width the text gained");
	}
}
