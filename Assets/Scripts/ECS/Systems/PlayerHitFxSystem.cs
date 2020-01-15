using Unity.Entities;
using UnityEngine;

public class PlayerHitFxSystem : ComponentSystem
{
    private EntityQuery _HpUpdatedQuery;
    private EntityQuery _PlayerQuery;
    
    private static readonly int DieHash = Animator.StringToHash("Die");

    protected override void OnCreate()
    {
        _HpUpdatedQuery = GetEntityQuery(
            ComponentType.ReadOnly<HealthUpdatedData>());
        _PlayerQuery = GetEntityQuery(
            ComponentType.ReadOnly<PlayerData>(),
            ComponentType.ReadOnly<Animator>(),
            ComponentType.ReadOnly<AudioSource>()
        );
    }

    protected override void OnUpdate()
    {
        var gameUi = HeraldsBootstrap.Settings.GameUi;
        
        Entities.With(_HpUpdatedQuery).ForEach((Entity entity, ref HealthUpdatedData hp) =>
        {
            PostUpdateCommands.DestroyEntity(entity);
            gameUi.OnPlayerTookDamage(hp.Health);

            var health = hp.Health;
            Entities.With(_PlayerQuery).ForEach((AudioSource audio, Animator animator) =>
            {
                if (health <= 0)
                {
                    var playerDeathClip = HeraldsBootstrap.Settings.PlayerDeathClip;
                    audio.clip = playerDeathClip;

                    animator.SetTrigger(DieHash);
                }

                audio.Play();
            });

        });
        
    }
}
