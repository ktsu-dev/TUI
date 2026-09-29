// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Elements;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for the default measurement a custom container inherits from UIContainerBase
/// </summary>
[TestClass]
public sealed class UIContainerBaseMeasureTests
{
	/// <summary>
	/// A container that places every child at its content origin and keeps the default measurement
	/// </summary>
	private sealed class OriginContainer : UIContainerBase
	{
		protected override void OnRender(IConsoleProvider provider) { }

		protected override void OnArrangeChildren()
		{
			foreach (IUIElement child in GetVisibleChildren())
			{
				child.Position = GetContentPosition();
			}
		}
	}

	private static OriginContainer CreateContainer(Position position)
	{
		OriginContainer container = new()
		{
			Position = position,
			Padding = Padding.Uniform(1),
		};
		container.AddChild(new TextElement { Text = "abc" });
		container.ArrangeChildren();
		return container;
	}

	/// <summary>
	/// The required size is the content plus one padding, not the screen position plus two paddings
	/// </summary>
	[TestMethod]
	public void DefaultMeasurementIsContentPlusPadding()
	{
		OriginContainer container = CreateContainer(new Position(10, 5));

		Assert.AreEqual(new Dimensions(5, 3), container.CalculateRequiredDimensions());
	}

	/// <summary>
	/// Moving the container on screen does not change how large it asks to be
	/// </summary>
	[TestMethod]
	public void DefaultMeasurementDoesNotDependOnPosition()
	{
		Dimensions atOrigin = CreateContainer(Position.Origin).CalculateRequiredDimensions();
		Dimensions elsewhere = CreateContainer(new Position(40, 12)).CalculateRequiredDimensions();

		Assert.AreEqual(atOrigin, elsewhere);
	}
}
