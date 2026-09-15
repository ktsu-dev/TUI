# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
dotnet build                                    # Build the solution
dotnet test                                     # Run all tests
dotnet test --filter "FullyQualifiedName~TestName"  # Run specific test
dotnet test TUI.Test --logger "console;verbosity=detailed"  # Detailed test output
```

## Running the Demo App

```bash
dotnet run --project TUI.App                    # Sample app (default)
dotnet run --project TUI.App -- --interactive   # Interactive demo
dotnet run --project TUI.App -- --showcase      # Showcase demo
```

## Architecture

ktsu.TUI is a Text User Interface library built on Spectre.Console with this structure:

```
Contracts → Models → Services
    ↓         ↓         ↓
Elements → Layouts → Primitives
    ↓
Provider Abstraction (SpectreConsoleProvider)
```

**Key namespaces:**
- `ktsu.TUI.Core.Contracts` - Interfaces (`IUIElement`, `IUIContainer`, `IConsoleProvider`, `IUIApplication`)
- `ktsu.TUI.Core.Elements` - Base classes (`UIElementBase`, `UIContainerBase`)
- `ktsu.TUI.Core.Elements.Primitives` - UI components (`TextElement`, `BorderElement`)
- `ktsu.TUI.Core.Elements.Layouts` - Layout containers (`StackPanel`)
- `ktsu.TUI.Core.Models` - Data structures (`Position`, `Dimensions`, `Padding`, `TextStyle`, `InputResult`)
- `ktsu.TUI.Core.Services` - Application services (`UIApplication`, `SpectreConsoleProvider`)

**Rendering flow:**
1. `UIApplication` manages the main loop and input processing
2. Elements are arranged via `ArrangeChildren()` which sets child `Position` and `Dimensions`
3. Each pass is a full clear followed by a full redraw: `UIApplication.Render()` clears the
   console and every visible element draws again. `Invalidate()` marks an element as changed and
   raises `Invalidated`, but it does not gate drawing — a full clear combined with a dirty-only
   redraw erases static elements rather than preserving them (ktsu-dev/TUI#109)
4. `UIContainerBase.Render()` renders itself then all visible children

## Code Patterns

**Adding children to containers** - Use `AddChild()` or collection initializer (not direct assignment):
```csharp
var panel = new StackPanel
{
    new TextElement { Text = "Hello" },
    new BorderElement { /* ... */ }
};
// or
panel.AddChild(new TextElement { Text = "World" });
```

**Creating a UI application:**
```csharp
var app = UIApplication.CreateBuilder(new SpectreConsoleProvider())
    .UseRootElement(myRootElement)
    .Build();
await app.RunAsync();
```

**Custom UI elements** - Extend `UIElementBase` and implement `OnRender()`:
```csharp
protected override void OnRender(IConsoleProvider provider)
{
    // Use provider to draw content
}
```

## Testing

- Framework: MSTest with Moq for mocking
- When mocking `IUIElement`, explicitly set `IsVisible` to `true` (Moq defaults bools to false):
```csharp
var mockChild = new Mock<IUIElement>();
mockChild.Setup(c => c.IsVisible).Returns(true);
```

### Render tests

Property round-tripping is not enough on its own. Every alignment combination of `TextElement`,
every titled `BorderElement` with the default `TitleAlignment`, and every `BorderStyle.None`
element once threw `NotImplementedException` from the render path, and the suite stayed green
throughout because no test called `Render`.

`RecordingConsoleProvider` (in `TUI.Test`) is the test double for this. It implements
`IConsoleProvider` and records every `WriteAt` call rather than drawing, so a test can assert
*what* was drawn and *where*:

```csharp
RecordingConsoleProvider provider = new();
element.Render(provider);
Assert.AreEqual(0, provider.WritesOf("abc").Single().Position.X);
```

Rendering to the real `SpectreConsoleProvider` in a test writes to the runner's console and
leaves the output unobservable, so use the recorder instead.

Any new element needs render coverage across the full matrix of whatever enum drives its
layout — that is exactly where the bug above lived. `TextElementTests` and
`BorderElementRenderTests` use `[DynamicData]` over `Enum.GetValues<T>()` so a newly added
enum member is covered automatically rather than silently skipped.

Two sizing traps when writing these:

- `Dimensions.WithoutPadding` floors at zero, and an element with an empty content area returns
  from `OnRender` before drawing. An element must be larger than its own padding or it draws
  nothing and the assertion fails for a reason unrelated to what is under test.
- `BorderElement` only draws its title when `Width > 4`, and draws no border at all below 2x2.

## File Headers

All source files require this copyright header:
```csharp
// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.
```
