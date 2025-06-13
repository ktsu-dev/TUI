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
/// Tests for BorderElement functionality
/// </summary>
[TestClass]
public class BorderElementTests
{
	/// <summary>
	/// Tests that BorderElement initializes with correct default values
	/// </summary>
	[TestMethod]
	public void BorderElement_DefaultConstructor_InitializesCorrectly()
	{
		// Arrange & Act
		BorderElement borderElement = new BorderElement();

		// Assert
		Assert.AreEqual(BorderStyle.Single, borderElement.BorderStyle);
		Assert.AreEqual(string.Empty, borderElement.Title);
		Assert.AreEqual(HorizontalAlignment.Left, borderElement.TitleAlignment);
		Assert.IsNull(borderElement.Child);
		Assert.IsTrue(borderElement.IsVisible);
	}

	/// <summary>
	/// Tests that BorderElement properly sets and gets title property
	/// </summary>
	[TestMethod]
	public void BorderElement_SetTitle_UpdatesTitleProperty()
	{
		// Arrange
		BorderElement borderElement = new BorderElement();
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
	public void BorderElement_ChangeTitle_TriggersInvalidation()
	{
		// Arrange
		BorderElement borderElement = new BorderElement { Title = "Initial Title" };
		bool invalidated = false;
		borderElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		borderElement.Title = "New Title";

		// Assert
		Assert.IsTrue(invalidated);
	}

	/// <summary>
	/// Tests that BorderElement doesn't invalidate when setting same title
	/// </summary>
	[TestMethod]
	public void BorderElement_SetSameTitle_DoesNotInvalidate()
	{
		// Arrange
		string initialTitle = "Same Title";
		BorderElement borderElement = new BorderElement { Title = initialTitle };
		bool invalidated = false;
		borderElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		borderElement.Title = initialTitle;

		// Assert
		Assert.IsFalse(invalidated);
	}

	/// <summary>
	/// Tests that BorderElement properly sets border style
	/// </summary>
	[TestMethod]
	public void BorderElement_SetBorderStyle_UpdatesBorderStyleProperty()
	{
		// Arrange
		BorderElement borderElement = new BorderElement();
		BorderStyle expectedStyle = BorderStyle.Double;

		// Act
		borderElement.BorderStyle = expectedStyle;

		// Assert
		Assert.AreEqual(expectedStyle, borderElement.BorderStyle);
	}

	/// <summary>
	/// Tests that BorderElement invalidates when border style changes
	/// </summary>
	[TestMethod]
	public void BorderElement_ChangeBorderStyle_TriggersInvalidation()
	{
		// Arrange
		BorderElement borderElement = new BorderElement { BorderStyle = BorderStyle.Single };
		bool invalidated = false;
		borderElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		borderElement.BorderStyle = BorderStyle.Double;

		// Assert
		Assert.IsTrue(invalidated);
	}

	/// <summary>
	/// Tests that BorderElement properly sets title alignment
	/// </summary>
	[TestMethod]
	public void BorderElement_SetTitleAlignment_UpdatesTitleAlignmentProperty()
	{
		// Arrange
		BorderElement borderElement = new BorderElement();
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
	public void BorderElement_ChangeTitleAlignment_TriggersInvalidation()
	{
		// Arrange
		BorderElement borderElement = new BorderElement { TitleAlignment = HorizontalAlignment.Left };
		bool invalidated = false;
		borderElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		borderElement.TitleAlignment = HorizontalAlignment.Right;

		// Assert
		Assert.IsTrue(invalidated);
	}

	/// <summary>
	/// Tests that BorderElement properly sets child element
	/// </summary>
	[TestMethod]
	public void BorderElement_SetChild_UpdatesChildProperty()
	{
		// Arrange
		BorderElement borderElement = new BorderElement();
		TextElement childElement = new TextElement("Child content");

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
	public void BorderElement_ChangeChild_TriggersInvalidation()
	{
		// Arrange
		BorderElement borderElement = new BorderElement { Child = new TextElement("Initial child") };
		bool invalidated = false;
		borderElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		borderElement.Child = new TextElement("New child");

		// Assert
		Assert.IsTrue(invalidated);
	}

	/// <summary>
	/// Tests that BorderElement handles null child gracefully
	/// </summary>
	[TestMethod]
	public void BorderElement_SetNullChild_HandlesGracefully()
	{
		// Arrange
		BorderElement borderElement = new BorderElement();
		TextElement initialChild = new TextElement("Initial child");
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
	[DataRow(BorderStyle.Single)]
	[DataRow(BorderStyle.Double)]
	[DataRow(BorderStyle.Rounded)]
	[DataRow(BorderStyle.Thick)]
	[DataRow(BorderStyle.Ascii)]
	public void BorderElement_SetBorderStyleValues_AcceptsAllValidValues(BorderStyle borderStyle)
	{
		// Arrange
		BorderElement borderElement = new BorderElement
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
	public void BorderElement_ConstructorWithChild_SetsPropertiesCorrectly()
	{
		// Arrange
		TextElement childElement = new TextElement("Constructor child");

		// Act
		BorderElement borderElement = new BorderElement { Child = childElement };

		// Assert
		Assert.AreEqual(childElement, borderElement.Child);
		Assert.AreEqual(borderElement, childElement.Parent);
	}

	/// <summary>
	/// Tests that BorderElement with title renders correctly when not visible
	/// </summary>
	[TestMethod]
	public void BorderElement_RenderWhenNotVisible_DoesNotCallOnRender()
	{
		// Arrange
		Mock<IConsoleProvider> mockProvider = new Mock<IConsoleProvider>();
		BorderElement borderElement = new BorderElement
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
	public void BorderElement_HandleInput_DelegatesToChild()
	{
		// Arrange
		Mock<IUIElement> mockChild = new Mock<IUIElement>();
		mockChild.Setup(c => c.HandleInput(It.IsAny<InputResult>())).Returns(true);
		mockChild.Setup(c => c.IsVisible).Returns(true);
		BorderElement borderElement = new BorderElement { Child = mockChild.Object };
		InputResult input = new InputResult { Key = ConsoleKey.Enter, Modifiers = InputModifiers.None };

		// Act
		bool result = borderElement.HandleInput(input);

		// Assert
		Assert.IsTrue(result);
		mockChild.Verify(c => c.HandleInput(input), Times.Once);
	}

	/// <summary>
	/// Tests that BorderElement with no child returns false for input
	/// </summary>
	[TestMethod]
	public void BorderElement_HandleInputWithNoChild_ReturnsFalse()
	{
		// Arrange
		BorderElement borderElement = new BorderElement();
		InputResult input = new InputResult { Key = ConsoleKey.Enter, Modifiers = InputModifiers.None };

		// Act
		bool result = borderElement.HandleInput(input);

		// Assert
		Assert.IsFalse(result);
	}
}
