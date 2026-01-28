// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core.Elements;

using System.Collections;
using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;

/// <summary>
/// Base implementation for UI containers that can hold child elements
/// </summary>
public abstract class UIContainerBase : UIElementBase, IUIContainer, IEnumerable<IUIElement>
{
	private readonly List<IUIElement> _children = [];

	/// <inheritdoc />
	public IReadOnlyCollection<IUIElement> Children => _children.AsReadOnly();

	/// <inheritdoc />
	public virtual void AddChild(IUIElement child)
	{
		_ = child ?? throw new ArgumentNullException(nameof(child));

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
		_ = child ?? throw new ArgumentNullException(nameof(child));

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
		foreach (IUIElement child in _children)
		{
			child.Parent = null;
			child.Invalidated -= OnChildInvalidated;
		}

		_children.Clear();
		Invalidate();
	}

	/// <inheritdoc />
	public virtual void ArrangeChildren() => OnArrangeChildren();

	/// <inheritdoc />
	public override void Render(IConsoleProvider provider)
	{
		// Render the container itself
		base.Render(provider);

		// Render all visible children
		foreach (IUIElement? child in _children.Where(c => c.IsVisible))
		{
			child.Render(provider);
		}
	}

	/// <inheritdoc />
	public override bool HandleInput(InputResult input)
	{
		// Let children handle input first (reverse order for top-most first)
		foreach (IUIElement child in _children.Reverse<IUIElement>())
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
		Dimensions requiredDimensions = OnCalculateRequiredDimensionsForChildren();
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
		{
			return Dimensions.Empty;
		}

		int maxWidth = 0;
		int maxHeight = 0;

		foreach (IUIElement child in _children)
		{
			Dimensions childDimensions = child.CalculateRequiredDimensions();
			int childRight = child.Position.X + childDimensions.Width;
			int childBottom = child.Position.Y + childDimensions.Height;

			maxWidth = Math.Max(maxWidth, childRight);
			maxHeight = Math.Max(maxHeight, childBottom);
		}

		return new Dimensions(maxWidth, maxHeight);
	}

	/// <summary>
	/// Gets children that are visible
	/// </summary>
	/// <returns>Enumerable of visible children</returns>
	protected IEnumerable<IUIElement> GetVisibleChildren() => _children.Where(c => c.IsVisible);

	/// <inheritdoc />
	public IEnumerator<IUIElement> GetEnumerator() => _children.GetEnumerator();

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Adds a child element to the container (for collection initializer syntax)
	/// </summary>
	/// <param name="child">The child element to add</param>
	public void Add(IUIElement child) => AddChild(child);

	private void OnChildInvalidated(object? sender, EventArgs e) => Invalidate();
}
