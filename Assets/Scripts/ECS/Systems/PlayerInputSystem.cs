using Unity.Entities;
using Unity.Mathematics;
using UnityStandardAssets.CrossPlatformInput;

[DisableAutoCreation]
public class PlayerInputSystem : ComponentSystem
{
    private EntityQuery _Query;

    protected override void OnCreate()
    {
        _Query = GetEntityQuery(
            ComponentType.ReadOnly<PlayerInputData>(),
            ComponentType.Exclude<DeadData>());
    }

    protected override void OnUpdate()
    {
        Entities.With(_Query).ForEach(entity =>
        {
            var newInput = new PlayerInputData
            {
                Move = new float2(
                    CrossPlatformInputManager.GetAxisRaw("Horizontal"), 
                    CrossPlatformInputManager.GetAxisRaw("Vertical"))
            };
            PostUpdateCommands.SetComponent(entity, newInput);
        });
    }
}
