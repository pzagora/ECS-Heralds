using Enums;
using Unity.Entities;

public struct ItemChange : IComponentData
{
    public Entity Target;
    public FirstUpgrade FirstUpgrade;
    public SecondUpgrade SecondUpgrade;
}
