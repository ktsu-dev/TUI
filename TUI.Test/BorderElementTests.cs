// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

/// <summary>
/// Tests for <see cref="BorderElement"/>
/// </summary>
[TestClass]
public sealed class BorderElementTests
{
	/// <summary>
	/// Tests that BorderElement initializes with correct default values
	/// </summary>
	[TestMethod]
	public void BorderElementDefaultConstructorInitializesCorrectly()
	{
		// Arrange & Act
		BorderElement borderElement = [];

		// Assert
		Assert.AreEqual(BorderStyle.SingleLine, borderElement.BorderStyle);
		Assert.AreEqual(string.Empty, borderElement.Title);
		Assert.AreEqual(HorizontalAlignment.Left, borderElement.TitleAlignment);
		Assert.IsNull(borderElement.Child);
		Assert.IsTrue(borderElement.IsVisible, "Default IsVisible should be true");
	}

	/// <summary>
	/// Tests that BorderElement properly sets and gets title property
	/// </summary>
	[TestMethod]
	public void BorderElementSetTitleUpdatesTitleProperty()
	{
		// Arrange
		BorderElement borderElement = [];
		string expectedTitle = "Test Title";

		// Act
		borderElement.Title = expectedTitle;

		// Assert
		Assert.AreEqual(expectedTitle, borderElement.Title);
	}

	/// <summary>
	/// Tests that BorderElement invalidates when title changes
	/// </summary>
	[TestMethod]
	public void BorderElementChangeTitleTriggersInvalidation()
	{
		// Arrange
		BorderElement borderElement = new()
		{ Title = "Initial Title" };
		bool invalidated = false;
		borderElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		borderElement.Title = "New Title";

		// Assert
		Assert.IsTrue(invalidated, "Changing title should trigger invalidation event");
	}

	/// <summary>
	/// Tests that BorderElement doesn't invalidate when setting same title
	/// </summary>
	[TestMethod]
	public void BorderElementSetSameTitleDoesNotInvalidate()
	{
		// Arrange
		string initialTitle = "Same Title";
		BorderElement borderElement = new()
		{ Title = initialTitle };
		bool invalidated = false;
		borderElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		borderElement.Title = initialTitle;

		// Assert
		Assert.IsFalse(invalidated, "Setting the same title value should not trigger invalidation");
	}

	/// <summary>
	/// Tests that BorderElement properly sets border style
	/// </summary>
	[TestMethod]
	public void BorderElementSetBorderStyleUpdatesBorderStyleProperty()
	{
		// Arrange
		BorderElement borderElement = [];
		BorderStyle expectedStyle = BorderStyle.DoubleLine;

		// Act
		borderElement.BorderStyle = expectedStyle;

		// Assert
		Assert.AreEqual(expectedStyle, borderElement.BorderStyle);
	}

	/// <summary>
	/// Tests that BorderElement invalidates when border style changes
	/// </summary>
	[TestMethod]
	public void BorderElementChangeBorderStyleTriggersInvalidation()
	{
		// Arrange
		BorderElement borderElement = new()
		{ BorderStyle = BorderStyle.SingleLine };
		bool invalidated = false;
		borderElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		borderElement.BorderStyle = BorderStyle.DoubleLine;

		// Assert
		Assert.IsTrue(invalidated, "Changing border style should trigger invalidation event");
	}

	/// <summary>
	/// Tests that BorderElement properly sets title alignment
	/// </summary>
	[TestMethod]
	public void BorderElementSetTitleAlignmentUpdatesTitleAlignmentProperty()
	{
		// Arrange
		BorderElement borderElement = [];
		HorizontalAlignment expectedAlignment = HorizontalAlignment.Center;

		// Act
		borderElement.TitleAlignment = expectedAlignment;

		// Assert
		Assert.AreEqual(expectedAlignment, borderElement.TitleAlignment);
	}

	/// <summary>
	/// Tests that BorderElement invalidates when title alignment changes
	/// </summary>
	[TestMethod]
	public void BorderElementChangeTitleAlignmentTriggersInvalidation()
	{
		// Arrange
		BorderElement borderElement = new()
		{ TitleAlignment = HorizontalAlignment.Left };
		bool invalidated = false;
		borderElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		borderElement.TitleAlignment = HorizontalAlignment.Right;

		// Assert
		Assert.IsTrue(invalidated, "Changing title alignment should trigger invalidation event");
	}

	/// <summary>
	/// Tests that BorderElement properly sets child element
	/// </summary>
	[TestMethod]
	public void BorderElementSetChildUpdatesChildProperty()
	{
		// Arrange
		BorderElement borderElement = [];
		TextElement childElement = new("Child content");

		// Act
		borderElement.Child = childElement;

		// Assert
		Assert.AreEqual(childElement, borderElement.Child);
		Assert.AreEqual(borderElement, childElement.Parent);
	}

	/// <summary>
	/// Tests that BorderElement invalidates when child changes
	/// </summary>
	[TestMethod]
	public void BorderElementChangeChildTriggersInvalidation()
	{
		// Arrange
		BorderElement borderElement = new()
		{ Child = new TextElement("Initial child") };
		bool invalidated = false;
		borderElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		borderElement.Child = new TextElement("New child");

		// Assert
		Assert.IsTrue(invalidated, "Changing child should trigger invalidation event");
	}

	/// <summary>
	/// Tests that BorderElement handles null child gracefully
	/// </summary>
	[TestMethod]
	public void BorderElementSetNullChildHandlesGracefully()
	{
		// Arrange
		BorderElement borderElement = [];
		TextElement initialChild = new("Initial child");
		borderElement.Child = initialChild;

		// Act
		borderElement.Child = null;

		// Assert
		Assert.IsNull(borderElement.Child);
		Assert.IsNull(initialChild.Parent);
	}

	/// <summary>
	/// Tests all border style enum values
	/// </summary>
	[TestMethod]
	[DataRow(BorderStyle.SingleLine)]
	[DataRow(BorderStyle.DoubleLine)]
	[DataRow(BorderStyle.Rounded)]
	[DataRow(BorderStyle.Thick)]
	[DataRow(BorderStyle.Ascii)]
	public void BorderElementSetBorderStyleValuesAcceptsAllValidValues(BorderStyle borderStyle)
	{
		// Arrange
		BorderElement borderElement = new()
		{
			// Act
			BorderStyle = borderStyle
		};

		// Assert
		Assert.AreEqual(borderStyle, borderElement.BorderStyle);
	}

	/// <summary>
	/// Tests that BorderElement constructor with child sets properties correctly
	/// </summary>
	[TestMethod]
	public void BorderElementConstructorWithChildSetsPropertiesCorrectly()
	{
		// Arrange
		TextElement childElement = new("Constructor child");

		// Act
		BorderElement borderElement = new()
		{ Child = childElement };

		// Assert
		Assert.AreEqual(childElement, borderElement.Child);
		Assert.AreEqual(borderElement, childElement.Parent);
	}

	/// <summary>
	/// Tests that BorderElement with title renders correctly when not visible
	/// </summary>
	[TestMethod]
	public void BorderElementRenderWhenNotVisibleDoesNotCallOnRender()
	{
		// Arrange
		Mock<IConsoleProvider> mockProvider = new();
		BorderElement borderElement = new()
		{
			Title = "Test Border",
			Child = new TextElement("Test content"),
			IsVisible = false
		};

		// Act
		borderElement.Render(mockProvider.Object);

		// Assert
		mockProvider.VerifyNoOtherCalls();
	}

	/// <summary>
	/// Tests that BorderElement handles input by delegating to child
	/// </summary>
	[TestMethod]
	public void BorderElementHandleInputDelegatesToChild()
	{
		// Arrange
		Mock<IUIElement> mockChild = new();
		mockChild.Setup(c => c.HandleInput(It.IsAny<InputResult>())).Returns(true);
		mockChild.Setup(c => c.IsVisible).Returns(true);
		BorderElement borderElement = new()
		{ Child = mockChild.Object };
		InputResult input = new()
		{ Key = ConsoleKey.Enter, Modifiers = InputModifiers.None };

		// Act
		bool result = borderElement.HandleInput(input);

		// Assert
		Assert.IsTrue(result, "HandleInput should return true when child handles the input");
		mockChild.Verify(c => c.HandleInput(input), Times.Once);
	}

	/// <summary>
	/// Tests that BorderElement with no child returns false for input
	/// </summary>
	[TestMethod]
	public void BorderElementHandleInputWithNoChildReturnsFalse()
	{
		// Arrange
		BorderElement borderElement = [];
		InputResult input = new()
		{ Key = ConsoleKey.Enter, Modifiers = InputModifiers.None };

		// Act
		bool result = borderElement.HandleInput(input);

		// Assert
		Assert.IsFalse(result, "HandleInput should return false when there is no child");
	}
}
