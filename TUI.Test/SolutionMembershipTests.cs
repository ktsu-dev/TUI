// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests that every project in the repository is built on every PR.
/// </summary>
/// <remarks>
/// <c>dotnet build</c> and <c>dotnet test</c> build only what <c>TUI.sln</c> lists, but the release
/// pipeline publishes any project it finds. A project left out of the solution therefore compiles for
/// the first time during a release, which is how <c>TUI.Cli</c>'s analyzer errors broke one
/// (ktsu-dev/TUI#241).
/// </remarks>
[TestClass]
public sealed class SolutionMembershipTests
{
	/// <summary>
	/// Tests that every project file under the repository root is listed in <c>TUI.sln</c>.
	/// </summary>
	[TestMethod]
	public void EveryProjectIsInTheSolution()
	{
		DirectoryInfo root = FindRepositoryRoot();
		string solution = File.ReadAllText(Path.Combine(root.FullName, "TUI.sln"));

		string[] missing = [.. Directory
			.EnumerateFiles(root.FullName, "*.csproj", SearchOption.AllDirectories)
			.Select(path => Path.GetRelativePath(root.FullName, path))
			.Where(relative => !relative.Split(Path.DirectorySeparatorChar).Any(IsBuildOutput))
			.Where(relative => !solution.Contains($"\"{relative.Replace('/', '\\')}\"", StringComparison.OrdinalIgnoreCase))
			.Order(StringComparer.Ordinal)];

		Assert.IsEmpty(missing, $"projects missing from TUI.sln, so no PR build compiles them: {string.Join(", ", missing)}");
	}

	private static bool IsBuildOutput(string segment) =>
		segment is "bin" or "obj" || segment.StartsWith('.');

	private static DirectoryInfo FindRepositoryRoot()
	{
		for (DirectoryInfo? dir = new(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
		{
			if (File.Exists(Path.Combine(dir.FullName, "TUI.sln")))
			{
				return dir;
			}
		}

		throw new FileNotFoundException("TUI.sln was not found above the test output directory.");
	}
}
