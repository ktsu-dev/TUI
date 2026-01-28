// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core.Elements;

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;

/// <summary>
/// Base implementation for UI elements
/// </summary>
public abstract class UIElementBase : IUIElement
{
	/// <inheritdoc />
	public Position Position
	{
		get;
		set
		{
			if (field != value)
			{
				field = value;
				Invalidate();
			}
		}
	}

	/// <inheritdoc />
	public Dimensions Dimensions
	{
		get;
		set
		{
			if (field != value)
			{
				field = value;
				Invalidate();
			}
		}
	}

	/// <inheritdoc />
	public bool IsVisible
	{
		get;
		set
		{
			if (field != value)
			{
				field = value;
				Invalidate();
			}
		}
	} = true;

	/// <inheritdoc />
	public IUIContainer? Parent { get; set; }

	/// <summary>
	/// Gets whether the element needs to be re-rendered
	/// </summary>
	protected bool IsDirty { get; private set; } = true;

	/// <summary>
	/// Gets or sets the padding for this element
	/// </summary>
	public Padding Padding { get; set; } = Padding.None;

	/// <summary>
	/// Event raised when the element is invalidated
	/// </summary>
	public event EventHandler? Invalidated;

	/// <inheritdoc />
	public virtual void Render(IConsoleProvider provider)
	{
		if (!IsVisible)
		{
			return;
		}

		if (IsDirty)
		{
			OnRender(provider);
			IsDirty = false;
		}
	}

	/// <inheritdoc />
	public virtual bool HandleInput(InputResult input) => OnHandleInput(input);

	/// <inheritdoc />
	public virtual Dimensions CalculateRequiredDimensions() => OnCalculateRequiredDimensions();

	/// <inheritdoc />
	public void Invalidate()
	{
		IsDirty = true;
		Parent?.Invalidate();
		Invalidated?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// When overridden in a derived class, renders the element
	/// </summary>
	/// <param name="provider">The console provider to use for rendering</param>
	protected abstract void OnRender(IConsoleProvider provider);

	/// <summary>
	/// When overridden in a derived class, handles input for the element
	/// </summary>
	/// <param name="input">The input to handle</param>
	/// <returns>True if the input was handled, false otherwise</returns>
	protected virtual bool OnHandleInput(InputResult input) => false;

	/// <summary>
	/// When overridden in a derived class, calculates the required dimensions for the element
	/// </summary>
	/// <returns>The calculated dimensions</returns>
	protected virtual Dimensions OnCalculateRequiredDimensions() => Dimensions;

	/// <summary>
	/// Gets the content area (dimensions minus padding)
	/// </summary>
	/// <returns>The content area dimensions</returns>
	protected Dimensions GetContentArea() => Dimensions.WithoutPadding(Padding);

	/// <summary>
	/// Gets the content position (position plus padding offset)
	/// </summary>
	/// <returns>The content position</returns>
	protected Position GetContentPosition() => Position.Offset(Padding.Left, Padding.Top);
}
