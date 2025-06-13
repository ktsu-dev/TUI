// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Test;
using System.Drawing;
using ktsu.TUI.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for TUI Core Models
/// </summary>
[TestClass]
public class ModelsTests
{
	/// <summary>
	/// Tests Position constructor and properties
	/// </summary>
	[TestMethod]
	public void Position_Constructor_SetsProperties()
	{
		// Arrange & Act
		Position position = new(10, 20);

		// Assert
		Assert.AreEqual(10, position.X);
		Assert.AreEqual(20, position.Y);
	}

	/// <summary>
	/// Tests Position equality
	/// </summary>
	[TestMethod]
	public void Position_Equality_WorksCorrectly()
	{
		// Arrange
		Position position1 = new(10, 20);
		Position position2 = new(10, 20);
		Position position3 = new(15, 25);

		// Act & Assert
		Assert.AreEqual(position1, position2);
		Assert.AreNotEqual(position1, position3);
		Assert.IsTrue(position1 == position2);
		Assert.IsTrue(position1 != position3);
	}

	/// <summary>
	/// Tests Position Offset method
	/// </summary>
	[TestMethod]
	public void Position_Offset_ReturnsNewPosition()
	{
		// Arrange
		Position position = new(10, 20);

		// Act
		Position offsetPosition = position.Offset(5, 3);

		// Assert
		Assert.AreEqual(15, offsetPosition.X);
		Assert.AreEqual(23, offsetPosition.Y);
		// Original position should be unchanged
		Assert.AreEqual(10, position.X);
		Assert.AreEqual(20, position.Y);
	}

	/// <summary>
	/// Tests Dimensions constructor and properties
	/// </summary>
	[TestMethod]
	public void Dimensions_Constructor_SetsProperties()
	{
		// Arrange & Act
		Dimensions dimensions = new(100, 50);

		// Assert
		Assert.AreEqual(100, dimensions.Width);
		Assert.AreEqual(50, dimensions.Height);
	}

	/// <summary>
	/// Tests Dimensions Empty property
	/// </summary>
	[TestMethod]
	public void Dimensions_Empty_ReturnsZeroDimensions()
	{
		// Act
		Dimensions empty = Dimensions.Empty;

		// Assert
		Assert.AreEqual(0, empty.Width);
		Assert.AreEqual(0, empty.Height);
	}

	/// <summary>
	/// Tests Dimensions IsEmpty property
	/// </summary>
	[TestMethod]
	public void Dimensions_IsEmpty_DetectsEmptyCorrectly()
	{
		// Arrange
		Dimensions empty = new(0, 0);
		Dimensions notEmpty = new(10, 5);

		// Assert
		Assert.IsTrue(empty.IsEmpty);
		Assert.IsFalse(notEmpty.IsEmpty);
	}

	/// <summary>
	/// Tests TextStyle default constructor
	/// </summary>
	[TestMethod]
	public void TextStyle_DefaultConstructor_SetsDefaults()
	{
		// Act
		TextStyle style = new();

		// Assert
		Assert.IsFalse(style.IsBold);
		Assert.IsFalse(style.IsItalic);
		Assert.IsFalse(style.IsUnderline);
		Assert.IsFalse(style.IsStrikethrough);
		Assert.IsNull(style.Foreground);
		Assert.IsNull(style.Background);
	}

	/// <summary>
	/// Tests TextStyle with all properties set
	/// </summary>
	[TestMethod]
	public void TextStyle_AllProperties_SetCorrectly()
	{
		// Arrange & Act
		TextStyle style = new()
		{
			IsBold = true,
			IsItalic = true,
			IsUnderline = true,
			IsStrikethrough = true,
			Foreground = "red",
			Background = "blue"
		};

		// Assert
		Assert.IsTrue(style.IsBold);
		Assert.IsTrue(style.IsItalic);
		Assert.IsTrue(style.IsUnderline);
		Assert.IsTrue(style.IsStrikethrough);
		// Color.FromName("red") returns Color.Red, which has Name = "Red"
		Assert.AreEqual("Red", style.Foreground);
		Assert.AreEqual("Blue", style.Background);
	}

	/// <summary>
	/// Tests TextStyle with color values
	/// </summary>
	[TestMethod]
	public void TextStyle_ColorProperties_WorkWithColorValues()
	{
		// Arrange
		Color foregroundColor = Color.Red;
		Color backgroundColor = Color.Blue;

		// Act
		TextStyle style = new()
		{
			ForegroundColor = foregroundColor,
			BackgroundColor = backgroundColor
		};

		// Assert
		Assert.AreEqual(foregroundColor, style.ForegroundColor);
		Assert.AreEqual(backgroundColor, style.BackgroundColor);
	}

	/// <summary>
	/// Tests Padding uniform constructor
	/// </summary>
	[TestMethod]
	public void Padding_UniformConstructor_SetsAllSides()
	{
		// Act
		Padding padding = Padding.Uniform(5);

		// Assert
		Assert.AreEqual(5, padding.Left);
		Assert.AreEqual(5, padding.Top);
		Assert.AreEqual(5, padding.Right);
		Assert.AreEqual(5, padding.Bottom);
	}

	/// <summary>
	/// Tests Padding individual sides constructor
	/// </summary>
	[TestMethod]
	public void Padding_IndividualSidesConstructor_SetsSides()
	{
		// Act
		Padding padding = new(1, 2, 3, 4);

		// Assert
		Assert.AreEqual(1, padding.Left);
		Assert.AreEqual(2, padding.Top);
		Assert.AreEqual(3, padding.Right);
		Assert.AreEqual(4, padding.Bottom);
	}

	/// <summary>
	/// Tests InputResult creation methods
	/// </summary>
	[TestMethod]
	public void InputResult_CreationMethods_WorkCorrectly()
	{
		// Act
		InputResult exitInput = InputResult.Exit();
		InputResult keyInput = InputResult.FromKey(ConsoleKey.Enter, InputModifiers.None);

		// Assert
		Assert.IsTrue(exitInput.IsExit);
		Assert.AreEqual(InputType.Keyboard, keyInput.Type);
		Assert.AreEqual(ConsoleKey.Enter, keyInput.Key);
		Assert.AreEqual(InputModifiers.None, keyInput.Modifiers);
	}

	/// <summary>
	/// Tests InputModifiers enum values
	/// </summary>
	[TestMethod]
	public void InputModifiers_EnumValues_AreValid()
	{
		// Test each enum value
		ConsoleModifiers[] modifiers = [InputModifiers.None, InputModifiers.Shift, InputModifiers.Control, InputModifiers.Alt];

		foreach (ConsoleModifiers modifier in modifiers)
		{
			// Act
			InputResult input = InputResult.FromKey(ConsoleKey.A, modifier);

			// Assert
			Assert.AreEqual(modifier, input.Modifiers);
		}
	}

	/// <summary>
	/// Tests InputType enum values
	/// </summary>
	[TestMethod]
	public void InputType_EnumValues_AreValid()
	{
		// Test each enum value
		InputType[] inputTypes = [InputType.None, InputType.Keyboard, InputType.Mouse];

		foreach (InputType inputType in inputTypes)
		{
			// Arrange
			InputResult input = new()
			{ Type = inputType };

			// Act & Assert
			Assert.AreEqual(inputType, input.Type);
		}
	}
}
