using Enums;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;

public struct QuadrantEntity : IComponentData {
}

public struct QuadrantData {
    public Entity entity;
    public float3 position;
    public CollisionType collisionType;
    public PhysicsCollider collider;
    public QuadrantEntity quadrantEntity;
}

public class QuadrantSystem : ComponentSystem {

    public static NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;

    public const int quadrantYMultiplier = 1000;
    private const int quadrantCellSize = 5;

    public static int GetPositionHashMapKey(float3 position) {
        return (int) (math.floor(position.x / quadrantCellSize) + (quadrantYMultiplier * math.floor(position.z / quadrantCellSize)));
    }

    [BurstCompile]
    [RequireComponentTag(typeof(QuadrantEntity))]
    private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<Translation, Collideable, PhysicsCollider>
    {
        public NativeMultiHashMap<int, QuadrantData>.Concurrent QuadrantMultiHashMap;

        public void Execute(Entity entity,
            int index,
            ref Translation translation,
            ref Collideable col,
            ref PhysicsCollider collider)
        {
            int hashMapKey = GetPositionHashMapKey(translation.Value);
            QuadrantMultiHashMap.Add(hashMapKey, new QuadrantData
            {
                entity = entity,
                position = translation.Value,
                collisionType = col.Type,
                collider = collider
            });
        }
    }

    protected override void OnCreate() {
        quadrantMultiHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
        base.OnCreate();
    }

    protected override void OnDestroy() {
        quadrantMultiHashMap.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate() {
        var entityQuery = GetEntityQuery(
            typeof(Translation), 
            typeof(Collideable), 
            typeof(PhysicsCollider), 
            typeof(QuadrantEntity), 
            ComponentType.Exclude<DeadData>());

        quadrantMultiHashMap.Clear();
        if (entityQuery.CalculateEntityCount() > quadrantMultiHashMap.Capacity) {
            quadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount();
        }

        var setQuadrantDataHashMapJob = new SetQuadrantDataHashMapJob {
            QuadrantMultiHashMap = quadrantMultiHashMap.ToConcurrent(),
        };
        var jobHandle = setQuadrantDataHashMapJob.Schedule(entityQuery);
        jobHandle.Complete();
    }

}
