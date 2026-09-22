// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using System.Reflection;
using ktsu.TUI.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests that the library still publishes under the bare family name.
/// </summary>
/// <remarks>
/// <c>ktsu.Sdk</c> derives the package ID and the assembly name from the project file name, so
/// the library project has to stay <c>TUI.csproj</c> for the package to stay <c>ktsu.TUI</c>.
/// Restoring the old <c>TUI.Core</c> name — or adding any other suffix — would silently change
/// the published package ID, which is a break for every consumer. Nothing else in the build
/// fails when that drifts, so it is pinned here (ktsu-dev/TUI#99).
/// </remarks>
[TestClass]
public sealed class PackageIdentityTests
{
	/// <summary>
	/// Tests that the library assembly is named for the repository family, with no suffix.
	/// </summary>
	[TestMethod]
	public void LibraryAssemblyIsNamedForTheFamilyAlone()
	{
		Assembly library = typeof(Position).Assembly;

		Assert.AreEqual(
			"ktsu.TUI",
			library.GetName().Name,
			"the library publishes under its assembly name, so a suffix here is a package-ID break");
	}
}
