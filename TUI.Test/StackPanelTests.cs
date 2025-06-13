// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Test;
using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

/// <summary>
/// Tests for StackPanel layout functionality
/// </summary>
[TestClass]
public class StackPanelTests
{
	/// <summary>
	/// Tests that StackPanel initializes with correct default values
	/// </summary>
	[TestMethod]
	public void StackPanel_DefaultConstructor_InitializesCorrectly()
	{
		// Arrange & Act
		StackPanel stackPanel = [];

		// Assert
		Assert.AreEqual(Orientation.Vertical, stackPanel.Orientation);
		Assert.AreEqual(0, stackPanel.Spacing);
		Assert.IsNotNull(stackPanel.Children);
		Assert.AreEqual(0, stackPanel.Children.Count);
		Assert.IsTrue(stackPanel.IsVisible);
	}

	/// <summary>
	/// Tests that StackPanel properly sets orientation
	/// </summary>
	[TestMethod]
	public void StackPanel_SetOrientation_UpdatesOrientationProperty()
	{
		// Arrange
		StackPanel stackPanel = [];
		Orientation expectedOrientation = Orientation.Horizontal;

		// Act
		stackPanel.Orientation = expectedOrientation;

		// Assert
		Assert.AreEqual(expectedOrientation, stackPanel.Orientation);
	}

	/// <summary>
	/// Tests that StackPanel invalidates when orientation changes
	/// </summary>
	[TestMethod]
	public void StackPanel_ChangeOrientation_TriggersInvalidation()
	{
		// Arrange
		StackPanel stackPanel = new()
		{ Orientation = Orientation.Vertical };
		bool invalidated = false;
		stackPanel.Invalidated += (sender, args) => invalidated = true;

		// Act
		stackPanel.Orientation = Orientation.Horizontal;

		// Assert
		Assert.IsTrue(invalidated);
	}

	/// <summary>
	/// Tests that StackPanel properly sets spacing
	/// </summary>
	[TestMethod]
	public void StackPanel_SetSpacing_UpdatesSpacingProperty()
	{
		// Arrange
		StackPanel stackPanel = [];
		int expectedSpacing = 5;

		// Act
		stackPanel.Spacing = expectedSpacing;

		// Assert
		Assert.AreEqual(expectedSpacing, stackPanel.Spacing);
	}

	/// <summary>
	/// Tests that StackPanel invalidates when spacing changes
	/// </summary>
	[TestMethod]
	public void StackPanel_ChangeSpacing_TriggersInvalidation()
	{
		// Arrange
		StackPanel stackPanel = new()
		{ Spacing = 0 };
		bool invalidated = false;
		stackPanel.Invalidated += (sender, args) => invalidated = true;

		// Act
		stackPanel.Spacing = 3;

		// Assert
		Assert.IsTrue(invalidated);
	}

	/// <summary>
	/// Tests that StackPanel can add child elements
	/// </summary>
	[TestMethod]
	public void StackPanel_AddChild_AddsChildToCollection()
	{
		// Arrange
		StackPanel stackPanel = [];
		TextElement child = new("Test child");

		// Act
		stackPanel.AddChild(child);

		// Assert
		Assert.AreEqual(1, stackPanel.Children.Count);
		Assert.AreEqual(child, stackPanel.Children.First());
		Assert.AreEqual(stackPanel, child.Parent);
	}

	/// <summary>
	/// Tests that StackPanel invalidates when child is added
	/// </summary>
	[TestMethod]
	public void StackPanel_AddChild_TriggersInvalidation()
	{
		// Arrange
		StackPanel stackPanel = [];
		bool invalidated = false;
		stackPanel.Invalidated += (sender, args) => invalidated = true;

		// Act
		stackPanel.AddChild(new TextElement("Test child"));

		// Assert
		Assert.IsTrue(invalidated);
	}

	/// <summary>
	/// Tests that StackPanel can remove child elements
	/// </summary>
	[TestMethod]
	public void StackPanel_RemoveChild_RemovesChildFromCollection()
	{
		// Arrange
		StackPanel stackPanel = [];
		TextElement child = new("Test child");
		stackPanel.AddChild(child);

		// Act
		bool result = stackPanel.RemoveChild(child);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(0, stackPanel.Children.Count);
		Assert.IsNull(child.Parent);
	}

	/// <summary>
	/// Tests that StackPanel invalidates when child is removed
	/// </summary>
	[TestMethod]
	public void StackPanel_RemoveChild_TriggersInvalidation()
	{
		// Arrange
		StackPanel stackPanel = [];
		TextElement child = new("Test child");
		stackPanel.AddChild(child);
		bool invalidated = false;
		stackPanel.Invalidated += (sender, args) => invalidated = true;

		// Act
		stackPanel.RemoveChild(child);

		// Assert
		Assert.IsTrue(invalidated);
	}

	/// <summary>
	/// Tests that StackPanel remove returns false for non-existent child
	/// </summary>
	[TestMethod]
	public void StackPanel_RemoveNonExistentChild_ReturnsFalse()
	{
		// Arrange
		StackPanel stackPanel = [];
		TextElement child = new("Test child");

		// Act
		bool result = stackPanel.RemoveChild(child);

		// Assert
		Assert.IsFalse(result);
	}

	/// <summary>
	/// Tests that StackPanel can clear all children
	/// </summary>
	[TestMethod]
	public void StackPanel_ClearChildren_RemovesAllChildren()
	{
		// Arrange
		StackPanel stackPanel = [];
		TextElement child1 = new("Child 1");
		TextElement child2 = new("Child 2");
		stackPanel.AddChild(child1);
		stackPanel.AddChild(child2);

		// Act
		stackPanel.ClearChildren();

		// Assert
		Assert.AreEqual(0, stackPanel.Children.Count);
		Assert.IsNull(child1.Parent);
		Assert.IsNull(child2.Parent);
	}

	/// <summary>
	/// Tests that StackPanel invalidates when children are cleared
	/// </summary>
	[TestMethod]
	public void StackPanel_ClearChildren_TriggersInvalidation()
	{
		// Arrange
		StackPanel stackPanel = [];
		stackPanel.AddChild(new TextElement("Test child"));
		bool invalidated = false;
		stackPanel.Invalidated += (sender, args) => invalidated = true;

		// Act
		stackPanel.ClearChildren();

		// Assert
		Assert.IsTrue(invalidated);
	}

	/// <summary>
	/// Tests that StackPanel handles null child gracefully
	/// </summary>
	[TestMethod]
	public void StackPanel_AddNullChild_ThrowsArgumentNullException()
	{
		// Arrange
		StackPanel stackPanel = [];

		// Act & Assert
		Assert.ThrowsException<ArgumentNullException>(() => stackPanel.AddChild(null!));
	}

	/// <summary>
	/// Tests that StackPanel handles input by checking children in reverse order
	/// </summary>
	[TestMethod]
	public void StackPanel_HandleInput_ChecksChildrenInReverseOrder()
	{
		// Arrange
		StackPanel stackPanel = [];
		Mock<IUIElement> mockChild1 = new();
		Mock<IUIElement> mockChild2 = new();

		mockChild1.Setup(c => c.HandleInput(It.IsAny<InputResult>())).Returns(false);
		mockChild1.Setup(c => c.IsVisible).Returns(true);
		mockChild2.Setup(c => c.HandleInput(It.IsAny<InputResult>())).Returns(true);
		mockChild2.Setup(c => c.IsVisible).Returns(true);

		stackPanel.AddChild(mockChild1.Object);
		stackPanel.AddChild(mockChild2.Object);

		InputResult input = new()
		{ Key = ConsoleKey.Enter, Modifiers = InputModifiers.None };

		// Act
		bool result = stackPanel.HandleInput(input);

		// Assert
		Assert.IsTrue(result);
		// mockChild2 (added last) should be called first and handle the input
		mockChild2.Verify(c => c.HandleInput(input), Times.Once);
		// mockChild1 should not be called because mockChild2 handled the input
		mockChild1.Verify(c => c.HandleInput(input), Times.Never);
	}

	/// <summary>
	/// Tests that StackPanel checks all children when none handle input
	/// </summary>
	[TestMethod]
	public void StackPanel_HandleInputNoChildrenHandleIt_ChecksAllChildren()
	{
		// Arrange
		StackPanel stackPanel = [];
		Mock<IUIElement> mockChild1 = new();
		Mock<IUIElement> mockChild2 = new();

		mockChild1.Setup(c => c.HandleInput(It.IsAny<InputResult>())).Returns(false);
		mockChild1.Setup(c => c.IsVisible).Returns(true);
		mockChild2.Setup(c => c.HandleInput(It.IsAny<InputResult>())).Returns(false);
		mockChild2.Setup(c => c.IsVisible).Returns(true);

		stackPanel.AddChild(mockChild1.Object);
		stackPanel.AddChild(mockChild2.Object);

		InputResult input = new()
		{ Key = ConsoleKey.Enter, Modifiers = InputModifiers.None };

		// Act
		bool result = stackPanel.HandleInput(input);

		// Assert
		Assert.IsFalse(result);
		// Both children should be called since neither handles the input
		mockChild1.Verify(c => c.HandleInput(input), Times.Once);
		mockChild2.Verify(c => c.HandleInput(input), Times.Once);
	}

	/// <summary>
	/// Tests that StackPanel with no children returns false for input
	/// </summary>
	[TestMethod]
	public void StackPanel_HandleInputWithNoChildren_ReturnsFalse()
	{
		// Arrange
		StackPanel stackPanel = [];
		InputResult input = new()
		{ Key = ConsoleKey.Enter, Modifiers = InputModifiers.None };

		// Act
		bool result = stackPanel.HandleInput(input);

		// Assert
		Assert.IsFalse(result);
	}

	/// <summary>
	/// Tests orientation enum values
	/// </summary>
	[TestMethod]
	[DataRow(Orientation.Vertical)]
	[DataRow(Orientation.Horizontal)]
	public void StackPanel_SetOrientationValues_AcceptsAllValidValues(Orientation orientation)
	{
		// Arrange
		StackPanel stackPanel = new()
		{
			// Act
			Orientation = orientation
		};

		// Assert
		Assert.AreEqual(orientation, stackPanel.Orientation);
	}
}
