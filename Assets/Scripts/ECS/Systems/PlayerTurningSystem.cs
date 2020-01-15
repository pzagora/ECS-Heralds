﻿using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[DisableAutoCreation]
public class PlayerTurningSystem : ComponentSystem
{
    private EntityQuery query;

    protected override void OnCreate()
    {
        query = GetEntityQuery(
            ComponentType.ReadOnly<Rotation>(),
            ComponentType.ReadOnly<PlayerData>(),
            ComponentType.ReadOnly<Rigidbody>(),
            ComponentType.Exclude<DeadData>());
    }

    protected override void OnUpdate()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;
        
        var mousePos = Input.mousePosition;

        var camRayLen = HeraldsBootstrap.Settings.CamRayLen;
        var floor = LayerMask.GetMask("Ground");

        Entities.With(query).ForEach((Entity entity, Rigidbody rigidBody) =>
        {
            var camRay = mainCamera.ScreenPointToRay(mousePos);
            RaycastHit floorHit;
            if (Physics.Raycast(camRay, out floorHit, camRayLen, floor))
            {
                var position = rigidBody.gameObject.transform.position;
                var playerToMouse = floorHit.point - new Vector3(position.x, position.y, position.z);
                playerToMouse.y = 0f;
                var newRot = Quaternion.LookRotation(playerToMouse);
                rigidBody.MoveRotation(newRot);
            }
        });
    }
}
