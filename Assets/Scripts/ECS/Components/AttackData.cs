using Unity.Entities;

public struct AttackData : IComponentData
{
    public int Damage;
    public Entity Source;
    public Entity Target;
}
