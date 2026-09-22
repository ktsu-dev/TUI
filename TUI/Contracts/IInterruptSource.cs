// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Core.Contracts;

/// <summary>
/// Defines the contract for a source of process interrupt signals such as Ctrl+C and SIGTERM
/// </summary>
/// <remarks>
/// The real source is the process itself, which a test cannot signal without terminating the test
/// run. This seam lets the application's interrupt handling be exercised directly.
/// </remarks>
internal interface IInterruptSource
{
	/// <summary>
	/// Registers a callback to invoke when an interrupt signal arrives
	/// </summary>
	/// <param name="onInterrupt">The callback to invoke</param>
	/// <returns>A registration that unhooks the callback when disposed</returns>
	public IDisposable Register(Action onInterrupt);
}
