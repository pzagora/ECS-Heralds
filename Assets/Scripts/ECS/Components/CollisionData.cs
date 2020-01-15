using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public unsafe struct CollisionData : IComponentData
{
    public fixed bool CollisionMatrix[9];
}