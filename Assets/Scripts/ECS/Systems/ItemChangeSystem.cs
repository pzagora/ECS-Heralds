using Enums;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class ItemChangeSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem _Barrier;

    protected override void OnCreate()
    {
        _Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    private struct ItemChangeJob : IJobForEachWithEntity<ItemChange>
    {
        public EntityCommandBuffer.Concurrent Ecb;

        [ReadOnly] public ComponentDataFromEntity<PlayerData> GunData;
        
        public void Execute(Entity entity, int index, ref ItemChange itemChange)
        {
            var target = itemChange.Target;
            
            if (itemChange.FirstUpgrade == FirstUpgrade.None)
            {
                Ecb.SetComponent(index, target, new PlayerData{FirstUpgrade = GunData[target].FirstUpgrade, SecondUpgrade = itemChange.SecondUpgrade});
            }
            else if (itemChange.SecondUpgrade == SecondUpgrade.None)
            {
                Ecb.SetComponent(index, target, new PlayerData{FirstUpgrade = itemChange.FirstUpgrade, SecondUpgrade = GunData[target].SecondUpgrade});
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ItemChangeJob
        {
            Ecb = _Barrier.CreateCommandBuffer().ToConcurrent(),
            GunData = GetComponentDataFromEntity<PlayerData>()
        };
        inputDeps = job.Schedule(this, inputDeps);
        inputDeps.Complete();
        _Barrier.AddJobHandleForProducer(inputDeps);
        return inputDeps;
    }
}
