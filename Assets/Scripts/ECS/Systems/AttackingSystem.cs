using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class AttackingSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem barrier;

    protected override void OnCreate()
    {
        barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    private struct AttackJob : IJobForEachWithEntity<AttackData>
    {
        public EntityCommandBuffer.Concurrent Ecb;

        [ReadOnly] public ComponentDataFromEntity<HealthData> Health;
        [ReadOnly] public ComponentDataFromEntity<DamagedData> Damaged;

        public void Execute(Entity entity, int index, ref AttackData attackData)
        {
            if (World.Active.EntityManager.Exists(entity))
            {
                var target = attackData.Target;
                var source = attackData.Source;
                if (World.Active.EntityManager.Exists(target) && World.Active.EntityManager.Exists(source) && Health.Exists(target))
                {
                    if (Health[target].Value > 0 && !Damaged.Exists(target))
                    {
                        Ecb.AddComponent(index, target, new DamagedData {Damage = attackData.Damage, Source = source});
                    }
                }
            }
            Ecb.RemoveComponent(index, entity, typeof(AttackData));
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new AttackJob
        {
            Ecb = barrier.CreateCommandBuffer().ToConcurrent(),
            Health = GetComponentDataFromEntity<HealthData>(),
            Damaged = GetComponentDataFromEntity<DamagedData>()
            
        };
        inputDeps = job.Schedule(this, inputDeps);
        inputDeps.Complete();
        barrier.AddJobHandleForProducer(inputDeps);
        return inputDeps;
    }
}
