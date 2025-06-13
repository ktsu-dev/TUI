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
/// Tests for basic TUI elements functionality
/// </summary>
[TestClass]
sealed public class UIElementTests
{
	/// <summary>
	/// Tests that TextElement properly sets and gets text property
	/// </summary>
	[TestMethod]
	public void TextElementSetTextUpdatesTextProperty()
	{
		// Arrange
		TextElement textElement = new();
		string expectedText = "Hello World";

		// Act
		textElement.Text = expectedText;

		// Assert
		Assert.AreEqual(expectedText, textElement.Text);
	}

	/// <summary>
	/// Tests that TextElement invalidates when text changes
	/// </summary>
	[TestMethod]
	public void TextElementChangeTextTriggersInvalidation()
	{
		// Arrange
		TextElement textElement = new("Initial text");
		bool invalidated = false;
		textElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		textElement.Text = "New text";

		// Assert
		Assert.IsTrue(invalidated);
	}

	/// <summary>
	/// Tests that TextElement doesn't invalidate when setting same text
	/// </summary>
	[TestMethod]
	public void TextElementSetSameTextDoesNotInvalidate()
	{
		// Arrange
		string initialText = "Same text";
		TextElement textElement = new(initialText);
		bool invalidated = false;
		textElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		textElement.Text = initialText;

		// Assert
		Assert.IsFalse(invalidated);
	}

	/// <summary>
	/// Tests that TextElement handles null text gracefully
	/// </summary>
	[TestMethod]
	public void TextElementSetNullTextSetsEmptyString()
	{
		// Arrange
		TextElement textElement = new()
		{
			// Act
			Text = null!
		};

		// Assert
		Assert.AreEqual(string.Empty, textElement.Text);
	}

	/// <summary>
	/// Tests that TextElement initializes with correct default values
	/// </summary>
	[TestMethod]
	public void TextElementDefaultConstructorInitializesCorrectly()
	{
		// Arrange & Act
		TextElement textElement = new();

		// Assert
		Assert.AreEqual(string.Empty, textElement.Text);
		Assert.AreEqual(HorizontalAlignment.Left, textElement.HorizontalAlignment);
		Assert.AreEqual(VerticalAlignment.Top, textElement.VerticalAlignment);
		Assert.IsFalse(textElement.WordWrap);
		Assert.IsTrue(textElement.IsVisible);
	}

	/// <summary>
	/// Tests that TextElement constructor with text parameter works correctly
	/// </summary>
	[TestMethod]
	public void TextElementConstructorWithTextSetsTextCorrectly()
	{
		// Arrange
		string expectedText = "Constructor text";

		// Act
		TextElement textElement = new(expectedText);

		// Assert
		Assert.AreEqual(expectedText, textElement.Text);
	}

	/// <summary>
	/// Tests that TextElement constructor with text and style works correctly
	/// </summary>
	[TestMethod]
	public void TextElementConstructorWithTextAndStyleSetsPropertiesCorrectly()
	{
		// Arrange
		string expectedText = "Styled text";
		TextStyle expectedStyle = new()
		{ IsBold = true, Foreground = "red" };

		// Act
		TextElement textElement = new(expectedText, expectedStyle);

		// Assert
		Assert.AreEqual(expectedText, textElement.Text);
		Assert.AreEqual(expectedStyle.IsBold, textElement.Style.IsBold);
		Assert.AreEqual(expectedStyle.Foreground, textElement.Style.Foreground);
	}

	/// <summary>
	/// Tests that Position property changes trigger invalidation
	/// </summary>
	[TestMethod]
	public void UIElementChangePositionTriggersInvalidation()
	{
		// Arrange
		TextElement textElement = new();
		bool invalidated = false;
		textElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		textElement.Position = new Position(10, 20);

		// Assert
		Assert.IsTrue(invalidated);
		Assert.AreEqual(10, textElement.Position.X);
		Assert.AreEqual(20, textElement.Position.Y);
	}

	/// <summary>
	/// Tests that Dimensions property changes trigger invalidation
	/// </summary>
	[TestMethod]
	public void UIElementChangeDimensionsTriggersInvalidation()
	{
		// Arrange
		TextElement textElement = new();
		bool invalidated = false;
		textElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		textElement.Dimensions = new Dimensions(100, 50);

		// Assert
		Assert.IsTrue(invalidated);
		Assert.AreEqual(100, textElement.Dimensions.Width);
		Assert.AreEqual(50, textElement.Dimensions.Height);
	}

	/// <summary>
	/// Tests that Visibility property changes trigger invalidation
	/// </summary>
	[TestMethod]
	public void UIElementChangeVisibilityTriggersInvalidation()
	{
		// Arrange
		TextElement textElement = new();
		bool invalidated = false;
		textElement.Invalidated += (sender, args) => invalidated = true;

		// Act
		textElement.IsVisible = false;

		// Assert
		Assert.IsTrue(invalidated);
		Assert.IsFalse(textElement.IsVisible);
	}

	/// <summary>
	/// Tests that Render doesn't execute when element is not visible
	/// </summary>
	[TestMethod]
	public void UIElementRenderWhenNotVisibleDoesNotCallOnRender()
	{
		// Arrange
		Mock<IConsoleProvider> mockProvider = new();
		TextElement textElement = new("Test text")
		{
			IsVisible = false
		};

		// Act
		textElement.Render(mockProvider.Object);

		// Assert
		mockProvider.VerifyNoOtherCalls();
	}

	/// <summary>
	/// Tests that HandleInput returns false by default
	/// </summary>
	[TestMethod]
	public void UIElementHandleInputReturnsFalseByDefault()
	{
		// Arrange
		TextElement textElement = new();
		InputResult input = new()
		{ Key = ConsoleKey.Enter, Modifiers = InputModifiers.None };

		// Act
		bool result = textElement.HandleInput(input);

		// Assert
		Assert.IsFalse(result);
	}

	/// <summary>
	/// Tests that Padding property is properly set and retrieved
	/// </summary>
	[TestMethod]
	public void UIElementSetPaddingUpdatesPaddingProperty()
	{
		// Arrange
		TextElement textElement = new();
		Padding expectedPadding = new(1, 2, 3, 4);

		// Act
		textElement.Padding = expectedPadding;

		// Assert
		Assert.AreEqual(expectedPadding, textElement.Padding);
	}
}
