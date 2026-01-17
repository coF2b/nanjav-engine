# nanjav Game Engine

## Documentation for version 0.1.0

A lightweight and simple 2D game engine in C#, built on Silk.NET for rapid development of 2D games with input handling and OpenGL rendering.

## 📋 Table of Contents

1. [Core Concepts](#core-concepts)
2. [Architecture](#architecture)
3. [Quick Start](#quick-start)
4. [API Reference](#api-reference)
5. [Usage Examples](#usage-examples)

---

## Core Concepts

### GameObject (Game Object)

The fundamental unit of a scene. Every object in the game is a GameObject, which can have components and child objects.

**Properties:**
- `Name` — name of the object
- `IsActive` — whether the object is active
- `Transform` — position, rotation, and scale
- `Parent` — parent object

### Component

A component-based system. Components are added to GameObjects and give them functionality.

**Built-in Components:**
- `SpriteRenderer` — renders a rectangle on screen
- `Transform` — manages position and transformation
- `BoxCollider2D` — 2D box collision detection
- `Rigidbody2D` — physics, gravity, and velocity

### Transform (Transformation)

Responsible for position, rotation, and scale of an object.

**Properties:**
- `X, Y` — screen coordinates
- `Rotation` — rotation angle (in radians)
- `ScaleX, ScaleY` — scale on each axis

### BoxCollider2D

Component for 2D box collision detection.

**Properties:**
- `Width` — collider width
- `Height` — collider height
- `Left, Right, Top, Bottom` — collision bounds (read-only)

**Methods:**
- `IsColliding(BoxCollider2D other)` — check if colliding with another collider

### Rigidbody2D

Component for physics simulation with gravity and velocity.

**Properties:**
- `Mass` — object mass (default: 1.0)
- `GravityScale` — gravity multiplier (default: 9.7)
- `Velocity` — current velocity as Vector2

**Methods:**
- `ApplyForce(Vector2 force)` — apply force to velocity
- `Jump(float jumpForce)` — apply upward velocity for jumping

### Camera2D

Component for 2D camera control with zoom and position tracking.

**Properties:**
- `Position` — camera position in world space
- `Zoom` — camera zoom level (default: 1.0)
- `Offset` — screen center offset

**Methods:**
- `GetViewMatrix()` — returns the view matrix for rendering

---

## Architecture

### Core Engine Components

```
nanjav/
├── core/
│   ├── GameWindowApp.cs      # Main window and game loop
│   ├── Renderer.cs           # OpenGL rendering
│   └── GameObject.cs         # Object system
├── input/
│   ├── Keyboard.cs           # Keyboard input
│   ├── Mouse.cs              # Mouse input
│   ├── Keys.cs               # Key codes
│   └── MouseButton.cs        # Mouse button codes
└── ...
```

### Game Loop

1. **Load** — resource initialization
2. **Update** — logic updates (called every frame)
3. **Render** — graphics rendering
4. **Close** — resource cleanup

---

## Physics and Collision System

The engine includes a built-in 2D physics system with gravity, velocity, and collision detection.

### How Collisions Work

- **BoxCollider2D** stores all active colliders in a static list `AllColliders`
- **Rigidbody2D** automatically checks collisions with all colliders each frame
- Vertical collisions (falling/jumping) are handled separately from horizontal collisions
- Objects with a Rigidbody2D will fall due to gravity and respond to collisions

### Setting Up Physics

```csharp
// Create a player with physics
var player = GameObject.Create("Player");
player.Transform.X = 100;
player.Transform.Y = 100;

// Add sprite for visuals
var sprite = player.AddComponent<SpriteRenderer>();
sprite.Width = 50;
sprite.Height = 50;
sprite.Color = new Vector3(0f, 1f, 0f);

// Add collider for collision detection
var collider = player.AddComponent<BoxCollider2D>();
collider.Width = 50;
collider.Height = 50;

// Add rigidbody for physics
var rigidbody = player.AddComponent<Rigidbody2D>();
rigidbody.GravityScale = 9.7f;

game.AddGameObject(player);
```

### Jumping and Movement with Physics

```csharp
game.OnUpdate += (dt) =>
{
    var player = // ... get player
    var rigidbody = player.GetComponent<Rigidbody2D>();
    
    // Horizontal movement
    if (game.Keyboard.IsKeyDown(Keys.A))
        rigidbody.Velocity.X = -300f;
    else if (game.Keyboard.IsKeyDown(Keys.D))
        rigidbody.Velocity.X = 300f;
    else
        rigidbody.Velocity.X = 0;
    
    // Jumping
    if (game.Keyboard.IsKeyPressed(Keys.Space))
        rigidbody.Jump(-400f); // Negative = upward
};
```

---

## Quick Start

### Basic Program Structure

```csharp
using nanjav.core;
using nanjav.input;
using System.Numerics;

class Program
{
    static void Main()
    {
        // Create window
        var game = new GameWindowApp("My Game", 1280, 720);

        // Subscribe to events
        game.OnLoad += OnLoad;
        game.OnUpdate += OnUpdate;
        game.OnRender += OnRender;

        // Run the game
        game.Run();
    }

    static void OnLoad()
    {
        // Game initialization
    }

    static void OnUpdate(double deltaTime)
    {
        // Logic updates
    }

    static void OnRender(double deltaTime)
    {
        // Rendering
    }
}
```

### Creating Your First Object

```csharp
// Create GameObject
var player = GameObject.Create("Player");
player.Transform.X = 100;
player.Transform.Y = 100;

// Add SpriteRenderer component
var sprite = player.AddComponent<SpriteRenderer>();
sprite.Width = 50;
sprite.Height = 50;
sprite.Color = new Vector3(1f, 0f, 0f); // Red color

// Add to scene
game.AddGameObject(player);
```

---

## API Reference

### GameWindowApp

Main class for managing the window and game loop.

#### Constructor
```csharp
GameWindowApp(string title = "nanjav Game", int width = 800, int height = 600, bool vSync = true)
```

#### Main Methods
```csharp
void Run()                           // Start game loop
void Close()                         // Close window
void AddGameObject(GameObject obj)   // Add object to scene
void SetTitle(string title)          // Set window title
void SetSize(int width, int height)  // Change window size
void ToggleFullscreen()              // Toggle fullscreen mode
void CenterWindow()                  // Center window on screen
```

#### Properties
```csharp
Keyboard Keyboard              // Keyboard object
Mouse Mouse                    // Mouse object
Renderer Renderer              // Renderer
int Width                      // Window width
int Height                     // Window height
double Time                    // Game time
bool IsRunning                 // Is game running
```

#### Events
```csharp
event Action? OnLoad                    // On load
event Action<double>? OnUpdate          // On update (deltaTime passed)
event Action<double>? OnRender          // On render (deltaTime passed)
event Action<int, int>? OnResize        // On window resize
event Action? OnClose                   // On close
```

---

### GameObject

Represents an object in the game.

#### Methods
```csharp
static GameObject Create(string name = "GameObject")    // Create new object
T AddComponent<T>() where T : Component, new()         // Add component
T? GetComponent<T>() where T : Component               // Get component
void AddChild(GameObject child)                        // Add child object
void RemoveChild(GameObject child)                     // Remove child object
IReadOnlyList<GameObject> GetChildren()               // Get child objects
void SetActive(bool active)                           // Activate/deactivate
void Update(double deltaTime)                         // Update logic
void Render()                                         // Render
```

#### Properties
```csharp
string Name                    // Object name
bool IsActive                  // Is active
Transform Transform            // Transformation component
GameObject? Parent             // Parent object
```

---

### Keyboard

Manages keyboard input.

#### Methods
```csharp
bool IsKeyDown(Keys key)           // Key is held down
bool IsKeyPressed(Keys key)        // Key was pressed this frame
bool IsKeyReleased(Keys key)       // Key was released this frame
bool IsAnyKeyDown()                // Any key is held down
IReadOnlySet<Keys> GetPressedKeys() // Get all pressed keys
```

#### Example
```csharp
if (game.Keyboard.IsKeyPressed(Keys.Space))
{
    // Spacebar was pressed
}

if (game.Keyboard.IsKeyDown(Keys.W))
{
    // W is held down
}
```

---

### Mouse

Manages mouse input.

#### Methods
```csharp
bool IsButtonDown(MouseButton button)      // Button is held
bool IsButtonPressed(MouseButton button)   // Button pressed this frame
bool IsButtonReleased(MouseButton button)  // Button released this frame
bool IsAnyButtonDown()                     // Any button is held
bool IsMoving()                            // Mouse is moving
bool IsScrolling()                         // Wheel is scrolling
IReadOnlySet<MouseButton> GetPressedButtons() // Get pressed buttons
```

#### Properties
```csharp
Vector2 Position          // Current mouse position
Vector2 PreviousPosition  // Previous mouse position
Vector2 Delta             // Position difference (movement per frame)
float ScrollDelta         // Scroll per frame
```

#### Example
```csharp
if (game.Mouse.IsButtonPressed(MouseButton.Left))
{
    var pos = game.Mouse.Position;
    // Handle click at position pos
}
```

---

### SpriteRenderer

Component for displaying 2D sprites.

#### Properties
```csharp
float Width                        // Sprite width
float Height                       // Sprite height
Vector3 Color                      // Color (RGB, 0 to 1)
```

#### Example
```csharp
var sprite = obj.AddComponent<SpriteRenderer>();
sprite.Width = 100;
sprite.Height = 100;
sprite.Color = new Vector3(0.5f, 0.2f, 0.8f); // Purple
```

---

### Transform

Manages object position and transformation.

#### Properties
```csharp
float X           // Position on X axis
float Y           // Position on Y axis
float Rotation    // Rotation angle (in radians)
float ScaleX      // Scale on X axis
float ScaleY      // Scale on Y axis
Transform? Parent // Parent transformation
```

#### Example
```csharp
var obj = GameObject.Create("Box");
obj.Transform.X = 250;
obj.Transform.Y = 150;
obj.Transform.ScaleX = 2f;
obj.Transform.ScaleY = 1.5f;
```

---

### BoxCollider2D

Component for 2D box collision detection. All colliders are automatically tracked for collision checks.

#### Properties
```csharp
float Width                  // Collider width
float Height                 // Collider height
float Left                   // Left edge position (read-only)
float Right                  // Right edge position (read-only)
float Top                    // Top edge position (read-only)
float Bottom                 // Bottom edge position (read-only)
List<BoxCollider2D> AllColliders  // Static list of all colliders
```

#### Methods
```csharp
bool IsColliding(BoxCollider2D other)  // Check collision with another collider
```

#### Example
```csharp
var obj = GameObject.Create("Ground");
var collider = obj.AddComponent<BoxCollider2D>();
collider.Width = 800;
collider.Height = 50;
obj.Transform.Y = 550;

game.AddGameObject(obj);
```

---

### Rigidbody2D

Component for 2D physics simulation with gravity, velocity, and collision response.

#### Properties
```csharp
float Mass           // Object mass (default: 1.0)
float GravityScale   // Gravity multiplier (default: 9.7)
Vector2 Velocity     // Current velocity
```

#### Methods
```csharp
void ApplyForce(Vector2 force)     // Add force to velocity
void Jump(float jumpForce)         // Apply upward velocity (use negative values)
void Update(double deltaTime)      // Update physics (called automatically)
```

#### Behavior
- Automatically applies gravity each frame
- Checks collisions with all BoxCollider2D objects
- Prevents falling through solid objects
- Stops horizontal movement when hitting walls
- Handles jumping on platforms

#### Example
```csharp
var player = GameObject.Create("Player");
var collider = player.AddComponent<BoxCollider2D>();
collider.Width = 50;
collider.Height = 50;

var rigidbody = player.AddComponent<Rigidbody2D>();
rigidbody.Mass = 1.0f;
rigidbody.GravityScale = 9.7f;

// In update loop
if (game.Keyboard.IsKeyPressed(Keys.Space))
    rigidbody.Jump(-400f);
```

---

### Camera2D

Component for 2D camera control with position, zoom, and view matrix.

#### Constructor
```csharp
Camera2D(float screenWidth, float screenHeight)
```

#### Properties
```csharp
Vector2 Position  // Camera position in world space
float Zoom        // Zoom level (default: 1.0)
Vector2 Offset    // Screen center offset
```

#### Methods
```csharp
Matrix4x4 GetViewMatrix()  // Get the view matrix for rendering
```

#### Example
```csharp
var camera = new Camera2D(1280, 720);
camera.Position = new Vector2(640, 360);
camera.Zoom = 1.5f;
game.SetCamera(camera);
```

---

### Transform

Manages object position and transformation.

#### Properties
```csharp
float X           // Position on X axis
float Y           // Position on Y axis
float Rotation    // Rotation angle (in radians)
float ScaleX      // Scale on X axis
float ScaleY      // Scale on Y axis
Transform? Parent // Parent transformation
```

#### Example
```csharp
var obj = GameObject.Create("Box");
obj.Transform.X = 250;
obj.Transform.Y = 150;
obj.Transform.ScaleX = 2f;
obj.Transform.ScaleY = 1.5f;
```

---

## Usage Examples

### Example 1: Simple Moving Object

```csharp
class Game
{
    private GameWindowApp _game;
    private GameObject _player;

    public Game()
    {
        _game = new GameWindowApp("Player", 800, 600);
        _game.OnLoad += OnLoad;
        _game.OnUpdate += OnUpdate;
    }

    void OnLoad()
    {
        // Create player
        _player = GameObject.Create("Player");
        _player.Transform.X = 400;
        _player.Transform.Y = 300;

        var sprite = _player.AddComponent<SpriteRenderer>();
        sprite.Width = 50;
        sprite.Height = 50;
        sprite.Color = new Vector3(0f, 1f, 0f); // Green

        _game.AddGameObject(_player);
    }

    void OnUpdate(double deltaTime)
    {
        // Move player
        float speed = 300f;
        
        if (_game.Keyboard.IsKeyDown(Keys.W))
            _player.Transform.Y -= (float)(speed * deltaTime);
        if (_game.Keyboard.IsKeyDown(Keys.S))
            _player.Transform.Y += (float)(speed * deltaTime);
        if (_game.Keyboard.IsKeyDown(Keys.A))
            _player.Transform.X -= (float)(speed * deltaTime);
        if (_game.Keyboard.IsKeyDown(Keys.D))
            _player.Transform.X += (float)(speed * deltaTime);
    }

    public void Run() => _game.Run();
}
```

### Example 2: Parent-Child System

```csharp
// Parent
var parent = GameObject.Create("Parent");
parent.Transform.X = 200;
parent.Transform.Y = 200;

// Child
var child = GameObject.Create("Child");
child.Transform.X = 50;  // Relative to parent
child.Transform.Y = 0;

parent.AddChild(child);  // Add child
game.AddGameObject(parent); // Add only parent

// Moving parent automatically affects child
```

### Example 3: Handling Mouse Clicks

```csharp
game.OnUpdate += (dt) =>
{
    if (game.Mouse.IsButtonPressed(MouseButton.Left))
    {
        var clickPos = game.Mouse.Position;
        
        // Create object at click position
        var obj = GameObject.Create("Clicked");
        obj.Transform.X = clickPos.X;
        obj.Transform.Y = clickPos.Y;

        var sprite = obj.AddComponent<SpriteRenderer>();
        sprite.Width = 30;
        sprite.Height = 30;
        sprite.Color = new Vector3(1f, 0f, 0f);

        game.AddGameObject(obj);
    }
};
```

### Example 4: 2D Platformer with Physics

```csharp
class PlatformerGame
{
    private GameWindowApp _game;
    private GameObject _player;
    private GameObject _ground;

    public PlatformerGame()
    {
        _game = new GameWindowApp("Platformer", 1280, 720);
        _game.OnLoad += OnLoad;
        _game.OnUpdate += OnUpdate;
    }

    void OnLoad()
    {
        // Create ground platform
        _ground = GameObject.Create("Ground");
        _ground.Transform.X = 0;
        _ground.Transform.Y = 600;

        var groundSprite = _ground.AddComponent<SpriteRenderer>();
        groundSprite.Width = 1280;
        groundSprite.Height = 120;
        groundSprite.Color = new Vector3(0.5f, 0.5f, 0.5f);

        var groundCollider = _ground.AddComponent<BoxCollider2D>();
        groundCollider.Width = 1280;
        groundCollider.Height = 120;

        _game.AddGameObject(_ground);

        // Create player
        _player = GameObject.Create("Player");
        _player.Transform.X = 100;
        _player.Transform.Y = 500;

        var playerSprite = _player.AddComponent<SpriteRenderer>();
        playerSprite.Width = 50;
        playerSprite.Height = 50;
        playerSprite.Color = new Vector3(0f, 0f, 1f);

        var playerCollider = _player.AddComponent<BoxCollider2D>();
        playerCollider.Width = 50;
        playerCollider.Height = 50;

        var playerRigidbody = _player.AddComponent<Rigidbody2D>();
        playerRigidbody.GravityScale = 9.7f;

        _game.AddGameObject(_player);
    }

    void OnUpdate(double deltaTime)
    {
        var rigidbody = _player.GetComponent<Rigidbody2D>();
        if (rigidbody == null) return;

        // Horizontal movement
        if (_game.Keyboard.IsKeyDown(Keys.A))
            rigidbody.Velocity.X = -300f;
        else if (_game.Keyboard.IsKeyDown(Keys.D))
            rigidbody.Velocity.X = 300f;
        else
            rigidbody.Velocity.X = 0f;

        // Jump
        if (_game.Keyboard.IsKeyPressed(Keys.Space))
            rigidbody.Jump(-400f);
    }

    public void Run() => _game.Run();
}
```

---

## Tips and Recommendations

1. **Optimization**: Deactivate objects with `SetActive(false)` instead of deleting for better performance
2. **Scene Structure**: Use parent objects to organize scene hierarchy
3. **DeltaTime**: Always use `deltaTime` for smooth movement independent of FPS
4. **Custom Components**: Extend the `Component` base class for your own components
5. **Colors**: RGB values should be from 0 to 1 (not 0 to 255)

---

## License and Contact

nanjav Game Engine — A simple and lightweight engine for learning and rapid prototyping of 2D games.