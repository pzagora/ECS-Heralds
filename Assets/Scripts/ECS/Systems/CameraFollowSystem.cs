using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public class CameraFollowSystem : ComponentSystem
{
    private EntityQuery _Query;
    
    private bool _FirstFrame = true;
    private Vector3 _Offset;

    protected override void OnCreate()
    {
        _Query = GetEntityQuery(
            ComponentType.ReadOnly<Transform>(),
            ComponentType.ReadOnly<PlayerInputData>());
    }

    protected override void OnUpdate()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Entities.With(_Query).ForEach(
            (Entity entity, Transform transform, ref PlayerInputData data) =>
            {
                var go = transform.gameObject;
                var playerPos = go.transform.position;

                if (_FirstFrame)
                {
                    _Offset = mainCamera.transform.position - playerPos;
                    _FirstFrame = false;
                }

                var smoothing = HeraldsBootstrap.Settings.CamSmoothing;
                var dt = Time.deltaTime;
                var targetCamPos = playerPos + _Offset;
                mainCamera.transform.position =
                    Vector3.Lerp(mainCamera.transform.position, targetCamPos, smoothing * dt);
            });
    }
}
