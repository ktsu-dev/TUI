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
	/// Responds to an arriving signal, whichever mechanism delivered it
	/// </summary>
	/// <param name="cancelDefaultTermination">Cancels the runtime's default "terminate now" response</param>
	/// <param name="onInterrupt">Notifies the application that it should shut down</param>
	/// <remarks>
	/// Both delivery mechanisms funnel through here because neither
	/// <see cref="ConsoleCancelEventArgs"/> nor <see cref="PosixSignalContext"/> can be constructed
	/// by a test, so the response to a signal is only assertable once it is separated from the
	/// delivery of one.
	/// </remarks>
	internal static void OnSignal(Action cancelDefaultTermination, Action onInterrupt)
	{
		// Cancel first. Notifying can run arbitrary application code, and until the default
		// response is cancelled the runtime is still entitled to kill the process underneath it.
		cancelDefaultTermination();
		onInterrupt();
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
			_cancelKeyPress = (_, e) => OnSignal(() => e.Cancel = true, onInterrupt);

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
				return PosixSignalRegistration.Create(
					PosixSignal.SIGTERM,
					context => OnSignal(() => context.Cancel = true, onInterrupt));
			}
			catch (PlatformNotSupportedException)
			{
				// Nothing to do: Ctrl+C is still handled, which is the common case.
				return null;
			}
		}
	}
}
