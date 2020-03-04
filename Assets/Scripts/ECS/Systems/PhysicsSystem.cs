using System;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;

/*public class PhysicsSystem : ComponentSystem
{
    private EntityQuery _MovingQuery;
    private EntityQuery _ColliderQuery;

    // Here we define the group 
    protected override void OnCreateManager()
    {
        // Get all moving objects
        _MovingQuery = GetEntityQuery(
            ComponentType.ReadOnly<PhysicsCollider>(),
            ComponentType.ReadOnly<Transform>(),
            typeof(CollisionData));

        // Get all colliders
        _ColliderQuery = GetEntityQuery(
            ComponentType.ReadOnly<PhysicsCollider>(),
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<GroundType>()
        );
    }

    protected override void OnUpdate()
    {
        // Get elapsed time
        float dt = UnityEngine.Time.deltaTime;

        Entities.With(_MovingQuery).ForEach(
            (Entity entity, Transform translation, ref PhysicsCollider collider,
                ref CollisionData collisionData) =>
            {

                var movingTranslation = translation;
                var movingCollider = collider;
                var movingCollisionData = collisionData;
                
                Entities.With(_ColliderQuery).ForEach(
                    (Entity otherEntity, ref Translation otherTranslation, ref PhysicsCollider otherCollider,
                        ref GroundType groundType) =>
                    {
                        if (entity == otherEntity ||
                            groundType.Type == Enums.GroundType.Walkable ||
                            Vector2.Distance(new Vector2(movingTranslation.position.x, movingTranslation.position.z), new Vector2(otherTranslation.Value.x, otherTranslation.Value.z)) >
                            movingCollider.Size / 2 + otherCollider.Size / 2 + 0.2f)
                        {
                            return;
                        }

                        var position = movingTranslation.position;
                        var x = math.abs(position.x - otherTranslation.Value.x);
                        var z = math.abs(position.z - otherTranslation.Value.z);

                        if (x < z)
                        {
                            if (movingTranslation.position.x < otherTranslation.Value.x)
                            {
                                movingCollisionData.Down = true;
                            }
                            else
                            {
                                movingCollisionData.Up = true;
                            }
                        }
                        else if (z < x)
                        {
                            if (movingTranslation.position.z < otherTranslation.Value.z)
                            {
                                movingCollisionData.Left = true;
                            }
                            else
                            {
                                movingCollisionData.Right = true;
                            }

                        }

                        EntityManager.SetComponentData(entity, movingCollisionData);
                    });

            });
    }
}*/

public class PhysicsJobSystem : JobComponentSystem
{
    [BurstCompile]
    private struct PhysicsQuadrantJob : IJobForEachWithEntity<Translation, PhysicsCollider, CollisionData>
    {
        [ReadOnly] public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;

        public NativeArray<CollisionData> collisionDatas;

        public void Execute(Entity entity, int index, ref Translation translation, ref PhysicsCollider collider, ref CollisionData collisionData)
        {
            int hashMapKey = QuadrantSystem.GetPositionHashMapKey(translation.Value);

            CheckCollisions(hashMapKey, index, ref entity, ref translation, ref collider, ref collisionData);
            CheckCollisions(hashMapKey + 1, index, ref entity, ref translation, ref collider, ref collisionData);
            CheckCollisions(hashMapKey - 1, index, ref entity, ref translation, ref collider, ref collisionData);
            CheckCollisions(hashMapKey + QuadrantSystem.quadrantYMultiplier, index, ref entity, ref translation, ref collider, ref collisionData);
            CheckCollisions(hashMapKey - QuadrantSystem.quadrantYMultiplier, index, ref entity, ref translation, ref collider, ref collisionData);
            CheckCollisions(hashMapKey + 1 + QuadrantSystem.quadrantYMultiplier, index, ref entity, ref translation, ref collider, ref collisionData);
            CheckCollisions(hashMapKey - 1 + QuadrantSystem.quadrantYMultiplier, index, ref entity, ref translation, ref collider, ref collisionData);
            CheckCollisions(hashMapKey + 1 - QuadrantSystem.quadrantYMultiplier, index, ref entity, ref translation, ref collider, ref collisionData);
            CheckCollisions(hashMapKey - 1 - QuadrantSystem.quadrantYMultiplier, index, ref entity, ref translation, ref collider, ref collisionData);
        }

        private void CheckCollisions(int hashMapKey, int index, ref Entity entity, ref Translation movingTranslation,
            ref PhysicsCollider movingCollider, ref CollisionData cData)
        {
            if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out var quadrantData, out var nativeMultiHashMapIterator))
            {
                do
                {
                    var isTheSameEntity = entity == quadrantData.entity;
                    var isWalkable = quadrantData.collisionType == Enums.CollisionType.Walkable;
                    var isInRadius =
                        Vector2.Distance(new Vector2(movingTranslation.Value.x, movingTranslation.Value.z),
                            new Vector2(quadrantData.position.x, quadrantData.position.z)) <
                        movingCollider.Size / 2 + quadrantData.collider.Size / 2 + 0.5f;

                    if (!isTheSameEntity && !isWalkable && isInRadius)
                    {
                        Vector3 heading = quadrantData.position - movingTranslation.Value;
                        var distance = heading.magnitude;
                        var direction = heading / distance;

                        unsafe
                        {
                            if (direction.x == 0)
                            {
                                if (direction.z > 0)
                                {
                                    cData.CollisionMatrix[1] = true;
                                }
                                else
                                {
                                    cData.CollisionMatrix[7] = true;
                                }
                            }
                            else if (direction.z == 0)
                            {
                                if (direction.x < 0)
                                {
                                    cData.CollisionMatrix[3] = true;
                                }
                                else
                                {
                                    cData.CollisionMatrix[5] = true;
                                }
                            }
                            else
                            {
                                if (direction.x < 0 && direction.z > 0)
                                {
                                    cData.CollisionMatrix[0] = true;
                                }
                                else if (direction.x > 0 && direction.z > 0)
                                {
                                    cData.CollisionMatrix[2] = true;
                                }
                                else if (direction.x < 0 && direction.z < 0)
                                {
                                    cData.CollisionMatrix[6] = true;
                                }
                                else if (direction.x > 0 && direction.z < 0)
                                {
                                    cData.CollisionMatrix[8] = true;
                                }
                            }


                        }

                        collisionDatas[index] = cData;
                    }
                } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
            }
        }
    }

    private struct SetComponentJob : IJobForEachWithEntity<Translation, CollisionData>
    {
        [DeallocateOnJobCompletion, ReadOnly] public NativeArray<CollisionData> collisionDatas; 
        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        public void Execute(Entity entity, int index, ref Translation translation, ref CollisionData collision)
        {
            entityCommandBuffer.SetComponent(index, entity, collisionDatas[index]);
        }
    }

    private EndSimulationEntityCommandBufferSystem _EndSimulationEntityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        _EndSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var unitQuery = GetEntityQuery(
            typeof(CollisionData),
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<PhysicsCollider>()
            );
        var  collisionData = new NativeArray<CollisionData>(
            unitQuery.CalculateEntityCount(),
            Allocator.TempJob
            );

        var physicsQuadrantJob = new PhysicsQuadrantJob
        {
            quadrantMultiHashMap = QuadrantSystem.quadrantMultiHashMap,
            collisionDatas = collisionData
        };
        var jobHandle = physicsQuadrantJob.Schedule(this, inputDeps);
        var setComponentJob = new SetComponentJob
        {
            collisionDatas = collisionData,
            entityCommandBuffer = _EndSimulationEntityCommandBufferSystem
                .CreateCommandBuffer()
                .ToConcurrent()
        };
        jobHandle = setComponentJob.Schedule(this, jobHandle);
        _EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}