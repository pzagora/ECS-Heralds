using Unity.Entities;
using UnityEngine;

public class RunFixedUpdateSystems : MonoBehaviour
{
    private PlayerInputSystem playerInputSystem;
    private PlayerMovementSystem playerMovementSystem;
    private PlayerTurningSystem playerTurningSystem;
    private PlayerAnimationSystem playerAnimationSystem;
    private CameraFollowSystem cameraFollowSystem;
    
    private void Start()
    {
        playerInputSystem = World.Active.GetOrCreateSystem<PlayerInputSystem>();
        playerMovementSystem = World.Active.GetOrCreateSystem<PlayerMovementSystem>();
        playerTurningSystem = World.Active.GetOrCreateSystem<PlayerTurningSystem>();
        playerAnimationSystem = World.Active.GetOrCreateSystem<PlayerAnimationSystem>();
        cameraFollowSystem = World.Active.GetOrCreateSystem<CameraFollowSystem>();
        
        World.Active.GetExistingSystem<EnemyMovementSystem>().Enabled = false;
        World.Active.GetExistingSystem<EnemyShootingSystem>().Enabled = false;
        World.Active.GetExistingSystem<PlayerMovementSystem>().Enabled = false;
        World.Active.GetExistingSystem<PlayerShootingSystem>().Enabled = false;
        World.Active.GetExistingSystem<PlayerTurningSystem>().Enabled = false;
        World.Active.GetExistingSystem<ZoneShrinkingSystem>().Enabled = false;
    }

    private void FixedUpdate()
    {
        playerInputSystem.Update();
        playerMovementSystem.Update();
        playerTurningSystem.Update();
        playerAnimationSystem.Update();
        cameraFollowSystem.Update();
    }
}
