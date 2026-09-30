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

	/// <summary>
	/// A change deep in the tree reaches the root once, not once per route at every level
	/// (ktsu-dev/TUI#148)
	/// </summary>
	[TestMethod]
	public void AChangeNestedManyLevelsDeepInvalidatesTheRootOnce()
	{
		StackPanel root = [];
		StackPanel current = root;
		for (int i = 0; i < 15; i++)
		{
			StackPanel next = [];
			current.AddChild(next);
			current = next;
		}

		TextElement leaf = new() { Text = "x" };
		current.AddChild(leaf);

		int count = 0;
		root.Invalidated += (_, _) => count++;

		leaf.Text = "y";

		Assert.AreEqual(1, count);
	}

	/// <summary>
	/// Each ancestor between the change and the root is invalidated exactly once
	/// </summary>
	[TestMethod]
	public void AChangeInvalidatesEachAncestorOnce()
	{
		StackPanel root = [];
		StackPanel middle = [];
		TextElement leaf = new() { Text = "x" };
		root.AddChild(middle);
		middle.AddChild(leaf);

		int rootCount = 0;
		int middleCount = 0;
		root.Invalidated += (_, _) => rootCount++;
		middle.Invalidated += (_, _) => middleCount++;

		leaf.Text = "y";

		Assert.AreEqual(1, middleCount);
		Assert.AreEqual(1, rootCount);
	}
}
