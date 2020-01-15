using System.Collections.Generic;
using PlasticApps;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class EnemyShootingSystem : ComponentSystem
{ 
    private List<Entity> _TweenRecord;
    
    private EntityQuery _EnemyQuery;
    private EntityQuery _PlayerQuery;

    private const float MaxShootOffset = 0.8f;

    protected override void OnCreate()
    {
        _TweenRecord = new List<Entity>();
        
        _EnemyQuery = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<EnemyData>(),
            ComponentType.ReadOnly<HealthData>(),
            ComponentType.ReadOnly<CollisionData>(),
            ComponentType.Exclude<DeadData>());
        
        _PlayerQuery = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<PlayerData>(),
            ComponentType.ReadOnly<HealthData>());
    }

    protected override void OnUpdate()
    {
        float3 closestEntityPosition = Vector3.negativeInfinity;
        var entityPositions = new Dictionary<Entity, float3>();
        var playerHp = 0;

        Entities.With(_EnemyQuery).ForEach(
            (Entity entity, ref Translation translation, ref HealthData hp) =>
            {
                entityPositions.Add(entity, translation.Value);
                playerHp = hp.Value;
            });
        
        Entities.With(_PlayerQuery).ForEach(
            (Entity entity, ref Translation translation, ref HealthData hp) =>
            {
                entityPositions.Add(entity, translation.Value);
                playerHp = hp.Value;
            });

        if (playerHp <= 0)
        {
            return;
        }

        Entities.With(_EnemyQuery).ForEach(
            (Entity entity, ref Translation translation, ref HealthData hp) =>
            {
                if (hp.Value > 0 && !_TweenRecord.Contains(entity))
                {
                    var position = translation.Value;
                    Vector3 heading = default;
                    float magnitude;

                    foreach (var entityPosition in entityPositions)
                    {
                        if (entityPosition.Key.Index != entity.Index)
                        {
                            var currentEntityMagnitude = ((Vector3)(entityPosition.Value - position)).magnitude;
                            heading = closestEntityPosition - position;
                            magnitude = heading.magnitude;
                        
                            if (currentEntityMagnitude < magnitude)
                            {
                                closestEntityPosition = entityPosition.Value;
                            }
                        }
                    }
                    
                    heading = closestEntityPosition - position;
                    magnitude = heading.magnitude;

                    if (magnitude < 5)
                    {
                        _TweenRecord.Add(entity);
                        var damageObject = Object.Instantiate(HeraldsBootstrap.Settings.DamagePrefab,
                            closestEntityPosition + new float3(
                                Random.Range(-MaxShootOffset, MaxShootOffset), 
                                0,
                                Random.Range(-MaxShootOffset, MaxShootOffset)), Quaternion.identity);
                        damageObject.GetComponent<Attacker>().SetSource(entity);
                        Tween.Delay(
                            Random.Range(HeraldsBootstrap.Settings.EnemyMinShootingDelay, HeraldsBootstrap.Settings.EnemyMaxShootingDelay),
                            () => { _TweenRecord.Remove(entity); });
                    }
                }
            });
    }
}
