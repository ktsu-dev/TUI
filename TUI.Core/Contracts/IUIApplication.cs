// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core.Contracts;

/// <summary>
/// Defines the contract for the main UI application
/// </summary>
public interface IUIApplication
{
	/// <summary>
	/// Gets or sets the root UI element
	/// </summary>
	public IUIElement? RootElement { get; set; }

	/// <summary>
	/// Gets the console provider
	/// </summary>
	public IConsoleProvider ConsoleProvider { get; }

	/// <summary>
	/// Gets whether the application is running
	/// </summary>
	public bool IsRunning { get; }

	/// <summary>
	/// Starts the application
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task representing the application run</returns>
	public Task RunAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Shuts down the application
	/// </summary>
	public void Shutdown();

	/// <summary>
	/// Renders the current UI state
	/// </summary>
	public void Render();

	/// <summary>
	/// Processes input events
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task representing the input processing</returns>
	public Task ProcessInputAsync(CancellationToken cancellationToken = default);
}
