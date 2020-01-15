using System.Collections.Generic;
using System.Linq;
using Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public sealed class HeraldsBootstrap
{
    public static SurvivalShooterSettings Settings;
    
    private static readonly float ForestPropOffset = 0.4f;
    private static readonly float GrassOffset = 0.35f;
    private static readonly Vector2[] PropOffset =
    {
        new Vector2(-0.25f, -0.25f),
        new Vector2(0.25f, -0.25f),
        new Vector2(-0.25f, 0.25f),
        new Vector2(0.25f, 0.25f)
    };

    private static EntityArchetype _PropArchetype;
    private static EntityManager _EntityManager;

    public static void NewGame()
    {
        InitializeWithScene();
        Settings.GameUi = GameObject.Find("GameUi").GetComponent<GameUi>();
        
        var noiseGenerator = new NoiseGenerator();
        var mapInfo = noiseGenerator.InitializeMap();

        var camera = Camera.main;
        if (camera != null)
        {
            var transform = camera.transform;
            transform.position = mapInfo.PlayerSpawns[0] + transform.position;
        }
        
        _EntityManager = World.Active.EntityManager;
        _PropArchetype = _EntityManager.CreateArchetype(
            typeof(Translation),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(NonUniformScale),
            typeof(LocalToWorld)
        );

        CreateUpgrades(mapInfo.UpgradeSpawns);
        CreateSpawnPlatforms(mapInfo.PlayerSpawns);
        CreatePlayer(mapInfo.PlayerSpawns[0]);
        CreateEnemies(mapInfo.PlayerSpawns.Skip(1).ToList());
        CreateProps(mapInfo.MountainLocations, mapInfo.SandLocations, mapInfo.GrassLocations);
        CreateGrass(mapInfo.GrassLocations);
        CreateZone();
        
        Settings.GameUi.FadeAway();
    }

    private static void CreatePlayer(Vector3 position)
    {
        var player = Object.Instantiate(Settings.PlayerPrefab, position + Vector3.down, quaternion.identity, Settings.PlayerParent);
        var entity = player.GetComponent<GameObjectEntity>().Entity;
        _EntityManager.AddComponentData(entity, new Translation {Value = player.transform.position});
        _EntityManager.AddComponentData(entity, new Rotation());
        _EntityManager.AddComponentData(entity, new PlayerData());
        _EntityManager.AddComponentData(entity, new CollisionData());
        _EntityManager.AddComponentData(entity, new PhysicsCollider {Type = ColliderType.Box, Size = 1, Height = 1});
        _EntityManager.AddComponentData(entity, new HealthData {Value = Settings.StartingPlayerHealth});
        _EntityManager.AddComponentData(entity, new PlayerInputData {Move = new float2(0, 0)});
        _EntityManager.AddComponentData(entity, new Collideable {Type = CollisionType.NonWalkable});
        _EntityManager.AddComponentData(entity, new NameData {Value = 1});
        _EntityManager.AddComponentData(entity, new QuadrantEntity());
    }

    private static void CreateUpgrades(List<Vector3> locations)
    {
        foreach (var location in locations)
        {
            Object.Instantiate(Settings.UpgradePrefab, location, quaternion.identity, Settings.UpgradeParent);
            Object.Instantiate(Settings.UpgradePlatform, new Vector3(location.x, -0.5f, location.z), quaternion.Euler(0, Random.Range(0, 360), 0), Settings.UpgradeParent);
            Settings.GameUi.UpdateLoadingText("Upgrades", locations.IndexOf(location), locations.Count);
        }
    }   
    
    private static void CreateSpawnPlatforms(List<Vector3> locations)
    {
        foreach (var location in locations)
        {
            Object.Instantiate(Settings.SpawnPlatform, new Vector3(location.x, -0.5f, location.z), quaternion.Euler(0, Random.Range(0, 360), 0));
            Settings.GameUi.UpdateLoadingText("Spawn Platforms", locations.IndexOf(location), locations.Count);
        }
    }

    private static void CreateZone()
    {
        var zoneArchetype = _EntityManager.CreateArchetype(
            typeof(Translation),
            typeof(Rotation),
            typeof(NonUniformScale),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(ZoneData),
            typeof(NameData)
        );
        
        var entity = _EntityManager.CreateEntity(zoneArchetype);
        _EntityManager.SetComponentData(entity, new NameData {Value = 0});
        _EntityManager.SetComponentData(entity, new Translation {Value = new Vector3(Settings.MapWidth / 2f, 0, Settings.MapHeight / 2f)});
        _EntityManager.SetComponentData(entity, new NonUniformScale {Value = Settings.MapWidth + Settings.MapHeight});
        _EntityManager.SetSharedComponentData(entity, new RenderMesh
        {
            mesh = Settings.ZoneMesh,
            material = Settings.ZoneMaterial
        });
    }

    private static void CreateEnemies(List<Vector3> locations)
    {
        for (int i = 0; i < locations.Count; i++)
        {
            var enemy = Object.Instantiate(Settings.EnemyPrefab, locations[i] + Vector3.down, quaternion.identity, Settings.PlayerParent);
            var entity = enemy.GetComponent<GameObjectEntity>().Entity;
            _EntityManager.AddComponentData(entity, new Translation {Value = enemy.transform.position});
            _EntityManager.AddComponentData(entity, new Rotation());
            _EntityManager.AddComponentData(entity, new EnemyData());
            _EntityManager.AddComponentData(entity, new CollisionData());
            _EntityManager.AddComponentData(entity, new PhysicsCollider {Type = ColliderType.Box, Size = 1, Height = 1});
            _EntityManager.AddComponentData(entity, new HealthData {Value = Settings.StartingEnemyHealth});
            _EntityManager.AddComponentData(entity, new Collideable {Type = CollisionType.NonWalkable});
            _EntityManager.AddComponentData(entity, new NameData {Value = i+2});
            _EntityManager.AddComponentData(entity, new QuadrantEntity());
            
            Settings.GameUi.UpdateLoadingText("Upgrades", i, locations.Count);
        }
    }
    
    private static void CreateProps(List<Vector3> mountainLoc, List<Vector3> desertLoc, List<Vector3> forestLoc)
    {
        if (Settings.MountainMeshes.Any() && Settings.MountainMaterials.Any())
        {
            foreach (var location in mountainLoc)
            {
                var objectId = Random.Range(0, Settings.MountainMeshes.Length);
                
                var entity = _EntityManager.CreateEntity(_PropArchetype);
                _EntityManager.SetComponentData(entity, 
                    new Translation
                    {
                        Value = location
                    });
                _EntityManager.SetComponentData(entity,
                    new NonUniformScale
                    {
                        Value = objectId < 12
                            ? new float3(1.3f, Random.Range(1f, 1.7f), 1.3f)
                            : new float3(0.8f, Random.Range(0.8f, 1.3f), 0.8f)
                    });
                _EntityManager.SetComponentData(entity,
                    new Rotation
                    {
                        Value = Quaternion.Euler(0, Random.Range(0, 360), 0)
                    });
                _EntityManager.SetSharedComponentData(entity,
                    new RenderMesh
                    {
                        mesh = Settings.MountainMeshes[objectId],
                        material = Settings.MountainMaterials[objectId],
                    });
                
                Settings.GameUi.UpdateLoadingText("Mountains", mountainLoc.IndexOf(location), mountainLoc.Count);
            }
        }
        
        if (Settings.DesertMeshes.Any() && Settings.DesertScales.Any() && Settings.DesertMaterial != null)
        {
            foreach (var location in desertLoc)
            {
                var random = Random.Range(0f, 100f);
                if (random < 65f)
                {
                    var amount = random < 30f
                        ? 1
                        : random < 57f
                            ? 2
                            : 3;
                    var offsetNumbers = new int[amount];
                    for (var i = 0; i < offsetNumbers.Length; i++)
                    {
                        offsetNumbers[i] = -1;
                    }
                    
                    for (var i = 0; i < amount; i++)
                    {
                        int newValue;
                        do
                        {
                            newValue = Random.Range(0, PropOffset.Length);
                        } while (offsetNumbers.Contains(newValue));
                        offsetNumbers[i] = newValue;
                        
                        var objectId = Random.Range(0, Settings.DesertMeshes.Length);
                        var entity = _EntityManager.CreateEntity(_PropArchetype);
                        _EntityManager.SetComponentData(entity,
                            new Translation
                            {
                                Value = location + new Vector3(PropOffset[offsetNumbers[i]].x, 0, PropOffset[offsetNumbers[i]].y)
                            });
                        _EntityManager.SetComponentData(entity,
                            new NonUniformScale
                            {
                                Value = Vector3.one * Settings.DesertScales[objectId]
                            });
                        _EntityManager.SetComponentData(entity,
                            new Rotation
                            {
                                Value = Quaternion.Euler(0, Random.Range(0, 360), 0)
                            });
                        _EntityManager.SetSharedComponentData(entity,
                            new RenderMesh
                            {
                                mesh = Settings.DesertMeshes[objectId],
                                material = Settings.DesertMaterial
                            });
                    }
                }
                Settings.GameUi.UpdateLoadingText("Sand Items", desertLoc.IndexOf(location), desertLoc.Count);
            }
        }
        
        if (Settings.ForestMeshes.Any() && Settings.ForestScales.Any())
        {
            foreach (var location in forestLoc)
            {
                var random = Random.Range(0f, 100f);
                if (random < 60f)
                {
                    var amount = random % 2;

                    for (var i = 0; i < amount; i++)
                    {
                        var itemId = random < 25 ? Random.Range(0, 2) : random < 50 ? Random.Range(2, 4) : Random.Range(4, 6) ;
                        var materialArray = itemId < 2 ? Settings.BushMaterials : itemId < 4 ? Settings.LogMaterials : Settings.TreeMaterials;

                        var pickedLocation = location + new Vector3(
                                                 Random.value < 0.5 ? -ForestPropOffset : ForestPropOffset,
                                                 0,
                                                 Random.value < 0.5 ? -ForestPropOffset : ForestPropOffset);
                        var pickedScale = Vector3.one * Random.Range(Settings.ForestScales[itemId].x, Settings.ForestScales[itemId].y);
                        var pickedRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                        for (var j = 0; j < materialArray.Length; j++)
                        {
                            var entity = _EntityManager.CreateEntity(_PropArchetype);
                            _EntityManager.SetComponentData(entity,
                                new Translation
                                {
                                    Value = pickedLocation
                                });
                            _EntityManager.SetComponentData(entity,
                                new NonUniformScale
                                {
                                    Value = pickedScale
                                });
                            _EntityManager.SetComponentData(entity,
                                new Rotation
                                {
                                    Value = pickedRotation
                                });
                            _EntityManager.SetSharedComponentData(entity,
                                new RenderMesh
                                {
                                    mesh = Settings.ForestMeshes[itemId],
                                    material = materialArray[j],
                                    subMesh = j
                                });
                        }
                    }
                }
                Settings.GameUi.UpdateLoadingText("Forest Items", forestLoc.IndexOf(location), forestLoc.Count);
            }
        }
    }

    private static void CreateGrass(List<Vector3> grassLoc)
    {
        if (Settings.GrassMaterials != null && Settings.GrassMesh != null)
        {
            foreach (var location in grassLoc)
            {
                var random = Random.Range(0f, 100f);
                if (random < 80f)
                {
                    var amount = random % 14;

                    for (int i = 0; i < amount; i++)
                    {
                        var materialId = Random.Range(0, Settings.GrassMaterials.Length);
                        var entity = _EntityManager.CreateEntity(_PropArchetype);
                        _EntityManager.SetComponentData(entity,
                            new Translation
                            {
                                Value = location + new Vector3(Random.Range(-GrassOffset, GrassOffset), 0, Random.Range(-GrassOffset, GrassOffset))
                            });
                        _EntityManager.SetComponentData(entity,
                            new NonUniformScale
                            {
                                Value = Vector3.one * Random.Range(Settings.GrassScale.x, Settings.GrassScale.y)
                            });
                        _EntityManager.SetComponentData(entity,
                            new Rotation
                            {
                                Value = Quaternion.Euler(0, Random.Range(0, 360), 0)
                            });
                        _EntityManager.SetSharedComponentData(entity,
                            new RenderMesh
                            {
                                mesh = Settings.GrassMesh,
                                material = Settings.GrassMaterials[materialId]
                            });
                    }
                }
                Settings.GameUi.UpdateLoadingText("Grass", grassLoc.IndexOf(location), grassLoc.Count);
            }
        }
    }

    public static void InitializeWithScene()
    {
        var settingsGo = GameObject.Find("-- Settings --");
        Settings = settingsGo.GetComponent<SurvivalShooterSettings>();
        Assert.IsNotNull(Settings);
    }
}
