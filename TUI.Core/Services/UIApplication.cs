// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.TUI.Core.Contracts;
using ktsu.TUI.Core.Models;
using Microsoft.Extensions.Logging;

namespace ktsu.TUI.Core.Services;

/// <summary>
/// Main UI application service that orchestrates the TUI
/// </summary>
public class UIApplication : IUIApplication
{
	private readonly ILogger<UIApplication>? _logger;
	private bool _isRunning;
	private CancellationTokenSource? _cancellationTokenSource;

	/// <inheritdoc />
	public IUIElement? RootElement { get; set; }

	/// <inheritdoc />
	public IConsoleProvider ConsoleProvider { get; }

	/// <inheritdoc />
	public bool IsRunning => _isRunning;

	/// <summary>
	/// Initializes a new instance of the <see cref="UIApplication"/> class
	/// </summary>
	/// <param name="consoleProvider">The console provider to use</param>
	/// <param name="logger">Optional logger</param>
	public UIApplication(IConsoleProvider consoleProvider, ILogger<UIApplication>? logger = null)
	{
		ConsoleProvider = consoleProvider ?? throw new ArgumentNullException(nameof(consoleProvider));
		_logger = logger;
	}

	/// <inheritdoc />
	public async Task RunAsync(CancellationToken cancellationToken = default)
	{
		if (_isRunning)
		{
			_logger?.LogWarning("Application is already running");
			return;
		}

		_isRunning = true;
		_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

		try
		{
			_logger?.LogInformation("Starting UI application");

			// Initialize console
			ConsoleProvider.Clear();
			ConsoleProvider.SetCursorVisibility(false);

			// Initial render
			Render();

			// Start input processing
			await ProcessInputAsync(_cancellationTokenSource.Token);
		}
		catch (OperationCanceledException)
		{
			_logger?.LogInformation("UI application was cancelled");
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "An error occurred while running the UI application");
			throw;
		}
		finally
		{
			_isRunning = false;
			ConsoleProvider.SetCursorVisibility(true);
			_logger?.LogInformation("UI application stopped");
		}
	}

	/// <inheritdoc />
	public void Stop()
	{
		_logger?.LogInformation("Stopping UI application");
		_cancellationTokenSource?.Cancel();
	}

	/// <inheritdoc />
	public void Render()
	{
		if (RootElement == null)
		{
			_logger?.LogDebug("No root element to render");
			return;
		}

		try
		{
			_logger?.LogDebug("Rendering UI");

			// Clear the console
			ConsoleProvider.Clear();

			// Set root element dimensions to console dimensions if not set
			if (RootElement.Dimensions.IsEmpty)
			{
				RootElement.Dimensions = ConsoleProvider.Dimensions;
			}

			// Render the root element
			RootElement.Render(ConsoleProvider);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "An error occurred while rendering the UI");
		}
	}

	/// <inheritdoc />
	public async Task ProcessInputAsync(CancellationToken cancellationToken = default)
	{
		_logger?.LogDebug("Starting input processing");

		while (!cancellationToken.IsCancellationRequested && _isRunning)
		{
			try
			{
				var input = await ConsoleProvider.ReadInputAsync();

				_logger?.LogDebug("Received input: {InputType}", input.Type);

				// Handle global exit conditions
				if (input.IsExit)
				{
					_logger?.LogInformation("Exit input received");
					Stop();
					break;
				}

				// Let the root element handle the input
				var handled = RootElement?.HandleInput(input) ?? false;

				if (!handled)
				{
					_logger?.LogDebug("Input was not handled by any element");
				}

				// Re-render if needed (elements invalidate themselves when they change)
				Render();
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "An error occurred while processing input");

				// Continue processing unless it's a critical error
				if (ex is OutOfMemoryException or StackOverflowException)
				{
					throw;
				}
			}
		}

		_logger?.LogDebug("Input processing stopped");
	}

	/// <summary>
	/// Sets up the application with the specified root element
	/// </summary>
	/// <param name="rootElement">The root element to display</param>
	public void Setup(IUIElement rootElement)
	{
		ArgumentNullException.ThrowIfNull(rootElement);

		RootElement = rootElement;
		_logger?.LogInformation("UI application setup with root element of type {ElementType}", rootElement.GetType().Name);
	}

	/// <summary>
	/// Creates a builder for configuring the UI application
	/// </summary>
	/// <param name="consoleProvider">The console provider to use</param>
	/// <returns>A new application builder</returns>
	public static UIApplicationBuilder CreateBuilder(IConsoleProvider? consoleProvider = null)
	{
		return new UIApplicationBuilder(consoleProvider);
	}
}

/// <summary>
/// Builder for configuring UI applications
/// </summary>
public class UIApplicationBuilder
{
	private IConsoleProvider? _consoleProvider;
	private ILogger<UIApplication>? _logger;
	private IUIElement? _rootElement;

	/// <summary>
	/// Initializes a new instance of the <see cref="UIApplicationBuilder"/> class
	/// </summary>
	/// <param name="consoleProvider">Optional console provider</param>
	internal UIApplicationBuilder(IConsoleProvider? consoleProvider)
	{
		_consoleProvider = consoleProvider;
	}

	/// <summary>
	/// Sets the console provider
	/// </summary>
	/// <param name="consoleProvider">The console provider to use</param>
	/// <returns>This builder for method chaining</returns>
	public UIApplicationBuilder UseConsoleProvider(IConsoleProvider consoleProvider)
	{
		_consoleProvider = consoleProvider ?? throw new ArgumentNullException(nameof(consoleProvider));
		return this;
	}

	/// <summary>
	/// Sets the logger
	/// </summary>
	/// <param name="logger">The logger to use</param>
	/// <returns>This builder for method chaining</returns>
	public UIApplicationBuilder UseLogger(ILogger<UIApplication> logger)
	{
		_logger = logger;
		return this;
	}

	/// <summary>
	/// Sets the root element
	/// </summary>
	/// <param name="rootElement">The root element to display</param>
	/// <returns>This builder for method chaining</returns>
	public UIApplicationBuilder UseRootElement(IUIElement rootElement)
	{
		_rootElement = rootElement ?? throw new ArgumentNullException(nameof(rootElement));
		return this;
	}

	/// <summary>
	/// Builds the configured UI application
	/// </summary>
	/// <returns>The configured UI application</returns>
	public UIApplication Build()
	{
		var consoleProvider = _consoleProvider ?? new SpectreConsoleProvider();
		var app = new UIApplication(consoleProvider, _logger);

		if (_rootElement != null)
		{
			app.Setup(_rootElement);
		}

		return app;
	}
}
