// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Test;

[TestClass]
public class SampleTests
{
	[TestMethod]
	public void Echo_ReturnsProvidedValue()
	{
		// Arrange
		var expectedValue = 42;

		// Act
		var result = Sample.Echo(expectedValue);

		// Assert
		Assert.AreEqual(expectedValue, result);
	}

	[TestMethod]
	public void Echo_WithString_ReturnsProvidedString()
	{
		// Arrange
		var expectedValue = "Hello World";

		// Act
		var result = Sample.Echo(expectedValue);

		// Assert
		Assert.AreEqual(expectedValue, result);
	}
}
