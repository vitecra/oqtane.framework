# 3D Visualization Module for Oqtane

## Overview

This module provides 3D visualization capabilities with a C# wrapper for creating and manipulating 3D objects in Oqtane Framework. It replaces the need for Three.js by providing a pure C# API with JavaScript interop for rendering.

## Features

- **C# Wrapper API**: Full C# API for 3D object manipulation
- **Basic Primitives**: Support for Cube, Sphere, and Plane objects
- **Transformation Support**: Position, Rotation, and Scale for all objects
- **Color Management**: RGBA color support with predefined color constants
- **Scene Management**: Add, remove, and clear objects from the scene
- **Animation**: Built-in animation support with start/stop controls
- **Interactive Controls**: Add objects dynamically with random positions and colors

## Architecture

### C# Models (Shared Layer)

The module consists of several C# classes that provide the wrapper functionality:

- **Vector3**: Represents 3D coordinates and provides vector math operations
  - Add, Subtract, Multiply, Normalize, Dot, Cross product
  - Predefined constants: Zero, One, Forward, Up, Right

- **Color**: Manages RGBA colors
  - Predefined colors: Red, Green, Blue, White, Black, Yellow, Cyan, Magenta, Gray
  - ToRgba() and ToHex() conversion methods

- **Object3D**: Base class for all 3D objects
  - Properties: Position, Rotation, Scale, Color, Visible
  - Derived classes: Cube, Sphere, Plane

- **Camera**: Manages the viewport perspective
  - Properties: Position, Target, Up, FieldOfView, AspectRatio, Near, Far
  - LookAt() method for camera targeting

- **Scene**: Container for all 3D objects and camera
  - Add(), Remove(), Clear() methods for object management
  - Background color support

### JavaScript Interop (Client Layer)

The rendering is handled by JavaScript through the `Module.js` file which provides:

- Canvas initialization and management
- Drawing methods for Cube, Sphere, and Plane
- Animation support with rotation updates
- Cleanup and disposal methods

### Blazor Component (Client Layer)

The `Index.razor` component provides:

- Interactive UI with controls to add objects
- Animation toggle
- Real-time rendering of the 3D scene
- Clean disposal of resources

## Usage Example

```csharp
// Create a scene
var scene = new Scene();
scene.BackgroundColor = new Color(240, 240, 240);

// Create a cube
var cube = new Cube(1.5);
cube.Position = new Vector3(-2, 0, 0);
cube.Color = Color.Blue;
scene.Add(cube);

// Create a sphere
var sphere = new Sphere(1.0);
sphere.Position = new Vector3(2, 0, 0);
sphere.Color = Color.Red;
scene.Add(sphere);

// Create a plane
var plane = new Plane(2, 2);
plane.Position = new Vector3(0, -2, 0);
plane.Color = Color.Green;
scene.Add(plane);
```

## Implementation Details

### Code Consolidation

This implementation uses "Option C" - C# Wrapper approach:
- All 3D logic is written in C#
- JavaScript is used only for low-level canvas rendering
- State management happens entirely in C#
- Easy to extend and maintain in C#

### Rendering Pipeline

1. C# creates and manages 3D objects
2. Objects are stored in a Scene
3. The Blazor component calls JavaScript interop for each object
4. JavaScript renders objects onto an HTML5 Canvas
5. Animation updates are coordinated from C#

## Benefits Over Three.js

1. **Type Safety**: Full C# type checking at compile time
2. **Code Consolidation**: All logic in C# instead of split between C# and JavaScript
3. **Better Debugging**: Debug entire application in C# debugger
4. **Maintainability**: Single language codebase
5. **Integration**: Native integration with Oqtane and Blazor lifecycle

## Future Enhancements

Potential improvements:
- Additional primitives (Cylinder, Cone, etc.)
- Lighting system
- Materials and textures
- Mouse interaction (rotation, zoom, pan)
- WebGL rendering for better performance
- Shadow rendering
- Export/Import scene configurations

## Module Structure

```
Oqtane.Shared/Modules/Visualization3D/
  └── Models/
      ├── Vector3.cs        # 3D vector math
      ├── Color.cs          # Color management
      ├── Object3D.cs       # Base class and primitives
      ├── Camera.cs         # Camera system
      └── Scene.cs          # Scene management

Oqtane.Client/Modules/Visualization3D/
  ├── ModuleInfo.cs         # Module registration
  └── Index.razor           # Main component UI

Oqtane.Server/wwwroot/Modules/Oqtane.Modules.Visualization3D/
  └── Module.js             # JavaScript rendering
```

## License

This module is part of the Oqtane Framework and follows the same MIT license.
