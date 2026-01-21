# **What's news?**

## **0.1.3**
### MOST IMPORTANT NEW FEATURES:
1. FULL AUDIO SYSTEM (HUGE STEP!)
2. UTILITIES FOR BETTER CODE ORGANIZATION
3. IMPROVED DATA SYSTEM
### New features:
#### Full-featured audio system (COMPLETE NEW)
- `AudioSystem.cs` - OpenAL integration
- `AudioSource.cs` - Component for GameObject
#### Code Utilities (Refactoring)
- `ShaderUtils.cs` - Centralized Shader Work
- `MathUtils.cs` - Mathematical functions
#### Improved diagnostics system
- `Data.cs` - Extended system information
```cs
public class Data
{
    public string Version = "0.1.3"; // Engine version
    public string OSver = Environment.OSVersion.ToString(); // Operating system
    public string TimeNow = DateTime.Now.ToString(); // Current time

    // OpenGL information
    public string GLVersion { get; }
    public string GLVendor { get; }
    public string GLRenderer { get; }
    public string GLSLVersion { get; }

    public Data(GL gl) // Constructor with GL context
    {
    // Automatically collects graphics information
    }
}
```
#### CRITICAL CODE CHANGES
`Renderer.cs` - refactoring:
```cs
- // Old code (0.1.2): everything in one class
- _vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
- _fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);
- _program = _gl.CreateProgram();
- // ... a lot of shader code

+ // New code (0.1.3): using utilities
+ _program = ShaderUtils.CreateProgram(_gl, vertexSource, fragmentSource,
+ out _vertexShader, out _fragmentShader);
```
```cs
- // Old code (0.1.2): manual cleanup
- if (_vertexShader != 0) _gl.DeleteShader(_vertexShader);
- if (_fragmentShader != 0) _gl.DeleteShader(_fragmentShader);

+ // New code (0.1.3): automatic cleanup
+ ShaderUtils.DeleteProgram(_gl, ref _program, ref _vertexShader, ref _fragmentShader);
```
```cs
- // Old code (0.1.2): manual cleanup
- if (_vertexShader != 0) _gl.DeleteShader(_vertexShader);
- if (_fragmentShader != 0) _gl.DeleteShader(_fragmentShader);

+ // New code (0.1.3): automatic cleanup
+ ShaderUtils.DeleteProgram(_gl, ref _program, ref _vertexShader, ref _fragmentShader);
```

### Fixed/improved in 0.1.3:

1. Renderer refactoring - less code, better organization

2. Automatic shader cleanup

3. Centralized math and shader utilities

4. Audio error handling

## **0.1.2**
### New features:

updated documentation to 0.1.2

Object tracking camera

Dynamic zoom during gameplay

One-way jump system

Apply forces in any direction
- ### NEW FEATURES IN 0.1.2:
- Camera System (ALL NEW)
  
- Improved physics system
  
- Improved SpriteRenderer

- ### CRITICAL CODE CHANGES:
- `Renderer.cs` - vertex collection:
```cs
- // Old code (0.1.1): without camera
- float x = obj.Transform.X;
- float y = obj.Transform.Y;

+ // New code (0.1.2): with camera
+ float cameraX = Camera?.Position.X ?? 0f;
+ float cameraY = Camera?.Position.Y ?? 0f;
+ float x = obj.Transform.X - cameraX;
+ float y = obj.Transform.Y - cameraY;
```
- `GameObject.cs` - Transform:
```cs
- // Old code (0.1.1): without readonly
- public Transform Transform { get; private set; }

+ // New code (0.1.2): with readonly
+ public Transform Transform { get; private set; } // Added readonly
```
- ### FIXES AND IMPROVEMENTS
Fixed in 0.1.2:

1. Transform is now get; private set; (safer)

2. Rigidbody2D has full collisions (X and Y)

3. SpriteRenderer supports camera

## **0.1.1**
- ### NEW FEATURES IN 0.1.1:
- ### New folder structure:
```
nanjav-engine/
├── src/             All source files moved here
│ ├── core/          Core engine classes
│ ├── functions/     Function modules
│ │ ├── gameObject/  Object system
│ │ └── Physics/     NEW: Physics
│ ├── input/         Input
│ └── Data.cs        Engine version
└── README.md        Documentation
```
- ### 2D Physical System

- #### BoxCollider2D (new class)

- Implements a collision system for rectangles

- Automatically registers all colliders in `AllColliders`

- methods:
```cs
bool IsColliding(BoxCollider2D other) // Collision check 
float Left/Right/Top/Bottom           // Collider boundaries
```

- #### Rigidbody2D (new class)

- Implements body physics with gravity

- Properties:
```csharp
float Mass = 1.0f
float GravityScale = 9.7f
Vector2 Velocity = Vector2.Zero
```

- Automatically handles collisions with other BoxCollider2D

- Grounded detection

- ### `Data` class for versioning:
```cs
public class Data
{
    public string version = "0.1.1";  //updated version
}
```
- ### Improved input system:
- Keyboard: `Keyboard` now belongs to `nanjav.input` (without `_engine`)
- Mouse: Similarly, `Mouse` belongs to `nanjav.input`

- ### Technical improvements:
- #### Clearer code organization:

- All files are structured according to their purpose

- Physics is moved to a separate module

- Correct namespaces for all classes

- #### Normalized file paths:

- All files have consistent namespaces

- Fixed import inconsistencies

#### Updated documentation:

- Detailed API documentation

- Usage examples




## **0.1.0**
first version of the engine


