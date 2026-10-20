// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.TUI.Test;

using System.Collections.Immutable;
using System.Text.RegularExpressions;
using ktsu.TUI.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests that the README's code sample compiles against the library.
/// </summary>
/// <remarks>
/// The Quick Start used <c>BorderStyle.Double</c>, which never existed (the member is
/// <c>DoubleLine</c>), so the first thing a new user pasted failed with CS0117. Nothing in the build
/// reads the README, so it is compiled here, verbatim, against the current API (ktsu-dev/TUI#226).
/// </remarks>
[TestClass]
public sealed partial class ReadmeSampleTests
{
	/// <summary>
	/// Tests that every C# block in the README compiles as a program referencing the library.
	/// </summary>
	[TestMethod]
	public void ReadmeCSharpSamplesCompile()
	{
		string[] samples = [.. CSharpBlock().Matches(File.ReadAllText(FindReadme())).Select(m => m.Groups["code"].Value)];
		Assert.IsNotEmpty(samples, "the README has no C# sample to compile");

		ImmutableArray<MetadataReference> references = LibraryReferences();
		for (int i = 0; i < samples.Length; i++)
		{
			string[] errors = CompileErrors($"ReadmeSample{i}", samples[i], references);
			Assert.IsEmpty(errors, $"README C# sample {i + 1} does not compile:\n{string.Join('\n', errors)}");
		}
	}

	private static ImmutableArray<MetadataReference> LibraryReferences() =>
		[.. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(Path.PathSeparator)
			.Append(typeof(Position).Assembly.Location)
			.Append(typeof(Spectre.Console.AnsiConsole).Assembly.Location)
			.Distinct(StringComparer.Ordinal)
			.Select(path => MetadataReference.CreateFromFile(path))];

	private static string[] CompileErrors(string name, string source, ImmutableArray<MetadataReference> references)
	{
		CSharpCompilation compilation = CSharpCompilation.Create(
			name,
			[CSharpSyntaxTree.ParseText(source)],
			references,
			new CSharpCompilationOptions(OutputKind.ConsoleApplication));

		return [.. compilation.GetDiagnostics()
			.Where(d => d.Severity == DiagnosticSeverity.Error)
			.Select(d => d.ToString())];
	}

	private static string FindReadme()
	{
		for (DirectoryInfo? dir = new(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
		{
			string candidate = Path.Combine(dir.FullName, "README.md");
			if (File.Exists(candidate) && File.Exists(Path.Combine(dir.FullName, "TUI.sln")))
			{
				return candidate;
			}
		}

		throw new FileNotFoundException("README.md was not found above the test output directory.");
	}

	[GeneratedRegex(@"```csharp\r?\n(?<code>.*?)```", RegexOptions.Singleline)]
	private static partial Regex CSharpBlock();
}
