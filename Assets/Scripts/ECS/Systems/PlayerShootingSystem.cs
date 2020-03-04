using Enums;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class PlayerShootingSystem : ComponentSystem
{
    private EntityQuery gunQuery;
    private EntityQuery playerQuery;

    private float timer;

    protected override void OnCreate()
    {
        gunQuery = GetEntityQuery(
            ComponentType.ReadOnly<Transform>(),
            ComponentType.ReadOnly<PlayerGunData>(),
            ComponentType.ReadOnly<ParticleSystem>(),
            ComponentType.ReadOnly<LineRenderer>(),
            ComponentType.ReadOnly<AudioSource>(),
            ComponentType.ReadOnly<Light>());
        playerQuery = GetEntityQuery(
            ComponentType.ReadOnly<PlayerData>(),
            ComponentType.ReadOnly<HealthData>());
    }

    protected override void OnUpdate()
    {
        var playerEntities = playerQuery.ToEntityArray(Allocator.TempJob);
        var hp = playerQuery.ToComponentDataArray<HealthData>(Allocator.TempJob);
        var firstUpgradeData = playerQuery.ToComponentDataArray<PlayerData>(Allocator.TempJob);
        
        if (hp.Length == 0 || playerEntities.Length == 0 || firstUpgradeData.Length == 0)
        {
            playerEntities.Dispose();
            firstUpgradeData.Dispose();
            hp.Dispose();
            return;
        }

        if (hp[0].Value <= 0)
        {
            playerEntities.Dispose();
            firstUpgradeData.Dispose();
            hp.Dispose();
            return;
        }
        
        var player = playerEntities[0];
        var firstUpgrade = firstUpgradeData[0].FirstUpgrade;
        
        hp.Dispose();
        firstUpgradeData.Dispose();
        playerEntities.Dispose();

        timer += Time.deltaTime;

        var timeBetweenBullets = CalculateTimeBetweenShoots(firstUpgrade);
        var effectsDisplayTime = HeraldsBootstrap.Settings.GunEffectsDisplayTime;

        Entities.With(gunQuery).ForEach(
            (Entity entity, AudioSource audio, Light light, ParticleSystem particles, LineRenderer line) =>
            {
                if (Input.GetButton("Fire1") && timer > timeBetweenBullets)
                    Shoot(player, audio, firstUpgrade);

                if (timer >= timeBetweenBullets * effectsDisplayTime)
                    DisableEffects(light, line);
            });
    }

    private void Shoot(Entity entity, AudioSource audio, FirstUpgrade firstUpgrade)
    {
        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;
        
        var mousePos = Input.mousePosition;
        var camRayLen = HeraldsBootstrap.Settings.CamRayLen;
        var floor = LayerMask.GetMask("Ground");
        
        var camRay = mainCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(camRay, out var shootHit, camRayLen, floor))
        {
            var damageObject = Object.Instantiate(
                HeraldsBootstrap.Settings.DamagePrefab,
                shootHit.point,
                Quaternion.identity
                );
            damageObject.GetComponent<Attacker>().SetSource(entity);
            damageObject.GetComponent<Attacker>().SetDamage(CalculateDamage(firstUpgrade));
        }
    }

    private void DisableEffects(Light light, LineRenderer line)
    {
        light.enabled = false;
        line.enabled = false;
    }

    private float CalculateTimeBetweenShoots(FirstUpgrade firstUpgrade)
    {
        var baseTime = HeraldsBootstrap.Settings.TimeBetweenBullets;
        baseTime *= 3;

        switch (firstUpgrade)
        {
            case FirstUpgrade.AttackSpeedMin:
                baseTime *= 3f / 4f;
                break;
            case FirstUpgrade.AttackSpeedMed:
                baseTime *= 2f / 4f;
                break;
            case FirstUpgrade.AttackSpeedMax:
                baseTime *= 1f / 4f;
                break;
        }
        
        return baseTime;
    }
    
    private int CalculateDamage(FirstUpgrade firstUpgrade)
    {
        var baseDamage = HeraldsBootstrap.Settings.AttackDamage;

        switch (firstUpgrade)
        {
            case FirstUpgrade.DamageMin:
                baseDamage += 10;
                break;
            case FirstUpgrade.DamageMed:
                baseDamage += 20;
                break;
            case FirstUpgrade.DamageMax:
                baseDamage += 40;
                break;
        }
        
        return baseDamage;
    }
}
