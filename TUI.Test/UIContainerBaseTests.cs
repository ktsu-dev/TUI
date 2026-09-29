// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for the child management that every container inherits from UIContainerBase
/// </summary>
[TestClass]
public sealed class UIContainerBaseTests
{
	/// <summary>
	/// Adding an element to a second container moves it there rather than leaving it in both
	/// </summary>
	[TestMethod]
	public void AddChildMovesAChildOutOfItsPreviousContainer()
	{
		StackPanel first = [];
		StackPanel second = [];
		TextElement child = new() { Text = "abc" };

		first.AddChild(child);
		second.AddChild(child);

		Assert.IsEmpty(first.Children);
		Assert.HasCount(1, second.Children);
		Assert.AreSame(second, child.Parent);
	}

	/// <summary>
	/// Removing a moved element from its old container is a no-op and keeps its new parent
	/// </summary>
	[TestMethod]
	public void RemoveChildFromThePreviousContainerLeavesTheParentAlone()
	{
		StackPanel first = [];
		StackPanel second = [];
		TextElement child = new() { Text = "abc" };

		first.AddChild(child);
		second.AddChild(child);

		Assert.IsFalse(first.RemoveChild(child));
		Assert.AreSame(second, child.Parent);
		Assert.HasCount(1, second.Children);
	}

	/// <summary>
	/// The previous container stops listening to a moved element, so it is not invalidated by it
	/// </summary>
	[TestMethod]
	public void AMovedChildNoLongerInvalidatesItsPreviousContainer()
	{
		StackPanel first = [];
		StackPanel second = [];
		TextElement child = new() { Text = "abc" };

		first.AddChild(child);
		second.AddChild(child);

		int firstInvalidations = 0;
		first.Invalidated += (_, _) => firstInvalidations++;
		child.Text = "changed";

		Assert.AreEqual(0, firstInvalidations);
	}
}
