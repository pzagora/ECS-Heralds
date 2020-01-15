using Enums;
using Unity.Entities;

public struct PhysicsCollider : IComponentData
{
    public ColliderType Type;
    public float Size;
    public float Height; // only used in sphere colliders
}