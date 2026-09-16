// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using ktsu.TUI.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Render-loop tests for <see cref="UIApplication"/>.
/// </summary>
/// <remarks>
/// These cover the rendering model rather than any single element, which is where
/// ktsu-dev/TUI#109 lived: <see cref="UIApplication.Render"/> clears the whole screen on every
/// pass, but elements used to draw only while they were still flagged dirty — so any element the
/// last input did not invalidate was wiped and never redrawn.
/// </remarks>
[TestClass]
public sealed class UIApplicationTests
{
	/// <summary>
	/// Builds the layout from the issue: a titled border around a static label and an
	/// interactive sibling.
	/// </summary>
	/// <param name="interactive">The interactive sibling, which invalidates itself on input.</param>
	/// <returns>The root element, sized and arranged.</returns>
	private static BorderElement CreateLayout(out TextElement interactive)
	{
		TextElement staticLabel = new("Label");
		interactive = new TextElement("Input");

		StackPanel panel = [];
		panel.AddChild(staticLabel);
		panel.AddChild(interactive);

		BorderElement border = [];
		border.Title = "Frame";
		border.Position = Position.Origin;
		border.Dimensions = new Dimensions(30, 8);
		border.AddChild(panel);

		// The border was sized after its children were added, so re-arrange to give the labels a
		// non-empty content area to draw into.
		panel.ArrangeChildren();

		return border;
	}

	/// <summary>
	/// A second render pass must draw an element again even though nothing invalidated it. The
	/// screen is cleared on every pass, so anything skipped is anything erased.
	/// </summary>
	[TestMethod]
	public void RenderCleanElementTwiceDrawsItTwice()
	{
		// Arrange
		TextElement element = new("Label")
		{
			Position = Position.Origin,
			Dimensions = new Dimensions(20, 1)
		};
		RecordingConsoleProvider provider = new();

		// Act
		element.Render(provider);
		element.Render(provider);

		// Assert
		Assert.HasCount(2, provider.WritesOf("Label").ToList(), "A clean element must redraw, not be skipped");
	}

	/// <summary>
	/// The failure scenario from ktsu-dev/TUI#109: an input invalidates the interactive child
	/// only, and the static sibling must survive the next pass.
	/// </summary>
	[TestMethod]
	public void RenderAfterOnlyOneChildInvalidatesStillDrawsTheStaticSibling()
	{
		// Arrange
		BorderElement root = CreateLayout(out TextElement interactive);
		RecordingConsoleProvider provider = new();
		UIApplication application = new(provider);
		application.Setup(root);

		application.Render();
		int writesBeforeSecondPass = provider.Writes.Count;

		// Act - an input handled by the interactive child invalidates only that child
		interactive.Invalidate();
		application.Render();

		// Assert
		List<string> secondPass = [.. provider.Writes.Skip(writesBeforeSecondPass).Select(w => w.Text)];
		Assert.Contains("Label", secondPass, "The static label must be redrawn after the screen is cleared");
		Assert.Contains("Input", secondPass, "The invalidated child must be redrawn");
		Assert.Contains(" Frame ", secondPass, "The border title must be redrawn");
	}

	/// <summary>
	/// Every clear must be followed by a full redraw, so the number of full-tree redraws keeps
	/// pace with the number of clears no matter how many passes run.
	/// </summary>
	[TestMethod]
	public void RenderRedrawsTheWholeTreeOnEveryClear()
	{
		// Arrange
		BorderElement root = CreateLayout(out _);
		RecordingConsoleProvider provider = new();
		UIApplication application = new(provider);
		application.Setup(root);

		// Act
		application.Render();
		application.Render();
		application.Render();

		// Assert
		Assert.AreEqual(3, provider.ClearCount, "Every pass clears the screen");
		Assert.HasCount(3, provider.WritesOf("Label").ToList(), "Every clear must be followed by a full redraw");
	}

	/// <summary>
	/// The root element still takes its size from the console on the first pass.
	/// </summary>
	[TestMethod]
	public void RenderAssignsConsoleDimensionsToAnUnsizedRoot()
	{
		// Arrange
		TextElement root = new("Label");
		RecordingConsoleProvider provider = new() { Dimensions = new Dimensions(40, 12) };
		UIApplication application = new(provider);
		application.Setup(root);

		// Act
		application.Render();

		// Assert
		Assert.AreEqual(new Dimensions(40, 12), root.Dimensions);
	}
}
