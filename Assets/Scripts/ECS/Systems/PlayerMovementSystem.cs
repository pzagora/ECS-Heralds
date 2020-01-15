using System;
using System.Collections.Generic;
using System.Diagnostics;
using Enums;
using PlasticApps;
using PlasticApps.Components.Ease;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

[DisableAutoCreation]
public class PlayerMovementSystem : ComponentSystem
{
    private List<Entity> _TweenRecord;
    private EntityQuery _PlayerQuery;

    protected override void OnCreate()
    {
        _TweenRecord = new List<Entity>();

        _PlayerQuery = GetEntityQuery(
            ComponentType.ReadOnly<Transform>(),
            ComponentType.ReadOnly<PlayerInputData>(),
            ComponentType.ReadOnly<Rigidbody>(),
            ComponentType.ReadOnly<CollisionData>(),
            ComponentType.ReadOnly<PlayerData>(),
            ComponentType.Exclude<DeadData>());
    }

    protected override void OnUpdate()
    {
        var timeToMove = HeraldsBootstrap.Settings.PlayerMoveSpeed;

        Entities.With(_PlayerQuery).ForEach(
            (Entity entity, Rigidbody rigidBody, ref PlayerData playerData, ref PlayerInputData input, ref CollisionData collisionData) =>
            {
                if (!_TweenRecord.Contains(entity))
                {
                    var move = input.Move;
                    var movement = new Vector3(Mathf.RoundToInt(move.x), 0, Mathf.RoundToInt(move.y));
                    var position = rigidBody.gameObject.transform.position;
                    var newPos = new Vector3(Mathf.RoundToInt(position.x), 0f, Mathf.RoundToInt(position.z)) + movement;

                    var condition = false;

                    unsafe
                    {
                        condition = collisionData.CollisionMatrix[
                            Mathf.RoundToInt(move.x) + 1 + (-Mathf.RoundToInt(move.y) + 1) * 3];
                    }

                    if (!condition)
                    {
                        EntityManager.SetComponentData(entity, new Translation {Value = newPos});
                        EntityManager.SetComponentData(entity, new CollisionData());

                        _TweenRecord.Add(entity);
                        Tween.MoveGameObject(rigidBody.gameObject, CalculateTimeToMove(playerData.SecondUpgrade, timeToMove), newPos, EaseType.easeInOutQuint)
                            .OnTweenComplete(() => { _TweenRecord.Remove(entity); });
                    }
                }
            });
    }
    
    private static float CalculateTimeToMove(SecondUpgrade secondUpgrade, float movementSpeed)
    {
        var baseMovementSpeed = movementSpeed;

        switch (secondUpgrade)
        {
            case SecondUpgrade.MovementSpeedMin:
                baseMovementSpeed -= baseMovementSpeed / 4f;
                break;
            case SecondUpgrade.MovementSpeedMed:
                baseMovementSpeed -= baseMovementSpeed / 2f;
                break;
            case SecondUpgrade.MovementSpeedMax:
                baseMovementSpeed = baseMovementSpeed / 4f;
                break;
        }
        
        return baseMovementSpeed;
    }
}
