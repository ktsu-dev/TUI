// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.TUI.Core.Services;

using ktsu.TUI.Core.Contracts;
using Microsoft.Extensions.Logging;

/// <summary>
/// Main UI application service that orchestrates the TUI
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UIApplication"/> class
/// </remarks>
/// <param name="consoleProvider">The console provider to use</param>
/// <param name="logger">Optional logger</param>
public class UIApplication(IConsoleProvider consoleProvider, ILogger<UIApplication>? logger = null) : IUIApplication
{
	private readonly ILogger<UIApplication>? _logger = logger;
	private CancellationTokenSource? _cancellationTokenSource;

	// Logger message delegates for improved performance
	private static readonly Action<ILogger, Exception?> LogApplicationAlreadyRunning =
		LoggerMessage.Define(LogLevel.Warning, new EventId(1, nameof(LogApplicationAlreadyRunning)), "Application is already running");

	private static readonly Action<ILogger, Exception?> LogStartingApplication =
		LoggerMessage.Define(LogLevel.Information, new EventId(2, nameof(LogStartingApplication)), "Starting UI application");

	private static readonly Action<ILogger, Exception?> LogApplicationCancelled =
		LoggerMessage.Define(LogLevel.Information, new EventId(3, nameof(LogApplicationCancelled)), "UI application was cancelled");

	private static readonly Action<ILogger, Exception?> LogApplicationError =
		LoggerMessage.Define(LogLevel.Error, new EventId(4, nameof(LogApplicationError)), "An error occurred while running the UI application");

	private static readonly Action<ILogger, Exception?> LogApplicationStopped =
		LoggerMessage.Define(LogLevel.Information, new EventId(5, nameof(LogApplicationStopped)), "UI application stopped");

	private static readonly Action<ILogger, Exception?> LogStoppingApplication =
		LoggerMessage.Define(LogLevel.Information, new EventId(6, nameof(LogStoppingApplication)), "Stopping UI application");

	private static readonly Action<ILogger, Exception?> LogNoRootElement =
		LoggerMessage.Define(LogLevel.Debug, new EventId(7, nameof(LogNoRootElement)), "No root element to render");

	private static readonly Action<ILogger, Exception?> LogRenderingUI =
		LoggerMessage.Define(LogLevel.Debug, new EventId(8, nameof(LogRenderingUI)), "Rendering UI");

	private static readonly Action<ILogger, Exception?> LogRenderError =
		LoggerMessage.Define(LogLevel.Error, new EventId(9, nameof(LogRenderError)), "An error occurred while rendering the UI");

	private static readonly Action<ILogger, Exception?> LogStartingInputProcessing =
		LoggerMessage.Define(LogLevel.Debug, new EventId(10, nameof(LogStartingInputProcessing)), "Starting input processing");

	private static readonly Action<ILogger, string, Exception?> LogReceivedInput =
		LoggerMessage.Define<string>(LogLevel.Debug, new EventId(11, nameof(LogReceivedInput)), "Received input: {InputType}");

	private static readonly Action<ILogger, Exception?> LogExitInputReceived =
		LoggerMessage.Define(LogLevel.Information, new EventId(12, nameof(LogExitInputReceived)), "Exit input received");

	private static readonly Action<ILogger, Exception?> LogInputNotHandled =
		LoggerMessage.Define(LogLevel.Debug, new EventId(13, nameof(LogInputNotHandled)), "Input was not handled by any element");

	private static readonly Action<ILogger, Exception?> LogInputProcessingError =
		LoggerMessage.Define(LogLevel.Error, new EventId(14, nameof(LogInputProcessingError)), "An error occurred while processing input");

	private static readonly Action<ILogger, Exception?> LogInputProcessingStopped =
		LoggerMessage.Define(LogLevel.Debug, new EventId(15, nameof(LogInputProcessingStopped)), "Input processing stopped");

	private static readonly Action<ILogger, string, Exception?> LogUIApplicationSetup =
		LoggerMessage.Define<string>(LogLevel.Information, new EventId(16, nameof(LogUIApplicationSetup)), "UI application setup with root element of type {ElementType}");

	/// <inheritdoc />
	public IUIElement? RootElement { get; set; }

	/// <inheritdoc />
	public IConsoleProvider ConsoleProvider { get; } = Ensure.NotNull(consoleProvider);

	/// <inheritdoc />
	public bool IsRunning { get; private set; }

	/// <inheritdoc />
	public async Task RunAsync(CancellationToken cancellationToken = default)
	{
		if (IsRunning)
		{
			if (_logger != null)
			{
				LogApplicationAlreadyRunning(_logger, null);
			}
			return;
		}

		IsRunning = true;
		_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

		try
		{
			if (_logger != null)
			{
				LogStartingApplication(_logger, null);
			}

			// Initialize console
			ConsoleProvider.Clear();
			ConsoleProvider.SetCursorVisibility(false);

			// Initial render
			Render();

			// Start input processing
			await ProcessInputAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
			if (_logger != null)
			{
				LogApplicationCancelled(_logger, null);
			}
		}
		catch (Exception ex)
		{
			if (_logger != null)
			{
				LogApplicationError(_logger, ex);
			}
			throw;
		}
		finally
		{
			IsRunning = false;
			ConsoleProvider.SetCursorVisibility(true);
			if (_logger != null)
			{
				LogApplicationStopped(_logger, null);
			}
		}
	}

	/// <inheritdoc />
	public void Shutdown()
	{
		if (_logger != null)
		{
			LogStoppingApplication(_logger, null);
		}
		_cancellationTokenSource?.Cancel();
	}

	/// <inheritdoc />
	public void Render()
	{
		if (RootElement == null)
		{
			if (_logger != null)
			{
				LogNoRootElement(_logger, null);
			}
			return;
		}

		try
		{
			if (_logger != null)
			{
				LogRenderingUI(_logger, null);
			}

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
		catch (InvalidOperationException ex)
		{
			if (_logger != null)
			{
				LogRenderError(_logger, ex);
			}
		}
		catch (ArgumentException ex)
		{
			if (_logger != null)
			{
				LogRenderError(_logger, ex);
			}
		}
		catch (NotSupportedException ex)
		{
			if (_logger != null)
			{
				LogRenderError(_logger, ex);
			}
		}
	}

	/// <inheritdoc />
	public async Task ProcessInputAsync(CancellationToken cancellationToken = default)
	{
		if (_logger != null)
		{
			LogStartingInputProcessing(_logger, null);
		}

		while (!cancellationToken.IsCancellationRequested && IsRunning)
		{
			try
			{
				Models.InputResult input = await ConsoleProvider.ReadInputAsync().ConfigureAwait(false);

				if (_logger != null)
				{
					LogReceivedInput(_logger, input.Type.ToString(), null);
				}

				// Handle global exit conditions
				if (input.IsExit)
				{
					if (_logger != null)
					{
						LogExitInputReceived(_logger, null);
					}
					Shutdown();
					break;
				}

				// Let the root element handle the input
				bool handled = RootElement?.HandleInput(input) ?? false;

				if (!handled)
				{
					if (_logger != null)
					{
						LogInputNotHandled(_logger, null);
					}
				}

				// Re-render if needed (elements invalidate themselves when they change)
				Render();
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (InvalidOperationException ex)
			{
				if (_logger != null)
				{
					LogInputProcessingError(_logger, ex);
				}

				// Continue processing for recoverable errors
			}
			catch (ArgumentException ex)
			{
				if (_logger != null)
				{
					LogInputProcessingError(_logger, ex);
				}

				// Continue processing for recoverable errors
			}
			catch (OutOfMemoryException)
			{
				// Critical error - rethrow
				throw;
			}
			catch (StackOverflowException)
			{
				// Critical error - rethrow
				throw;
			}
		}

		if (_logger != null)
		{
			LogInputProcessingStopped(_logger, null);
		}
	}

	/// <summary>
	/// Sets up the application with the specified root element
	/// </summary>
	/// <param name="rootElement">The root element to display</param>
	public void Setup(IUIElement rootElement)
	{
		Ensure.NotNull(rootElement);

		RootElement = rootElement;
		if (_logger != null)
		{
			LogUIApplicationSetup(_logger, rootElement.GetType().Name, null);
		}
	}

	/// <summary>
	/// Creates a builder for configuring the UI application
	/// </summary>
	/// <param name="consoleProvider">The console provider to use</param>
	/// <returns>A new application builder</returns>
	public static UIApplicationBuilder CreateBuilder(IConsoleProvider? consoleProvider = null) => new(consoleProvider);
}
