namespace nanjav.core;

public class GameObject
{
    public string Name { get; set; } = "GameObject";
    public bool IsActive { get; private set; } = true;
    public Transform Transform { get; private set; }
    private List<Component> _components = new List<Component>();
    private List<GameObject> _children = new List<GameObject>();
    public GameObject? Parent { get; private set; }

    public GameObject(string name = "GameObject")
    {
        Name = name;
        Transform = new Transform();
    }

    public static GameObject Create(string name = "GameObject")
    {
        return new GameObject(name);
    }

    public T AddComponent<T>() where T : Component, new()
    {
        var component = new T();
        component.GameObject = this;
        _components.Add(component);
        return component;
    }

    public T? GetComponent<T>() where T : Component
    {
        return _components.Find(c => c is T) as T;
    }

    public void AddChild(GameObject child)
    {
        if (child.Parent != null)
        {
            child.Parent.RemoveChild(child);
        }
        child.Parent = this;
        _children.Add(child);
        child.Transform.SetParent(this.Transform);
    }

    public void RemoveChild(GameObject child)
    {
        if (_children.Remove(child))
        {
            child.Parent = null;
            child.Transform.SetParent(null);
        }
    }

    public IReadOnlyList<GameObject> GetChildren()
    {
        return _children.AsReadOnly();
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        foreach (var child in _children)
        {
            child.SetActive(active);
        }
    }

    public void Update(double deltaTime)
    {
        if (!IsActive) return;
        foreach (var component in _components)
        {
            component.Update(deltaTime);
        }
        foreach (var child in _children)
        {
            child.Update(deltaTime);
        }
    }

    public void Render()
    {
        if (!IsActive) return;
        foreach (var component in _components)
        {
            component.Render();
        }
        foreach (var child in _children)
        {
            child.Render();
        }
    }
}

public abstract class Component
{
    public GameObject? GameObject { get; set; }
    public Transform? Transform => GameObject?.Transform;

    public virtual void Update(double deltaTime) { }
    public virtual void Render() { }
}

public class SpriteRenderer : Component
{
    public float Width { get; set; } = 100f;
    public float Height { get; set; } = 100f;
    public System.Numerics.Vector3 Color { get; set; } = new System.Numerics.Vector3(1f, 1f, 1f);

    public string TexturePath { get; set; } = "";

    public SpriteRenderer() { }

    public SpriteRenderer(float width, float height)
    {
        Width = width;
        Height = height;
    }

    public SpriteRenderer(float width, float height, string texturePath)
    {
        Width = width;
        Height = height;
        TexturePath = texturePath;
    }

    public SpriteRenderer(float width, float height, System.Numerics.Vector3 color)
    {
        Width = width;
        Height = height;
        Color = color;
    }

    public SpriteRenderer(float width, float height, System.Numerics.Vector3 color, string texturePath)
    {
        Width = width;
        Height = height;
        Color = color;
        TexturePath = texturePath;
    }

    public void Render(Camera2D camera)
    {
        if (Transform == null) return;

        float screenX = (Transform.X - camera.Position.X) + camera.Offset.X;
        float screenY = (Transform.Y - camera.Position.Y) + camera.Offset.Y;
    }
}

public class Transform
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Rotation { get; set; }
    public float ScaleX { get; set; } = 1f;
    public float ScaleY { get; set; } = 1f;

    public Transform? Parent { get; private set; }
    private List<Transform> _children = new List<Transform>();

    public Transform()
    {
        X = 0; Y = 0; Rotation = 0;
    }

    public void SetParent(Transform? newParent)
    {
        if (Parent != null)
        {
            Parent._children.Remove(this);
        }

        Parent = newParent;

        if (Parent != null)
        {
            Parent._children.Add(this);
        }
    }
}