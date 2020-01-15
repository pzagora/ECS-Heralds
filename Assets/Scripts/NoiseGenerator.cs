using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;
using Random = Unity.Mathematics.Random;

public class NoiseGenerator
{
    [SerializeField] private Mesh _Mesh;
    [SerializeField] private Material[] _Material;

    private readonly int _Width = HeraldsBootstrap.Settings.MapWidth;
    private readonly int _Height = HeraldsBootstrap.Settings.MapHeight;
    private readonly float _Scale = HeraldsBootstrap.Settings.Scale;
    private readonly float _BorderRadius = HeraldsBootstrap.Settings.BorderRadius;

    private float _OffsetX = 0;
    private float _OffsetY = 0;

    private float[] _AvailableRotations = {0, 90, 180, 270};
    
    public MapInfo InitializeMap()
    {
        _Mesh = HeraldsBootstrap.Settings.GroundCubeMesh;
        _Material = HeraldsBootstrap.Settings.GroundCubeMaterial;
        
        _OffsetX = UnityEngine.Random.Range(0, (float)(_Width * _Height));
        _OffsetY = UnityEngine.Random.Range(0, (float)(_Width * _Height));

        var texture = GenerateTexture();
        
        var mapInfo = new MapInfo();
        InstantiateCubesBasedOnNoise(texture, ref mapInfo);
        SelectPlayerSpawnLocations(ref mapInfo);
        SelectUpgradesLocations(ref mapInfo);

        mapInfo.GrassLocations =
            mapInfo.GrassLocations.Except(mapInfo.PlayerSpawns).Except(mapInfo.UpgradeSpawns).ToList();
        mapInfo.SandLocations =
            mapInfo.SandLocations.Except(mapInfo.PlayerSpawns).Except(mapInfo.UpgradeSpawns).ToList();

        return mapInfo;
    }

    private void SelectUpgradesLocations(ref MapInfo mapInfo)
    {
        var possibleLocations = mapInfo.GrassLocations.Concat(mapInfo.SandLocations).ToList();
        possibleLocations = possibleLocations.Except(mapInfo.PlayerSpawns).ToList();
        var upgradeAmount = Mathf.FloorToInt(possibleLocations.Count / 100f);

        for (int i = 0; i < upgradeAmount; i++)
        {
            Vector3 location = default;
            do
            {
                location = possibleLocations[UnityEngine.Random.Range(0, possibleLocations.Count)];
            } while (mapInfo.UpgradeSpawns.Contains(location) || !IsFarEnough(location, mapInfo.UpgradeSpawns, 2));

            mapInfo.UpgradeSpawns.Add(location);
        }
    }

    private void SelectPlayerSpawnLocations(ref MapInfo mapInfo)
    {
        var possibleLocations = mapInfo.GrassLocations.Concat(mapInfo.SandLocations).ToList();
        mapInfo.PlayerSpawns.Add(possibleLocations[UnityEngine.Random.Range(0, possibleLocations.Count)]);
        
        for (int i = 0; i < HeraldsBootstrap.Settings.SpawnsAmount - 1; i++)
        {
            Vector3 location = default;
            do
            {
                location = possibleLocations[UnityEngine.Random.Range(0, possibleLocations.Count)];
            } while (mapInfo.PlayerSpawns.Contains(location) || !IsFarEnough(location, mapInfo.PlayerSpawns));
            
            mapInfo.PlayerSpawns.Add(location);
        }
    }

    private bool IsFarEnough(Vector3 point, List<Vector3> targets, int maxDistance = -1)
    {
        var result = true;
        if (maxDistance == -1)
        {
            maxDistance = HeraldsBootstrap.Settings.MapWidth / (HeraldsBootstrap.Settings.MapWidth / 3);
        }
        
        foreach (var target in targets)
        {
            var distance = (point - target).magnitude;
            if (distance < maxDistance)
            {
                result = false;
            }
        }
        
        return result;
    }

    private Texture2D GenerateTexture()
    {
        var texture = new Texture2D(_Width, _Height);

        for (var i = 0; i < _Width; i++)
        {
            for (var j = 0; j < _Height; j++)
            {
                var color = CalculateColorGreyScale(i, j);
                texture.SetPixel(i, j, color);
            }
        }

        texture.Apply();
        return texture;
    }

    private Color CalculateColorGreyScale(int x, int y)
    {
        var sample = GetHeight(x, y);
        return new Color(sample, sample, sample);
    }

    private void InstantiateCubesBasedOnNoise(Texture2D noise, ref MapInfo mapInfo)
    {
        var entityManager = World.Active.EntityManager;

        var groundArchetype = entityManager.CreateArchetype(
            typeof(Collideable),
            typeof(Translation),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(PhysicsCollider),
            typeof(QuadrantEntity)
        );

        var entityArray = new NativeList<Entity>(Allocator.Temp);

        var x = 0;
        var z = 0;
        var mapSize = noise.width * noise.height;

        for (var i = 0; i < mapSize; i++)
        {
            var height = AdjustHeight(10 * GetHeight(x, z));

            if (height != 0)
            {
                var groundLevel = GetGroundLevel(height);

                switch (groundLevel)
                {
                    case GroundLevel.Sand:
                        mapInfo.SandLocations.Add(new Vector3(x, -0.5f, z));
                        break;
                    case GroundLevel.Grass:
                        mapInfo.GrassLocations.Add(new Vector3(x, -0.5f, z));
                        break;
                    case GroundLevel.Mountain:
                        mapInfo.MountainLocations.Add(new Vector3(x, UnityEngine.Random.Range(-0.4f, -0.8f), z));
                        break;
                }
                
                var entity = entityManager.CreateEntity(groundArchetype);
                entityArray.Add(entity);
                entityManager.SetComponentData(entity,
                    new Collideable
                    {
                        Type = groundLevel > GroundLevel.Water && groundLevel < GroundLevel.Mountain
                            ? CollisionType.Walkable
                            : CollisionType.NonWalkable
                    });
                entityManager.SetComponentData(entity,
                    new PhysicsCollider {Type = ColliderType.Box, Size = 1f, Height = 0f});
                entityManager.SetComponentData(entity, new Translation {Value = new Vector3(x, -1f, z)});
                entityManager.SetComponentData(entity,
                    new Rotation
                    {
                        Value = Quaternion.Euler(0,
                            _AvailableRotations[UnityEngine.Random.Range(0, _AvailableRotations.Length)], 0)
                    });
                entityManager.SetSharedComponentData(entity, new RenderMesh
                {
                    mesh = _Mesh,
                    material = _Material[(int)groundLevel],
                    layer = 8
                });
            }

            z++;

            if (z < noise.height)
            {
                continue;
            }

            z = 0;
            x++;
        }

        entityArray.Dispose();
    }

    private GroundLevel GetGroundLevel(int height)
    {
        return height < 4
            ? GroundLevel.Water
            : height == 4
                ? GroundLevel.Sand
                : height < 6
                    ? GroundLevel.Grass
                    : GroundLevel.Mountain;
    }

    private float GetHeight(int x, int y)
    {
        var xCoordinate = (float)x / _Width * _Scale + _OffsetX;
        var yCoordinate = (float)y / _Height * _Scale + _OffsetY;

        var perlinNoise = Mathf.PerlinNoise(xCoordinate, yCoordinate);
        var distance = Vector3.Distance(new Vector3(x, y, 0), new Vector3(_Width / 2, _Height / 2, 0));

        if (distance > _Width / (_BorderRadius - 0.12f) || distance > _Height / (_BorderRadius - 0.12f))
        {
            return 0;
        }

        if (distance >= _Width / _BorderRadius || distance >= _Height / _BorderRadius)
        {
            if (distance > _Width / (_BorderRadius - 0.03f) || distance > _Height / (_BorderRadius - 0.03f))
            {
                return 0.3f;
            }

            return perlinNoise < 0.3f ? perlinNoise : 0.4f;
        }

        var height = perlinNoise < 0.3f || perlinNoise > 0.7f ? perlinNoise : perlinNoise < 0.38f ? 0.4f : 0.5f;

        return height == 0 ? height + 0.1f : height;
    }

    private int AdjustHeight(float currentHeight)
    {
        var newHeight = 6;
        var step = 1;
        var baseHeight = 7;

        if (currentHeight > baseHeight)
        {
            var diff = currentHeight - baseHeight;

            if (diff < 0.18f)
            {
                return (int)currentHeight;
            }

            var height = (int)math.floor(diff / 0.18f);
            height *= step;

            return height + newHeight;
        }

        if (currentHeight == 0)
        {
            return 0;
        }

        if (currentHeight < 3.5f)
        {
            return 3;
        }

        if (currentHeight < 4.5f)
        {
            return 4;
        }

        return (int)currentHeight;
    }
}

public class MapInfo
{
    public List<Vector3> SandLocations;
    public List<Vector3> GrassLocations;
    public List<Vector3> MountainLocations;
    public List<Vector3> PlayerSpawns;
    public List<Vector3> UpgradeSpawns;

    public MapInfo()
    {
        SandLocations = new List<Vector3>();
        GrassLocations = new List<Vector3>();
        MountainLocations = new List<Vector3>();
        PlayerSpawns = new List<Vector3>();
        UpgradeSpawns = new List<Vector3>();
    }
}
