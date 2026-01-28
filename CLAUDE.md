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
3. `Invalidate()` marks elements dirty; only dirty elements re-render
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

## File Headers

All source files require this copyright header:
```csharp
// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.
```
