// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core.Services;
using ktsu.TUI.Core.Contracts;
using Microsoft.Extensions.Logging;

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
	internal UIApplicationBuilder(IConsoleProvider? consoleProvider) => _consoleProvider = consoleProvider;

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
		IConsoleProvider consoleProvider = _consoleProvider ?? new SpectreConsoleProvider();
		UIApplication app = new UIApplication(consoleProvider, _logger);

		if (_rootElement != null)
		{
			app.Setup(_rootElement);
		}

		return app;
	}
}
