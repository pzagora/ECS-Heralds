using System.Collections.Generic;
using PlasticApps;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ZoneDamagingSystem : ComponentSystem
{
    private List<Entity> _TweenRecord;
    private EntityQuery _DamagableQuery;
    private EntityQuery _ZoneQuery;
    
    private EntityManager _EntityManager;
    private EntityArchetype _AttackArchetype;
    private Entity _AttackEntity;

    protected override void OnCreate()
    {
        _TweenRecord = new List<Entity>();
        _EntityManager = World.Active.EntityManager;
        _AttackArchetype = _EntityManager.CreateArchetype(typeof(AttackData));

        _DamagableQuery = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<HealthData>(),
            ComponentType.Exclude<DeadData>()
            );
        _ZoneQuery = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<NonUniformScale>(),
            ComponentType.ReadOnly<ZoneData>());
    }

    protected override void OnUpdate()
    {
        Entity zoneEntity = default;
        float3 zonePosition = Vector3.zero;
        var zoneDistance = 0f;

        Entities.With(_ZoneQuery).ForEach(
            (Entity entity, ref Translation translation, ref NonUniformScale scale) =>
            {
                zoneEntity = entity;
                zonePosition = translation.Value;
                zoneDistance = scale.Value.x;
            });
        

        Entities.With(_DamagableQuery).ForEach(
            (Entity entity, ref Translation translation, ref HealthData hp) =>
            {
                var position = translation.Value;
                var heading = (Vector3) (zonePosition - position);
                var magnitude = heading.magnitude;
                
                if (!_TweenRecord.Contains(entity) && magnitude > zoneDistance / 2)
                {
                    _TweenRecord.Add(entity);
                    _AttackEntity = _EntityManager.CreateEntity(_AttackArchetype);
                    _EntityManager.SetComponentData(_AttackEntity, new AttackData
                    {
                        Damage = HeraldsBootstrap.Settings.ZoneDamagePerTick,
                        Source = zoneEntity,
                        Target = entity
                    });
                    Tween.Delay(HeraldsBootstrap.Settings.TimePerTick, () => { _TweenRecord.Remove(entity); });
                }
            });
    }
}
