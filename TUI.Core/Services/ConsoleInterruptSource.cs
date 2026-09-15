// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Core.Services;

using System.Runtime.InteropServices;

using ktsu.TUI.Core.Contracts;

/// <summary>
/// An <see cref="IInterruptSource"/> backed by the process's real interrupt signals
/// </summary>
/// <remarks>
/// Ctrl+C is taken through <see cref="Console.CancelKeyPress"/> rather than by setting
/// <see cref="Console.TreatControlCAsInput"/>, because the latter only delivers the key while the
/// application happens to be inside a blocking read, whereas the event is observed wherever the
/// application is. SIGTERM is taken for the same reason: left alone, either signal ends the
/// process before the application can put back the terminal state it changed.
/// </remarks>
internal sealed class ConsoleInterruptSource : IInterruptSource
{
	/// <inheritdoc />
	public IDisposable Register(Action onInterrupt)
	{
		Ensure.NotNull(onInterrupt);

		return new Registration(onInterrupt);
	}

	/// <summary>
	/// Holds the signal hooks for one <see cref="Register"/> call and unhooks them on disposal
	/// </summary>
	private sealed class Registration : IDisposable
	{
		private readonly ConsoleCancelEventHandler _cancelKeyPress;
		private readonly PosixSignalRegistration? _sigTerm;
		private bool _disposed;

		internal Registration(Action onInterrupt)
		{
			_cancelKeyPress = (_, e) =>
			{
				// Cancel the runtime's default "terminate now" behaviour so the application
				// shuts down through its normal path and gets to restore the terminal.
				e.Cancel = true;
				onInterrupt();
			};

			Console.CancelKeyPress += _cancelKeyPress;
			_sigTerm = TryRegisterSigTerm(onInterrupt);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;
			Console.CancelKeyPress -= _cancelKeyPress;
			_sigTerm?.Dispose();
		}

		private static PosixSignalRegistration? TryRegisterSigTerm(Action onInterrupt)
		{
			try
			{
				return PosixSignalRegistration.Create(PosixSignal.SIGTERM, context =>
				{
					context.Cancel = true;
					onInterrupt();
				});
			}
			catch (PlatformNotSupportedException)
			{
				// Nothing to do: Ctrl+C is still handled, which is the common case.
				return null;
			}
		}
	}
}
