// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;

namespace ktsu.TUI.Core.Elements;

/// <summary>
/// Base implementation for UI containers that can hold child elements
/// </summary>
public abstract class UIContainerBase : UIElementBase, IUIContainer
{
	private readonly List<IUIElement> _children = [];

	/// <inheritdoc />
	public IReadOnlyCollection<IUIElement> Children => _children.AsReadOnly();

	/// <inheritdoc />
	public virtual void AddChild(IUIElement child)
	{
		ArgumentNullException.ThrowIfNull(child);

		if (!_children.Contains(child))
		{
			child.Parent = this;
			_children.Add(child);
			child.Invalidated += OnChildInvalidated;
			ArrangeChildren();
			Invalidate();
		}
	}

	/// <inheritdoc />
	public virtual bool RemoveChild(IUIElement child)
	{
		ArgumentNullException.ThrowIfNull(child);

		if (_children.Remove(child))
		{
			child.Parent = null;
			child.Invalidated -= OnChildInvalidated;
			ArrangeChildren();
			Invalidate();
			return true;
		}

		return false;
	}

	/// <inheritdoc />
	public virtual void ClearChildren()
	{
		foreach (var child in _children)
		{
			child.Parent = null;
			child.Invalidated -= OnChildInvalidated;
		}

		_children.Clear();
		Invalidate();
	}

	/// <inheritdoc />
	public virtual void ArrangeChildren()
	{
		OnArrangeChildren();
	}

	/// <inheritdoc />
	public override void Render(IConsoleProvider provider)
	{
		// Render the container itself
		base.Render(provider);

		// Render all visible children
		foreach (var child in _children.Where(c => c.IsVisible))
		{
			child.Render(provider);
		}
	}

	/// <inheritdoc />
	public override bool HandleInput(InputResult input)
	{
		// Let children handle input first (reverse order for top-most first)
		foreach (var child in _children.Reverse<IUIElement>())
		{
			if (child.IsVisible && child.HandleInput(input))
			{
				return true;
			}
		}

		// If no child handled it, let the container handle it
		return base.HandleInput(input);
	}

	/// <inheritdoc />
	protected override Dimensions OnCalculateRequiredDimensions()
	{
		var requiredDimensions = OnCalculateRequiredDimensionsForChildren();
		return requiredDimensions.WithPadding(Padding);
	}

	/// <summary>
	/// When overridden in a derived class, arranges the child elements within the container
	/// </summary>
	protected abstract void OnArrangeChildren();

	/// <summary>
	/// When overridden in a derived class, calculates the required dimensions based on children
	/// </summary>
	/// <returns>The calculated dimensions for children (without container padding)</returns>
	protected virtual Dimensions OnCalculateRequiredDimensionsForChildren()
	{
		if (_children.Count == 0)
			return Dimensions.Empty;

		var maxWidth = 0;
		var maxHeight = 0;

		foreach (var child in _children)
		{
			var childDimensions = child.CalculateRequiredDimensions();
			var childRight = child.Position.X + childDimensions.Width;
			var childBottom = child.Position.Y + childDimensions.Height;

			maxWidth = Math.Max(maxWidth, childRight);
			maxHeight = Math.Max(maxHeight, childBottom);
		}

		return new Dimensions(maxWidth, maxHeight);
	}

	/// <summary>
	/// Gets children that are visible
	/// </summary>
	/// <returns>Enumerable of visible children</returns>
	protected IEnumerable<IUIElement> GetVisibleChildren()
	{
		return _children.Where(c => c.IsVisible);
	}

	private void OnChildInvalidated(object? sender, EventArgs e)
	{
		Invalidate();
	}
}
