// Copyright (c) 2023-2026 ktsu-dev contributors

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
public sealed class StackPanelTests
{
	/// <summary>
	/// Tests that StackPanel initializes with correct default values
	/// </summary>
	[TestMethod]
	public void StackPanelDefaultConstructorInitializesCorrectly()
	{
		// Arrange & Act
		StackPanel stackPanel = [];

		// Assert
		Assert.AreEqual(Orientation.Vertical, stackPanel.Orientation);
		Assert.AreEqual(0, stackPanel.Spacing);
		Assert.IsNotNull(stackPanel.Children);
		Assert.IsEmpty(stackPanel.Children);
		Assert.IsTrue(stackPanel.IsVisible, "Default IsVisible should be true");
	}

	/// <summary>
	/// Tests that StackPanel properly sets orientation
	/// </summary>
	[TestMethod]
	public void StackPanelSetOrientationUpdatesOrientationProperty()
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
	public void StackPanelChangeOrientationTriggersInvalidation()
	{
		// Arrange
		StackPanel stackPanel = new()
		{ Orientation = Orientation.Vertical };
		bool invalidated = false;
		stackPanel.Invalidated += (sender, args) => invalidated = true;

		// Act
		stackPanel.Orientation = Orientation.Horizontal;

		// Assert
		Assert.IsTrue(invalidated, "Changing orientation should trigger invalidation event");
	}

	/// <summary>
	/// Tests that StackPanel properly sets spacing
	/// </summary>
	[TestMethod]
	public void StackPanelSetSpacingUpdatesSpacingProperty()
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
	public void StackPanelChangeSpacingTriggersInvalidation()
	{
		// Arrange
		StackPanel stackPanel = new()
		{ Spacing = 0 };
		bool invalidated = false;
		stackPanel.Invalidated += (sender, args) => invalidated = true;

		// Act
		stackPanel.Spacing = 3;

		// Assert
		Assert.IsTrue(invalidated, "Changing spacing should trigger invalidation event");
	}

	/// <summary>
	/// Tests that StackPanel can add child elements
	/// </summary>
	[TestMethod]
	public void StackPanelAddChildAddsChildToCollection()
	{
		// Arrange
		StackPanel stackPanel = [];
		TextElement child = new("Test child");

		// Act
		stackPanel.AddChild(child);

		// Assert
		Assert.HasCount(1, stackPanel.Children);
		Assert.AreEqual(child, stackPanel.Children.First());
		Assert.AreSame(stackPanel, child.Parent);
	}

	/// <summary>
	/// Tests that StackPanel invalidates when child is added
	/// </summary>
	[TestMethod]
	public void StackPanelAddChildTriggersInvalidation()
	{
		// Arrange
		StackPanel stackPanel = [];
		bool invalidated = false;
		stackPanel.Invalidated += (sender, args) => invalidated = true;

		// Act
		stackPanel.AddChild(new TextElement("Test child"));

		// Assert
		Assert.IsTrue(invalidated, "Adding a child should trigger invalidation event");
	}

	/// <summary>
	/// Tests that StackPanel can remove child elements
	/// </summary>
	[TestMethod]
	public void StackPanelRemoveChildRemovesChildFromCollection()
	{
		// Arrange
		StackPanel stackPanel = [];
		TextElement child = new("Test child");
		stackPanel.AddChild(child);

		// Act
		bool result = stackPanel.RemoveChild(child);

		// Assert
		Assert.IsTrue(result, "RemoveChild should return true when child is successfully removed");
		Assert.IsEmpty(stackPanel.Children);
		Assert.IsNull(child.Parent);
	}

	/// <summary>
	/// Tests that StackPanel invalidates when child is removed
	/// </summary>
	[TestMethod]
	public void StackPanelRemoveChildTriggersInvalidation()
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
		Assert.IsTrue(invalidated, "Removing a child should trigger invalidation event");
	}

	/// <summary>
	/// Tests that StackPanel remove returns false for non-existent child
	/// </summary>
	[TestMethod]
	public void StackPanelRemoveNonExistentChildReturnsFalse()
	{
		// Arrange
		StackPanel stackPanel = [];
		TextElement child = new("Test child");

		// Act
		bool result = stackPanel.RemoveChild(child);

		// Assert
		Assert.IsFalse(result, "RemoveChild should return false when child is not in the collection");
	}

	/// <summary>
	/// Tests that StackPanel can clear all children
	/// </summary>
	[TestMethod]
	public void StackPanelClearChildrenRemovesAllChildren()
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
		Assert.IsEmpty(stackPanel.Children);
		Assert.IsNull(child1.Parent);
		Assert.IsNull(child2.Parent);
	}

	/// <summary>
	/// Tests that StackPanel invalidates when children are cleared
	/// </summary>
	[TestMethod]
	public void StackPanelClearChildrenTriggersInvalidation()
	{
		// Arrange
		StackPanel stackPanel = [];
		stackPanel.AddChild(new TextElement("Test child"));
		bool invalidated = false;
		stackPanel.Invalidated += (sender, args) => invalidated = true;

		// Act
		stackPanel.ClearChildren();

		// Assert
		Assert.IsTrue(invalidated, "Clearing children should trigger invalidation event");
	}

	/// <summary>
	/// Tests that StackPanel handles null child gracefully
	/// </summary>
	[TestMethod]
	public void StackPanelAddNullChildThrowsArgumentNullException()
	{
		// Arrange
		StackPanel stackPanel = [];

		// Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() => stackPanel.AddChild(null!));
	}

	/// <summary>
	/// Tests that StackPanel handles input by checking children in reverse order
	/// </summary>
	[TestMethod]
	public void StackPanelHandleInputChecksChildrenInReverseOrder()
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
		Assert.IsTrue(result, "HandleInput should return true when a child handles the input");
		// mockChild2 (added last) should be called first and handle the input
		mockChild2.Verify(c => c.HandleInput(input), Times.Once);
		// mockChild1 should not be called because mockChild2 handled the input
		mockChild1.Verify(c => c.HandleInput(input), Times.Never);
	}

	/// <summary>
	/// Tests that StackPanel checks all children when none handle input
	/// </summary>
	[TestMethod]
	public void StackPanelHandleInputNoChildrenHandleItChecksAllChildren()
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
		Assert.IsFalse(result, "HandleInput should return false when no child handles the input");
		// Both children should be called since neither handles the input
		mockChild1.Verify(c => c.HandleInput(input), Times.Once);
		mockChild2.Verify(c => c.HandleInput(input), Times.Once);
	}

	/// <summary>
	/// Tests that StackPanel with no children returns false for input
	/// </summary>
	[TestMethod]
	public void StackPanelHandleInputWithNoChildrenReturnsFalse()
	{
		// Arrange
		StackPanel stackPanel = [];
		InputResult input = new()
		{ Key = ConsoleKey.Enter, Modifiers = InputModifiers.None };

		// Act
		bool result = stackPanel.HandleInput(input);

		// Assert
		Assert.IsFalse(result, "HandleInput should return false when there are no children");
	}

	/// <summary>
	/// Tests orientation enum values
	/// </summary>
	[TestMethod]
	[DataRow(Orientation.Vertical)]
	[DataRow(Orientation.Horizontal)]
	public void StackPanelSetOrientationValuesAcceptsAllValidValues(Orientation orientation)
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

	/// <summary>
	/// Tests that a child pushed out of a vertical panel by a shrink is not drawn at the position
	/// an earlier, taller arrange gave it (ktsu-dev/TUI#140).
	/// </summary>
	[TestMethod]
	public void StackPanelShrinkStopsRenderingVerticalChildrenThatNoLongerFit()
	{
		TextElement a = new("a"), b = new("b"), c = new("c");
		StackPanel panel = new() { Dimensions = new Dimensions(10, 3) };
		panel.Add(a);
		panel.Add(b);
		panel.Add(c);
		panel.ArrangeChildren();

		panel.Dimensions = new Dimensions(10, 2);
		panel.ArrangeChildren();
		RecordingConsoleProvider provider = new();
		panel.Render(provider);

		Assert.ContainsSingle(provider.WritesOf("a"));
		Assert.ContainsSingle(provider.WritesOf("b"));
		Assert.IsEmpty(provider.WritesOf("c"), "c no longer fits in two rows, so it should not be drawn");
	}

	/// <summary>
	/// Tests that a child pushed out of a horizontal panel by a shrink is not drawn either.
	/// </summary>
	[TestMethod]
	public void StackPanelShrinkStopsRenderingHorizontalChildrenThatNoLongerFit()
	{
		TextElement a = new("aa"), b = new("bb"), c = new("cc");
		StackPanel panel = new(Orientation.Horizontal) { Dimensions = new Dimensions(6, 1) };
		panel.Add(a);
		panel.Add(b);
		panel.Add(c);
		panel.ArrangeChildren();

		panel.Dimensions = new Dimensions(4, 1);
		panel.ArrangeChildren();
		RecordingConsoleProvider provider = new();
		panel.Render(provider);

		Assert.ContainsSingle(provider.WritesOf("aa"));
		Assert.ContainsSingle(provider.WritesOf("bb"));
		Assert.IsEmpty(provider.WritesOf("cc"), "cc no longer fits in four columns, so it should not be drawn");
	}

	/// <summary>
	/// Tests that shrinking a panel to an empty content area stops every child being drawn.
	/// </summary>
	[TestMethod]
	public void StackPanelShrinkToEmptyStopsRenderingAllChildren()
	{
		StackPanel panel = new() { Dimensions = new Dimensions(10, 2) };
		panel.Add(new TextElement("a"));
		panel.Add(new TextElement("b"));
		panel.ArrangeChildren();

		panel.Dimensions = new Dimensions(10, 0);
		panel.ArrangeChildren();
		RecordingConsoleProvider provider = new();
		panel.Render(provider);

		Assert.IsEmpty(provider.Writes, "a panel with no content area should draw none of its children");
	}
	/// <summary>
	/// Tests that a word-wrapped TextElement in a vertical panel is given one row per wrapped line,
	/// not the single row its unwrapped text needs (ktsu-dev/TUI#131).
	/// </summary>
	[TestMethod]
	public void StackPanelVerticalGivesAWrappedTextElementARowPerWrappedLine()
	{
		// Children are added before the panel has a size, and it is then arranged once, as
		// UIApplication does for its root. Arranging a second time would hide the bug, because the
		// first pass leaves the text with a width it then wraps against.
		TextElement text = new("hello world foo") { WordWrap = true };
		StackPanel panel = [text];
		panel.Dimensions = new Dimensions(5, 10);
		panel.ArrangeChildren();

		RecordingConsoleProvider provider = new();
		panel.Render(provider);

		Assert.AreEqual(new Dimensions(5, 3), text.Dimensions);
		Assert.AreEqual(new Position(0, 0), Assert.ContainsSingle(provider.WritesOf("hello")).Position);
		Assert.AreEqual(new Position(0, 1), Assert.ContainsSingle(provider.WritesOf("world")).Position);
		Assert.AreEqual(new Position(0, 2), Assert.ContainsSingle(provider.WritesOf("foo")).Position);
	}

	/// <summary>
	/// Tests that the child after a wrapped TextElement starts below all of its wrapped lines.
	/// </summary>
	[TestMethod]
	public void StackPanelVerticalPlacesTheNextChildBelowAWrappedTextElement()
	{
		StackPanel panel = [new TextElement("hello world") { WordWrap = true }, new TextElement("next")];
		panel.Dimensions = new Dimensions(5, 10);
		panel.ArrangeChildren();

		RecordingConsoleProvider provider = new();
		panel.Render(provider);

		Assert.AreEqual(new Position(0, 2), Assert.ContainsSingle(provider.WritesOf("next")).Position);
	}

	/// <summary>
	/// Tests that a vertical panel inside a vertical panel passes its width down, so wrapped text in
	/// the inner panel is measured against the width it will actually get.
	/// </summary>
	[TestMethod]
	public void StackPanelNestedVerticalPanelsWrapTextToTheInnerContentWidth()
	{
		StackPanel inner = new() { Padding = new Padding(1, 0, 0, 0) };
		inner.Add(new TextElement("aaaa bbbb") { WordWrap = true });
		StackPanel outer = [inner, new TextElement("next")];
		outer.Dimensions = new Dimensions(5, 10);
		outer.ArrangeChildren();
		inner.ArrangeChildren();

		RecordingConsoleProvider provider = new();
		outer.Render(provider);

		Assert.AreEqual(new Dimensions(5, 2), inner.Dimensions);
		Assert.AreEqual(new Position(1, 0), Assert.ContainsSingle(provider.WritesOf("aaaa")).Position);
		Assert.AreEqual(new Position(1, 1), Assert.ContainsSingle(provider.WritesOf("bbbb")).Position);
		Assert.AreEqual(new Position(0, 2), Assert.ContainsSingle(provider.WritesOf("next")).Position);
	}
	/// <summary>
	/// Tests that measuring a vertical panel with no width, as a parent that does not know one does,
	/// leaves wrapped text unwrapped rather than guessing a width.
	/// </summary>
	[TestMethod]
	public void StackPanelRequiredDimensionsWithoutAWidthMeasureWrappedTextUnwrapped()
	{
		StackPanel panel = [new TextElement("hello world") { WordWrap = true }];

		Assert.AreEqual(new Dimensions(11, 1), panel.CalculateRequiredDimensions());
		Assert.AreEqual(new Dimensions(5, 2), panel.CalculateRequiredDimensions(5));
	}

	/// <summary>
	/// Tests that elements whose size does not depend on their width, including ones that implement
	/// IUIElement directly, are measured and placed the same as before the width-aware measure.
	/// </summary>
	[TestMethod]
	public void StackPanelVerticalMeasuresWidthIndependentElementsAtTheirOwnSize()
	{
		FixedSizeElement direct = new(new Dimensions(3, 2));
		TextElement next = new("next");
		StackPanel panel = [direct, next];
		panel.Dimensions = new Dimensions(10, 10);
		panel.ArrangeChildren();

		FixedSizeElementBase derived = new() { Dimensions = new Dimensions(4, 3) };

		Assert.AreEqual(direct.Size, ((IUIElement)direct).CalculateRequiredDimensions(1));
		Assert.AreEqual(new Dimensions(3, 2), direct.Dimensions);
		Assert.AreEqual(new Position(0, 2), next.Position);
		Assert.AreEqual(derived.CalculateRequiredDimensions(), derived.CalculateRequiredDimensions(1));
	}

	/// <summary>
	/// Tests that a container that does not use the available width, such as BorderElement, measures
	/// the same with or without one.
	/// </summary>
	[TestMethod]
	public void ContainerWithoutAWidthAwareLayoutMeasuresTheSameWithAWidth()
	{
		BorderElement border = [new TextElement("abc")];

		Assert.AreEqual(border.CalculateRequiredDimensions(), border.CalculateRequiredDimensions(2));
	}

	/// <summary>
	/// A UIElementBase whose size is whatever it was given, and which does not override the
	/// width-aware measure.
	/// </summary>
	private sealed class FixedSizeElementBase : ktsu.TUI.Core.Elements.UIElementBase
	{
		protected override void OnRender(IConsoleProvider provider)
		{
		}
	}

	/// <summary>
	/// An IUIElement implemented directly, relying on the interface's default width-aware measure.
	/// </summary>
	private sealed class FixedSizeElement(Dimensions size) : IUIElement
	{
		public Dimensions Size { get; } = size;

		public Position Position { get; set; }

		public Dimensions Dimensions { get; set; }

		public bool IsVisible { get; set; } = true;

		public IUIContainer? Parent { get; set; }

		public event EventHandler? Invalidated;

		public void Render(IConsoleProvider provider)
		{
		}

		public bool HandleInput(InputResult input) => false;

		public Dimensions CalculateRequiredDimensions() => Size;

		public void Invalidate() => Invalidated?.Invoke(this, EventArgs.Empty);
	}
}
