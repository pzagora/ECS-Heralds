using Unity.Entities;

public struct PlayerData : IComponentData
{
    public Enums.FirstUpgrade FirstUpgrade;
    public Enums.SecondUpgrade SecondUpgrade;
}
