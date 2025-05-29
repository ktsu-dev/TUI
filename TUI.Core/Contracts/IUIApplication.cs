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
	IUIElement? RootElement { get; set; }

	/// <summary>
	/// Gets the console provider
	/// </summary>
	IConsoleProvider ConsoleProvider { get; }

	/// <summary>
	/// Gets whether the application is running
	/// </summary>
	bool IsRunning { get; }

	/// <summary>
	/// Starts the application
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task representing the application run</returns>
	Task RunAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Stops the application
	/// </summary>
	void Stop();

	/// <summary>
	/// Renders the current UI state
	/// </summary>
	void Render();

	/// <summary>
	/// Processes input events
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task representing the input processing</returns>
	Task ProcessInputAsync(CancellationToken cancellationToken = default);
}
