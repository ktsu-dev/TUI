## v2.0.7 (patch)

Changes since v2.0.6:

- fix: stretch StackPanel children across the cross axis so alignment applies [patch] ([@Claude](https://github.com/Claude))
- fix: measure a titled BorderElement wide enough to draw its title [patch] ([@Claude](https://github.com/Claude))

## v2.0.6 (patch)

Changes since v2.0.5:

- [patch] Leave the terminal's color for a transparent foreground or background ([@Claude](https://github.com/Claude))

## v2.0.5 (patch)

Changes since v2.0.4:

- test: drive ReadInputAsync through an injected key reader [patch] ([@Claude](https://github.com/Claude))
- Merge remote-tracking branch 'origin/main' into fix/152-keychar ([@Claude](https://github.com/Claude))
- test: cover Ctrl+C and plain C in ToInputResult [patch] ([@Claude](https://github.com/Claude))
- fix: pass the typed character through with each key [patch] ([@Claude](https://github.com/Claude))

## v2.0.4 (patch)

Changes since v2.0.3:

- fix: keep a BorderElement title with a line break on the top border row [patch] ([@Claude](https://github.com/Claude))
- fix: hide and show the cursor on the injected console without moving it [patch] ([@Claude](https://github.com/Claude))

## v2.0.3 (patch)

Changes since v2.0.2:

- fix: arrange the tree on every render pass, not only on a resize [patch] ([@Claude](https://github.com/Claude))

## v2.0.2 (patch)

Changes since v2.0.1:

- refactor: move overlong-word slicing out of WrapText [patch] ([@Claude](https://github.com/Claude))
- fix: keep indentation and spacing when TextElement wraps words [patch] ([@Claude](https://github.com/Claude))
- fix: invalidate each ancestor once instead of 2^depth times [patch] ([@Claude](https://github.com/Claude))

## v2.0.1 (patch)

Changes since v2.0.0:

- test: cover the ArgumentException read-failure path [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- fix: end the run after repeated failed reads instead of spinning forever [patch] ([@matt-edmondson](https://github.com/matt-edmondson))

## v2.0.0 (major)

Changes since v1.0.0:

- Fix analyzer errors that fail the TUI.Cli release publish ([@Claude](https://github.com/Claude))
- fix: measure a container's children from its content origin [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- fix: move a child out of its old container when it is added to another [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- Fix formatting in SampleCLI.cs flagged by IDE0055 ([@Claude](https://github.com/Claude))
- Move CI onto the shared ci-shared.yml pipeline ([@Claude](https://github.com/Claude))
- test: compare wrapped lines with Assert.AreSequenceEqual [patch] ([@Claude](https://github.com/Claude))
- refactor: format TextStyle colours with a switch instead of a nested ternary [patch] ([@Claude](https://github.com/Claude))
- fix: measure, align, wrap and clip TextElement text in terminal cells [patch] ([@Claude](https://github.com/Claude))
- fix: report unnamed TextStyle colours as fixed-width #AARRGGBB so they round trip [patch] ([@Claude](https://github.com/Claude))
- test: cover the width-independent fallbacks of the width-aware measure [patch] ([@Claude](https://github.com/Claude))
- fix: measure a vertical StackPanel child against the panel's width, so wrapped text gets every line [minor] ([@Claude](https://github.com/Claude))
- fix: give StackPanel children that no longer fit no space, so they stop rendering [patch] ([@Claude](https://github.com/Claude))
- fix: shift Spectre cursor positions to 1-based and clip WriteAt to the screen [patch] ([@Claude](https://github.com/Claude))
- refactor: flatten SplitIntoLines with SelectMany [patch] ([@Claude](https://github.com/Claude))
- fix: break TextElement on embedded newlines and clip lines to its width [patch] ([@Claude](https://github.com/Claude))
- fix: count BorderElement's border once when measuring [patch] ([@Claude](https://github.com/Claude))
- Rename TUI.CLI to TUI.Cli for PascalCase consistency ([@Claude](https://github.com/Claude))
- [major] Retire .Core from the published package ID ([@Claude](https://github.com/Claude))
- test: adopt the assertion APIs the MSTest analyzers ask for [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- fix: wire the interactive demo's advertised keys to its handler [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- test: assert the resize log line with Assert.Contains [patch] ([@Claude](https://github.com/Claude))
- fix: re-check the terminal size on every render pass [patch] ([@Claude](https://github.com/Claude))
- fix: reject unrecognized color names on TextStyle [minor] ([@Claude](https://github.com/Claude))
- Gate Dependabot auto-merge on CI actually being green ([@Claude](https://github.com/Claude))
- Merge main into fix/110-ctrl-c-exit-path ([@Claude](https://github.com/Claude))
- test: use Assert.HasCount for the exact-division wrap assertion [patch] ([@Claude](https://github.com/Claude))
- fix: break words longer than twice the wrap width [patch] ([@Claude](https://github.com/Claude))
- test: adopt the assertion APIs the MSTest analyzers ask for [patch] ([@Claude](https://github.com/Claude))
- test: cover the signal response and the interrupt log line [patch] ([@Claude](https://github.com/Claude))
- refactor: use a volatile field for the test double's cursor state [patch] ([@Claude](https://github.com/Claude))
- fix: exit through the normal shutdown path on Ctrl+C [patch] ([@Claude](https://github.com/Claude))
- fix: redraw the whole tree on every render pass [minor] ([@matt-edmondson](https://github.com/matt-edmondson))
- ci: adopt the consolidated .NET workflow [patch] ([@Claude](https://github.com/Claude))
- [patch] Fix NotImplementedException on the default render path ([@Claude](https://github.com/Claude))
- ci: make the SonarQube quality gate opt in [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- ci: adopt the unified dotnet workflow [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- chore: store icon.png in LFS as .gitattributes declares ([@matt-edmondson](https://github.com/matt-edmondson))
- docs: scope build badge to the default branch ([@matt-edmondson](https://github.com/matt-edmondson))
- docs: correct README, DESCRIPTION and TAGS metadata ([@matt-edmondson](https://github.com/matt-edmondson))
- Fix build errors from ktsu.Sdk analyzer updates (KTSU0002 InternalsVisibleTo, KTSU0007 Polyfill PrivateAssets) [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- Sync .github\workflows\dotnet.yml ([@KtsuTools](https://github.com/KtsuTools))
- Sync .editorconfig ([@KtsuTools](https://github.com/KtsuTools))
- Sync global.json ([@KtsuTools](https://github.com/KtsuTools))
- Sync .github\workflows\dotnet.yml ([@KtsuTools](https://github.com/KtsuTools))
- chore: update ktsu.Sdk to 2.21.1 [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- chore: remove unused SourceLink package references ([@matt-edmondson](https://github.com/matt-edmondson))
- chore: remove unused central package and SourceLink refs ([@matt-edmondson](https://github.com/matt-edmondson))
- Sync .github\workflows\dotnet.yml ([@KtsuTools](https://github.com/KtsuTools))
- Update permissions and enhance SonarCloud scanner command with exclusions ([@matt-edmondson](https://github.com/matt-edmondson))
- Remove legacy build scripts ([@matt-edmondson](https://github.com/matt-edmondson))
- Refactor null checks to use Ensure.NotNull for consistency ([@matt-edmondson](https://github.com/matt-edmondson))
- Remove .github\workflows\project.yml ([@matt-edmondson](https://github.com/matt-edmondson))
- Add step to ensure NuGet cache directory exists in workflow ([@matt-edmondson](https://github.com/matt-edmondson))
- migrate to dotnet 10 ([@matt-edmondson](https://github.com/matt-edmondson))
- Update configuration files and scripts for improved build and test processes ([@matt-edmondson](https://github.com/matt-edmondson))
- Update BorderStyle enum and related references across the codebase ([@matt-edmondson](https://github.com/matt-edmondson))
- Enhance documentation and tags for TUI library ([@matt-edmondson](https://github.com/matt-edmondson))
- Implement Directory.Build.props and update test class access modifiers ([@matt-edmondson](https://github.com/matt-edmondson))
- Refactor TUI application classes to use target-typed new expressions and change access modifiers ([@matt-edmondson](https://github.com/matt-edmondson))
- Enhance TUI library with new features and documentation ([@matt-edmondson](https://github.com/matt-edmondson))
- Update .editorconfig settings, .gitignore entries, and various project files ([@matt-edmondson](https://github.com/matt-edmondson))
- Implement Directory.Build.props to manage warnings and errors, enhance BorderElement and TextStyle with convenience properties, and introduce InputModifiers for backward compatibility. Update tests to reflect changes in input handling and property access. ([@matt-edmondson](https://github.com/matt-edmondson))
- Refactor project files to use standard Microsoft SDKs, update package references for central management, and resolve naming conflicts in Padding model. Added Microsoft.Extensions.DependencyInjection.Abstractions package version to Directory.Packages.props. ([@matt-edmondson](https://github.com/matt-edmondson))

## v1.0.55 (patch)

Changes since v1.0.54:

- ci: adopt the consolidated .NET workflow [patch] ([@Claude](https://github.com/Claude))

## v1.0.54 (patch)

Changes since v1.0.53:

- Bump Polyfill from 11.2.0 to 11.3.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.53 (patch)

Changes since v1.0.52:

- Bump the microsoft group with 2 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.52 (patch)

No significant changes detected since v1.0.51.

## v1.0.51 (patch)

Changes since v1.0.50:

- [patch] Fix NotImplementedException on the default render path ([@Claude](https://github.com/Claude))

## v1.0.50 (patch)

Changes since v1.0.49:

- ci: make the SonarQube quality gate opt in [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- ci: adopt the unified dotnet workflow [patch] ([@matt-edmondson](https://github.com/matt-edmondson))

## v1.0.49 (patch)

No significant changes detected since v1.0.48.

## v1.0.48 (patch)

Changes since v1.0.47:

- Bump the ktsu group with 9 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.47 (patch)

Changes since v1.0.46:

- Bump the ktsu group with 9 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.46 (patch)

Changes since v1.0.45:

- Bump the ktsu group with 9 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.45 (patch)

Changes since v1.0.44:

- chore: store icon.png in LFS as .gitattributes declares ([@matt-edmondson](https://github.com/matt-edmondson))
- docs: scope build badge to the default branch ([@matt-edmondson](https://github.com/matt-edmondson))
- docs: correct README, DESCRIPTION and TAGS metadata ([@matt-edmondson](https://github.com/matt-edmondson))

## v1.0.44 (patch)

Changes since v1.0.43:

- Fix build errors from ktsu.Sdk analyzer updates (KTSU0002 InternalsVisibleTo, KTSU0007 Polyfill PrivateAssets) [patch] ([@matt-edmondson](https://github.com/matt-edmondson))
- Sync .github\workflows\dotnet.yml ([@KtsuTools](https://github.com/KtsuTools))
- Sync .editorconfig ([@KtsuTools](https://github.com/KtsuTools))
- Sync global.json ([@KtsuTools](https://github.com/KtsuTools))
- Sync .github\workflows\dotnet.yml ([@KtsuTools](https://github.com/KtsuTools))

## v1.0.43 (patch)

Changes since v1.0.42:

- Bump Polyfill from 11.0.1 to 11.0.2 ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Bump the microsoft group with 2 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.42 (patch)

Changes since v1.0.41:

- chore: update ktsu.Sdk to 2.21.1 [patch] ([@matt-edmondson](https://github.com/matt-edmondson))

## v1.0.41 (patch)

Changes since v1.0.40:

- Bump Spectre.Console from 0.57.1 to 0.57.2 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.40 (patch)

Changes since v1.0.39:

- Bump the ktsu group with 8 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.39 (patch)

Changes since v1.0.38:

- Bump Polyfill from 10.11.1 to 10.11.2 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.38 (patch)

Changes since v1.0.37:

- Bump Polyfill from 10.11.0 to 10.11.1 ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Bump the ktsu group with 8 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.37 (patch)

Changes since v1.0.36:

- Merge remote-tracking branch 'refs/remotes/origin/main' ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync .github\workflows\dotnet.yml ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync global.json ([@ktsu[bot]](https://github.com/ktsu[bot]))

## v1.0.36 (patch)

Changes since v1.0.35:

- chore: remove unused SourceLink package references ([@matt-edmondson](https://github.com/matt-edmondson))
- chore: remove unused central package and SourceLink refs ([@matt-edmondson](https://github.com/matt-edmondson))

## v1.0.35 (patch)

Changes since v1.0.34:

- Bump Spectre.Console from 0.56.0 to 0.57.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.34 (patch)

Changes since v1.0.33:

- Sync .github\workflows\dotnet.yml ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync .github\dependabot.yml ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync .gitignore ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync .gitattributes ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync global.json ([@ktsu[bot]](https://github.com/ktsu[bot]))

## v1.0.33 (patch)

Changes since v1.0.32:

- Bump the microsoft group with 2 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.32 (patch)

Changes since v1.0.31:

- Bump Spectre.Console from 0.55.2 to 0.56.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Bump Polyfill from 10.8.0 to 10.8.1 ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Bump the ktsu group with 3 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.31 (patch)

Changes since v1.0.30:

- Bump Polyfill from 10.7.0 to 10.8.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.30 (patch)

Changes since v1.0.29:

- Bump Polyfill from 10.6.0 to 10.7.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.29 (patch)

Changes since v1.0.28:

- Bump Polyfill from 10.5.1 to 10.6.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.28 (patch)

Changes since v1.0.27:

- Bump MSTest.Sdk from 4.2.2 to 4.2.3 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.27 (patch)

Changes since v1.0.26:

- Bump the microsoft group with 4 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.26 (patch)

Changes since v1.0.25:

- Bump Polyfill from 10.5.0 to 10.5.1 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.25 (patch)

Changes since v1.0.24:

- Bump Polyfill from 10.4.0 to 10.5.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.24 (patch)

Changes since v1.0.23:

- Bump Polyfill from 10.3.0 to 10.4.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.23 (patch)

Changes since v1.0.22:

- Bump MSTest.Sdk from 4.2.1 to 4.2.2 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.22 (patch)

Changes since v1.0.21:

- Bump the microsoft group with 4 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.21 (patch)

Changes since v1.0.20:

- Bump Spectre.Console from 0.55.0 to 0.55.2 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.20 (patch)

Changes since v1.0.19:

- Bump the microsoft group with 4 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.19 (patch)

Changes since v1.0.18:

- Bump Polyfill from 10.2.0 to 10.3.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.18 (patch)

Changes since v1.0.17:

- Bump Polyfill from 10.1.1 to 10.2.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.17 (patch)

Changes since v1.0.16:

- Bump Polyfill from 10.0.0 to 10.1.1 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.16 (patch)

Changes since v1.0.15:

- Bump MSTest.Sdk from 4.1.0 to 4.2.1 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.15 (patch)

Changes since v1.0.14:

- Bump Polyfill from 9.24.0 to 10.0.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.14 (patch)

Changes since v1.0.13:

- Bump Spectre.Console from 0.54.0 to 0.55.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Bump Polyfill from 9.23.0 to 9.24.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.13 (patch)

Changes since v1.0.12:

- Bump Polyfill from 9.22.0 to 9.23.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Bump Polyfill from 9.18.0 to 9.22.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Bump the microsoft group with 4 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Bump the microsoft group with 4 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Bump Polyfill from 9.17.0 to 9.18.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Bump Polyfill from 9.13.0 to 9.17.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.12 (patch)

Changes since v1.0.11:

- Bump Polyfill from 9.12.0 to 9.13.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.11 (patch)

Changes since v1.0.10:

- Bump Polyfill from 9.11.0 to 9.12.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.10 (patch)

Changes since v1.0.9:

- Bump Polyfill from 9.10.0 to 9.11.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.9 (patch)

Changes since v1.0.8:

- Sync .github\workflows\dotnet.yml ([@KtsuTools](https://github.com/KtsuTools))

## v1.0.9-pre.1 (prerelease)

Changes since v1.0.8:

- Sync .github\workflows\dotnet.yml ([@KtsuTools](https://github.com/KtsuTools))

## v1.0.8 (patch)

Changes since v1.0.7:

- Bump Polyfill from 9.8.1 to 9.9.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.7 (patch)

Changes since v1.0.6:

- Update permissions and enhance SonarCloud scanner command with exclusions ([@matt-edmondson](https://github.com/matt-edmondson))

## v1.0.7-pre.1 (prerelease)

Changes since v1.0.6:

- Sync .github\workflows\dotnet.yml ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync .github\workflows\dotnet.yml ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync .github\workflows\dotnet.yml ([@ktsu[bot]](https://github.com/ktsu[bot]))

## v1.0.6 (patch)

Changes since v1.0.5:

- Remove legacy build scripts ([@matt-edmondson](https://github.com/matt-edmondson))

## v1.0.5 (patch)

Changes since v1.0.4:

- Sync .github\workflows\dotnet.yml ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync global.json ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Bump the microsoft group with 4 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Merge remote-tracking branch 'refs/remotes/origin/main' ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync scripts\update-winget-manifests.ps1 ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync scripts\update-winget-manifests.ps1 ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync scripts\PSBuild.psm1 ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Bump Polyfill from 9.8.0 to 9.8.1 ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Bump MSTest.Sdk from 4.0.2 to 4.1.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Merge remote-tracking branch 'refs/remotes/origin/main' ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync scripts\PSBuild.psm1 ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync scripts\PSBuild.psm1 ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync global.json ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Bump Polyfill from 9.7.7 to 9.8.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Merge remote-tracking branch 'refs/remotes/origin/main' ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync global.json ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Merge remote-tracking branch 'refs/remotes/origin/main' ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync global.json ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync COPYRIGHT.md ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Merge remote-tracking branch 'refs/remotes/origin/main' ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync scripts\PSBuild.psm1 ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync scripts\update-winget-manifests.ps1 ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync global.json ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync COPYRIGHT.md ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Bump the ktsu group with 3 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.5-pre.10 (prerelease)

Changes since v1.0.5-pre.9:

- Bump the microsoft group with 4 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.5-pre.9 (prerelease)

Changes since v1.0.5-pre.8:

- Merge remote-tracking branch 'refs/remotes/origin/main' ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync scripts\update-winget-manifests.ps1 ([@ktsu[bot]](https://github.com/ktsu[bot]))

## v1.0.5-pre.8 (prerelease)

Changes since v1.0.5-pre.7:

- Sync scripts\update-winget-manifests.ps1 ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync scripts\PSBuild.psm1 ([@ktsu[bot]](https://github.com/ktsu[bot]))

## v1.0.5-pre.7 (prerelease)

Changes since v1.0.5-pre.6:

- Bump Polyfill from 9.8.0 to 9.8.1 ([@dependabot[bot]](https://github.com/dependabot[bot]))
- Bump MSTest.Sdk from 4.0.2 to 4.1.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.5-pre.6 (prerelease)

Changes since v1.0.5-pre.5:

- Merge remote-tracking branch 'refs/remotes/origin/main' ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync scripts\PSBuild.psm1 ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync scripts\PSBuild.psm1 ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync global.json ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Bump Polyfill from 9.7.7 to 9.8.0 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.5-pre.5 (prerelease)

Changes since v1.0.5-pre.4:

- Merge remote-tracking branch 'refs/remotes/origin/main' ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync global.json ([@ktsu[bot]](https://github.com/ktsu[bot]))

## v1.0.5-pre.4 (prerelease)

Changes since v1.0.5-pre.3:

- Merge remote-tracking branch 'refs/remotes/origin/main' ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync global.json ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync COPYRIGHT.md ([@ktsu[bot]](https://github.com/ktsu[bot]))

## v1.0.5-pre.3 (prerelease)

Changes since v1.0.5-pre.2:

- Merge remote-tracking branch 'refs/remotes/origin/main' ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync scripts\PSBuild.psm1 ([@ktsu[bot]](https://github.com/ktsu[bot]))

## v1.0.5-pre.2 (prerelease)

Changes since v1.0.5-pre.1:

- Sync scripts\update-winget-manifests.ps1 ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync global.json ([@ktsu[bot]](https://github.com/ktsu[bot]))
- Sync COPYRIGHT.md ([@ktsu[bot]](https://github.com/ktsu[bot]))

## v1.0.5-pre.1 (prerelease)

Changes since v1.0.4:

- Bump the ktsu group with 3 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.4 (patch)

Changes since v1.0.3:

- Refactor null checks to use Ensure.NotNull for consistency ([@matt-edmondson](https://github.com/matt-edmondson))
- Remove .github\workflows\project.yml ([@matt-edmondson](https://github.com/matt-edmondson))

## v1.0.3 (patch)

Changes since v1.0.2:

- Add step to ensure NuGet cache directory exists in workflow ([@matt-edmondson](https://github.com/matt-edmondson))

## v1.0.2 (patch)

Changes since v1.0.1:

- migrate to dotnet 10 ([@matt-edmondson](https://github.com/matt-edmondson))
- Update configuration files and scripts for improved build and test processes ([@matt-edmondson](https://github.com/matt-edmondson))
- Update BorderStyle enum and related references across the codebase ([@matt-edmondson](https://github.com/matt-edmondson))
- Enhance documentation and tags for TUI library ([@matt-edmondson](https://github.com/matt-edmondson))
- Implement Directory.Build.props and update test class access modifiers ([@matt-edmondson](https://github.com/matt-edmondson))
- Refactor TUI application classes to use target-typed new expressions and change access modifiers ([@matt-edmondson](https://github.com/matt-edmondson))
- Enhance TUI library with new features and documentation ([@matt-edmondson](https://github.com/matt-edmondson))
- Update .editorconfig settings, .gitignore entries, and various project files ([@matt-edmondson](https://github.com/matt-edmondson))

## v1.0.2-pre.3 (prerelease)

Changes since v1.0.2-pre.2:

- Bump the ktsu group with 4 updates ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.2-pre.2 (prerelease)

Changes since v1.0.2-pre.1:

- Bump Microsoft.NET.Test.Sdk from 17.14.0 to 17.14.1 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.2-pre.1 (prerelease)

Changes since v1.0.1:

- Update: - MSTest.TestAdapter to 3.9.1 - MSTest.TestFramework to 3.9.1 ([@dependabot[bot]](https://github.com/dependabot[bot]))

## v1.0.1 (patch)

Changes since v1.0.0:

- Implement Directory.Build.props to manage warnings and errors, enhance BorderElement and TextStyle with convenience properties, and introduce InputModifiers for backward compatibility. Update tests to reflect changes in input handling and property access. ([@matt-edmondson](https://github.com/matt-edmondson))
- Refactor project files to use standard Microsoft SDKs, update package references for central management, and resolve naming conflicts in Padding model. Added Microsoft.Extensions.DependencyInjection.Abstractions package version to Directory.Packages.props. ([@matt-edmondson](https://github.com/matt-edmondson))

## v1.0.1-pre.1 (prerelease)

Changes since v1.0.0:

- Refactor project files to use standard Microsoft SDKs, update package references for central management, and resolve naming conflicts in Padding model. Added Microsoft.Extensions.DependencyInjection.Abstractions package version to Directory.Packages.props. ([@matt-edmondson](https://github.com/matt-edmondson))

## v1.0.0 (major)

- Add initial implementation of TUI library with core components, UI elements, and demos. Includes cursor ignore files and project configuration for package management. ([@matt-edmondson](https://github.com/matt-edmondson))
- Initial commit for TUI ([@matt-edmondson](https://github.com/matt-edmondson))

