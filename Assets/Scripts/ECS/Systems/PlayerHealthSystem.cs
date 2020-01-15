using Enums;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class PlayerHealthSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem _Barrier;
    private EntityArchetype _HealthUpdatedArchetype;

    protected override void OnCreate()
    {
        _Barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        _HealthUpdatedArchetype = EntityManager.CreateArchetype(typeof(HealthUpdatedData));
    }

    private struct PlayerHealthJob : IJobForEachWithEntity<PlayerData, HealthData, DamagedData>
    {
        public EntityCommandBuffer.Concurrent Ecb;

        public EntityArchetype HealthUpdatedArchetype;
        [ReadOnly] public ComponentDataFromEntity<DeadData> Dead;

        public void Execute(
            Entity entity,
            int index,
            [ReadOnly] ref PlayerData playerData,
            ref HealthData healthData,
            ref DamagedData damagedData)
        {
            healthData.Value -= CalculateDamage(playerData.SecondUpgrade, damagedData.Damage);
            Ecb.RemoveComponent<DamagedData>(index, entity);
            var e = Ecb.CreateEntity(index, HealthUpdatedArchetype);
            Ecb.SetComponent(index, e, new HealthUpdatedData {Health = healthData.Value});
            if (healthData.Value <= 0 && !Dead.Exists(entity))
            {
                Ecb.AddComponent(index, entity, new DeadData());
                var killedName = World.Active.EntityManager.GetComponentData<NameData>(entity);
                var killerName = World.Active.EntityManager.GetComponentData<NameData>(damagedData.Source);
                HeraldsBootstrap.Settings.GameUi.LogKill(killerName.Value, killedName.Value);
                HeraldsBootstrap.Settings.GameUi.OnPlayerKilled();
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new PlayerHealthJob
        {
            Ecb = _Barrier.CreateCommandBuffer().ToConcurrent(),
            HealthUpdatedArchetype = _HealthUpdatedArchetype,
            Dead = GetComponentDataFromEntity<DeadData>()
        };
        inputDeps = job.Schedule(this, inputDeps);
        inputDeps.Complete();
        _Barrier.AddJobHandleForProducer(inputDeps);
        return inputDeps;
    }
    
    private static int CalculateDamage(SecondUpgrade secondUpgrade, int damage)
    {
        var baseDamage = damage;

        switch (secondUpgrade)
        {
            case SecondUpgrade.ResistanceMin:
                baseDamage -= Mathf.RoundToInt(baseDamage / 4f);
                break;
            case SecondUpgrade.ResistanceMed:
                baseDamage -= Mathf.RoundToInt(baseDamage / 2f);
                break;
            case SecondUpgrade.ResistanceMax:
                baseDamage = Mathf.RoundToInt(baseDamage / 4f);
                break;
        }
        
        return baseDamage;
    }
}
