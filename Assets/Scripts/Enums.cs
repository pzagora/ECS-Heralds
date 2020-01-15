namespace Enums
{
    public enum CreatureType
    {
        Player,
        NPC,
        AI,
        HostileAI
    }

    public enum ColliderType
    {
        Box,
        Sphere,
        Capsule
    }

    public enum CollisionType
    {
        Walkable,
        NonWalkable
    }

    public enum FirstUpgrade
    {
        None,
        AttackSpeedMin,
        AttackSpeedMed,
        AttackSpeedMax,
        DamageMin,
        DamageMed,
        DamageMax
        
    }

    public enum SecondUpgrade
    {
        None,
        MovementSpeedMin,
        MovementSpeedMed,
        MovementSpeedMax,
        ResistanceMin,
        ResistanceMed,
        ResistanceMax
    }

    public enum GroundLevel
    {
        Water,
        Sand,
        Grass,
        Mountain
    }
}