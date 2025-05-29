// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using Moq;

namespace ktsu.TUI.Test;

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
		var borderElement = new BorderElement();

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
		var borderElement = new BorderElement();
		var expectedTitle = "Test Title";

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
		var borderElement = new BorderElement { Title = "Initial Title" };
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
		var initialTitle = "Same Title";
		var borderElement = new BorderElement { Title = initialTitle };
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
		var borderElement = new BorderElement();
		var expectedStyle = BorderStyle.Double;

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
		var borderElement = new BorderElement { BorderStyle = BorderStyle.Single };
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
		var borderElement = new BorderElement();
		var expectedAlignment = HorizontalAlignment.Center;

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
		var borderElement = new BorderElement { TitleAlignment = HorizontalAlignment.Left };
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
		var borderElement = new BorderElement();
		var childElement = new TextElement("Child content");

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
		var borderElement = new BorderElement { Child = new TextElement("Initial child") };
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
		var borderElement = new BorderElement();
		var initialChild = new TextElement("Initial child");
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
	[DataRow(BorderStyle.None)]
	[DataRow(BorderStyle.Single)]
	[DataRow(BorderStyle.Double)]
	[DataRow(BorderStyle.Rounded)]
	[DataRow(BorderStyle.Thick)]
	[DataRow(BorderStyle.Ascii)]
	public void BorderElement_SetBorderStyleValues_AcceptsAllValidValues(BorderStyle borderStyle)
	{
		// Arrange
		var borderElement = new BorderElement();

		// Act
		borderElement.BorderStyle = borderStyle;

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
		var childElement = new TextElement("Constructor child");

		// Act
		var borderElement = new BorderElement { Child = childElement };

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
		var mockProvider = new Mock<IConsoleProvider>();
		var borderElement = new BorderElement
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
		var mockChild = new Mock<IUIElement>();
		mockChild.Setup(c => c.HandleInput(It.IsAny<InputResult>())).Returns(true);
		var borderElement = new BorderElement { Child = mockChild.Object };
		var input = new InputResult { Key = "Enter", Modifiers = InputModifiers.None };

		// Act
		var result = borderElement.HandleInput(input);

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
		var borderElement = new BorderElement();
		var input = new InputResult { Key = "Enter", Modifiers = InputModifiers.None };

		// Act
		var result = borderElement.HandleInput(input);

		// Assert
		Assert.IsFalse(result);
	}
}
