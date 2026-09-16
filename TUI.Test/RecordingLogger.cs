// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using ktsu.TUI.Core.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// An <see cref="ILogger{TCategoryName}"/> that records the messages written to it, so a test can
/// assert what the application reported rather than only that it did not throw.
/// </summary>
internal sealed class RecordingLogger : ILogger<UIApplication>
{
	private readonly List<string> messages = [];

	/// <summary>
	/// Gets the messages logged so far, in call order.
	/// </summary>
	internal IEnumerable<string> Messages
	{
		get
		{
			lock (messages)
			{
				return [.. messages];
			}
		}
	}

	/// <inheritdoc />
	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

	/// <inheritdoc />
	public bool IsEnabled(LogLevel logLevel) => true;

	/// <inheritdoc />
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		string message = formatter is null ? string.Empty : formatter(state, exception);
		lock (messages)
		{
			messages.Add(message);
		}
	}
}
