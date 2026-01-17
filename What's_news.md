# **What's news?**

## **0.1.1**
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


