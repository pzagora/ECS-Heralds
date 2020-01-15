using System;
using System.Collections.Generic;
using PlasticApps;
using PlasticApps.Components.Ease;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyMovementSystem : ComponentSystem
{ 
    private List<Entity> _TweenRecord;
    
    private EntityQuery enemyQuery;
    private EntityQuery playerQuery;
    private EntityQuery zoneQuery;

    private List<Vector3> _MovementVectors;

    protected override void OnCreate()
    {
        _TweenRecord = new List<Entity>();
        _MovementVectors = new List<Vector3>()
        {
            new Vector3(-1, 0, -1), // Bottom left
            new Vector3(0, 0, -1), // Bottom
            new Vector3(1, 0, -1), // Bottom Right
            new Vector3(-1, 0, 0), // Left
            new Vector3(1, 0, 0), // Right
            new Vector3(-1, 0, 1), // Top Left
            new Vector3(0, 0, 1), // Top
            new Vector3(1, 0, 1), // Top Right
        };
        
        enemyQuery = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<Rigidbody>(),
            ComponentType.ReadOnly<EnemyData>(),
            ComponentType.ReadOnly<HealthData>(),
            ComponentType.ReadOnly<CollisionData>(),
            ComponentType.Exclude<DeadData>());
        
        playerQuery = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<PlayerData>(),
            ComponentType.ReadOnly<HealthData>());

        zoneQuery = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<NonUniformScale>(),
            ComponentType.ReadOnly<ZoneData>());
    }

    protected override void OnUpdate()
    {
        float3 closestEntityPosition = Vector3.negativeInfinity;
        var entityPositions = new Dictionary<Entity, float3>();
        var playerHp = 0;

        Entities.With(enemyQuery).ForEach(
            (Entity entity, ref Translation translation, ref HealthData hp) =>
            {
                entityPositions.Add(entity, translation.Value);
                playerHp = hp.Value;
            });
        
        Entities.With(playerQuery).ForEach(
            (Entity entity, ref Translation translation, ref HealthData hp) =>
            {
                entityPositions.Add(entity, translation.Value);
                playerHp = hp.Value;
            });

        if (playerHp <= 0)
        {
            return;
        }

        float3 zonePosition = Vector3.zero;
        var zoneDistance = 0f;

        Entities.With(zoneQuery).ForEach(
            (Entity entity, ref Translation translation, ref NonUniformScale scale) =>
            {
                zonePosition = translation.Value;
                zoneDistance = scale.Value.x;
            });

        var timeToMove = HeraldsBootstrap.Settings.PlayerMoveSpeed;

        Entities.With(enemyQuery).ForEach(
            (Entity entity, Rigidbody rigidBody, ref Translation translation, ref HealthData hp, ref CollisionData collisionData) =>
            {
                if (hp.Value > 0)
                {
                    var position = translation.Value;
                    Vector3 heading = default;
                    float magnitude;

                    foreach (var entityPosition in entityPositions)
                    {
                        if (entityPosition.Key != entity)
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
                        rigidBody.transform.LookAt(closestEntityPosition);
                    }
                    else
                    {
                        rigidBody.transform.LookAt(zonePosition);
                    }

                    if (!_TweenRecord.Contains(entity) && hp.Value > 0)
                    {
                        bool wasSorted;

                        if (magnitude < 5)
                        {
                            if (magnitude > 1.5f)
                            {
                                wasSorted = true;
                                _MovementVectors.Sort((v1, v2) =>
                                    (v1 + (Vector3) position - (Vector3) closestEntityPosition).sqrMagnitude.CompareTo(
                                        (v2 + (Vector3) position - (Vector3) closestEntityPosition).sqrMagnitude));
                            }
                            else
                            {
                                wasSorted = Random.Range(0, 100) > 90;
                            }
                        }
                        else
                        {
                            wasSorted = true;
                            _MovementVectors.Sort((v1, v2) =>
                                (v1 + (Vector3) position - (Vector3) zonePosition).sqrMagnitude.CompareTo(
                                    (v2 + (Vector3) position - (Vector3) zonePosition).sqrMagnitude));
                        }

                        if (wasSorted)
                        {
                            Vector2 move;
                            Vector3 movement;
                            Vector3 newPos = default;
                            var condition = false;

                            for (var i = 0; i < _MovementVectors.Count; i++)
                            {
                                condition = false;
                                move = new Vector2(_MovementVectors[i].x, _MovementVectors[i].z);
                                movement = new Vector3(Mathf.RoundToInt(move.x), 0, Mathf.RoundToInt(move.y));
                                newPos = new Vector3(Mathf.RoundToInt(position.x), 0f, Mathf.RoundToInt(position.z)) +
                                         movement;

                                unsafe
                                {
                                    condition = collisionData.CollisionMatrix[
                                        Mathf.RoundToInt(move.x) + 1 + (-Mathf.RoundToInt(move.y) + 1) * 3];
                                }

                                if (!condition)
                                {
                                    break;
                                }
                            }

                            EntityManager.SetComponentData(entity, new CollisionData());

                            if (!condition)
                            {
                                EntityManager.SetComponentData(entity, new Translation {Value = newPos});

                                _TweenRecord.Add(entity);
                                Tween.MoveGameObject(rigidBody.gameObject, timeToMove, newPos, EaseType.easeInOutQuint)
                                    .OnTweenComplete(() =>
                                    {
                                        Tween.Delay(
                                            magnitude > 4 
                                                ? Random.Range(HeraldsBootstrap.Settings.EnemyMovePauseOutsideRange.x, HeraldsBootstrap.Settings.EnemyMovePauseOutsideRange.y)
                                                : Random.Range(HeraldsBootstrap.Settings.EnemyMinMovePause, HeraldsBootstrap.Settings.EnemyMaxMovePause),
                                            () => { _TweenRecord.Remove(entity); });
                                    });
                            }
                        }
                    }
                }
            });
    }
}
