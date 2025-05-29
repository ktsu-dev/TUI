// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core;

/// <summary>
/// A sample class
/// </summary>
public static class Sample
{
	/// <summary>
	/// Sample method that returns the provided value
	/// </summary>
	/// <typeparam name="T">The type of value</typeparam>
	/// <param name="value">The value to return</param>
	/// <returns>The provided value</returns>
	public static T Echo<T>(T value) => value;
}

