# ktsu.TUI

A modern, extensible Text User Interface (TUI) library for .NET applications built on top of Spectre.Console. This library provides a clean, object-oriented approach to building interactive terminal applications with rich visual features.

## Overview

ktsu.TUI is designed with SOLID principles and provides a modular architecture for creating terminal-based user interfaces. It features:

- **Modular Architecture**: Clean separation of concerns with contracts, models, services, and UI elements
- **Event-Driven Rendering**: Efficient rendering system that only updates when needed
- **Extensible Design**: Easy to extend with custom UI elements and layouts
- **Provider Abstraction**: Pluggable console providers (currently supports Spectre.Console)
- **Dependency Injection Ready**: Built with DI in mind for easy integration

## Features

✓ **Rich Text Styling**: Bold, italic, underline, colors, and background colors  
✓ **Border Elements**: Multiple border styles (single, double, rounded, thick, ASCII, none)  
✓ **Layout System**: Stack panels with horizontal/vertical orientation and spacing  
✓ **Input Handling**: Keyboard input processing with ESC to exit  
✓ **Alignment Support**: Horizontal and vertical text alignment  
✓ **Padding & Positioning**: Flexible positioning and padding system  
✓ **Responsive Design**: Automatic sizing based on content and container dimensions  

## Architecture

```
Contracts → Models → Services
    ↓         ↓         ↓
Elements → Layouts → Primitives
    ↓
Provider Abstraction
```

- **Contracts**: Interfaces defining the core abstractions
- **Models**: Data structures for styling, positioning, and input
- **Services**: Core application logic and console providers  
- **Elements**: UI components (primitives like borders/text, layouts like stack panels)
- **Extensions**: Helper methods and utilities

## Installation

Add the NuGet package:

```bash
dotnet add package ktsu.TUI
```

## Quick Start

```csharp
using ktsu.TUI.Core.Services;
using ktsu.TUI.Core.Elements.Primitives;
using ktsu.TUI.Core.Elements.Layouts;
using ktsu.TUI.Core.Models;

// Create console provider
var consoleProvider = new SpectreConsoleProvider();

// Create a simple UI
var ui = new BorderElement
{
    Title = "My TUI App",
    BorderStyle = BorderStyle.Double,
    Child = new TextElement
    {
        Text = "Hello, TUI World!",
        Style = new TextStyle { IsBold = true, Foreground = "green" }
    }
};

// Create and run application
var app = UIApplication.CreateBuilder(consoleProvider)
    .UseRootElement(ui)
    .Build();

await app.RunAsync();
```

## Demo Applications

The library includes several demonstration applications:

- **Sample App** (default): Advanced demo showcasing architecture and features
- **Interactive Demo**: Interactive controls and real-time updates (`--interactive`)  
- **Showcase Demo**: Visual demonstration of all styling features (`--showcase`)

Run demos:
```bash
dotnet run --project TUI.App                    # Sample app
dotnet run --project TUI.App -- --interactive   # Interactive demo
dotnet run --project TUI.App -- --showcase      # Showcase demo
```

## Key Components

### UI Elements
- `TextElement`: Styled text display
- `BorderElement`: Bordered containers with titles
- `StackPanel`: Horizontal/vertical layout container

### Styling
- `TextStyle`: Font styling (bold, italic, underline, colors)
- `BorderStyle`: Border appearance (single, double, rounded, etc.)
- `Padding`: Space around content
- `Position`: Element positioning

### Input System
- Keyboard input handling
- ESC key to exit applications
- Extensible input event system

## Contributing

This project follows clean architecture principles and emphasizes:
- SOLID design principles
- DRY (Don't Repeat Yourself) implementation
- Comprehensive testing
- Clear separation of concerns

## License

MIT License. Copyright (c) ktsu.dev
