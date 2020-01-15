using System.Collections.Generic;
using PlasticApps;
using PlasticApps.Components.Ease;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

class CleanupSystem : ComponentSystem
{
    private EntityQuery _DeadObjects;

    protected override void OnCreate()
    {
        _DeadObjects = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<DeadData>(),
            ComponentType.ReadOnly<Rigidbody>(),
            ComponentType.Exclude<PlayerData>(),
            ComponentType.Exclude<SinkedData>()
        );
    }

    protected override void OnUpdate()
    {
        Entities.With(_DeadObjects).ForEach(
            (Entity entity, ref Translation translation, Rigidbody rigidBody) =>
            {
                PostUpdateCommands.AddComponent(entity, new SinkedData());
                var newPosition = (Vector3) translation.Value + new Vector3(0, -5, 0);
                Tween.MoveGameObject(rigidBody.gameObject, 5f, newPosition);
                Tween.Delay(2f,
                    () =>
                    {
                        EntityManager.SetComponentData(entity,
                            new Translation {Value = new float3(9999f, 0, 9999f)});
                    });
            });
    }
}