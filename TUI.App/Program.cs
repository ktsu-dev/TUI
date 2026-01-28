// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.TUI.App;

/// <summary>
/// Entry point for the TUI Demo Application
/// </summary>
internal static class Program
{
	/// <summary>
	/// Main entry point
	/// </summary>
	/// <param name="args">Command line arguments</param>
	/// <returns>Exit code</returns>
	public static async Task<int> Main(string[] args)
	{
		try
		{
			// Check for command line arguments
			if (args.Length > 0 && args[0] == "--interactive")
			{
				await InteractiveDemo.RunAsync().ConfigureAwait(false);
			}
			else if (args.Length > 0 && args[0] == "--showcase")
			{
				Console.WriteLine("ShowcaseDemo is currently being fixed - using SampleApp instead");
				await SampleApp.RunAsync().ConfigureAwait(false);
			}
			else
			{
				await SampleApp.RunAsync().ConfigureAwait(false);
			}

			return 0;
		}
		catch (OperationCanceledException)
		{
			// User cancelled - normal exit
			return 0;
		}
		catch (InvalidOperationException ex)
		{
			await Console.Error.WriteLineAsync($"Error: {ex.Message}").ConfigureAwait(false);
			return 1;
		}
		catch (ArgumentException ex)
		{
			await Console.Error.WriteLineAsync($"Error: {ex.Message}").ConfigureAwait(false);
			return 1;
		}
	}
}
