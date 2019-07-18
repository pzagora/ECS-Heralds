using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class NoiseGenerator : MonoBehaviour
{
    [SerializeField] private Mesh _Mesh;
    [SerializeField] private Material[] _Material;

    private int _Width = 256;
    private int _Height = 256;

    private float _Scale = 20f;

    private float _BorderRadius = 2.1f;

    private float _OffsetX = 0;
    private float _OffsetY = 0;

    void Start()
    {
        _OffsetX = UnityEngine.Random.Range(0, (float)(_Width * _Height));
        _OffsetY = UnityEngine.Random.Range(0, (float)(_Width * _Height));

        var rendererComponent = GetComponent<Renderer>();
        rendererComponent.material.mainTexture = GenerateTexture();

        InstantiateCubesBasedOnNoise((Texture2D)rendererComponent.material.mainTexture);
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

    private void InstantiateCubesBasedOnNoise(Texture2D noise)
    {
        var entityManager = World.Active.EntityManager;

        var levelArchetype = entityManager.CreateArchetype(
            typeof(LevelComponent),
            typeof(Translation),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(LocalToWorld)
        );

        var entityArray = new NativeArray<Entity>(noise.width * noise.height, Allocator.Temp);
        entityManager.CreateEntity(levelArchetype, entityArray);

        var x = 0;
        var z = 0;

        float[] availableRotations = {0, 90, 180, 270};
        

        for (var i = 0; i < entityArray.Length; i++)
        {
            var entity = entityArray[i];
            entityManager.SetComponentData(entity, new LevelComponent { Level = i });
            entityManager.SetComponentData(entity, new Translation {Value = new Vector3(x, 10 * GetHeight(x, z), z)});
            entityManager.SetComponentData(entity, new Rotation { Value = Quaternion.Euler(0, availableRotations[UnityEngine.Random.Range(0, availableRotations.Length)], 0) });

            int height = (int)entityManager.GetComponentData<Translation>(entity).Value.y;

            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = _Mesh,
                material = height < 4 
                    ? _Material[0] 
                    : height == 4
                        ? _Material[1] 
                        : height >= 7
                          ? _Material[3]
                          : _Material[2]
            });

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

    private float GetHeight(int x, int y)
    {
        var xCoordinate = (float)x / _Width * _Scale + _OffsetX;
        var yCoordinate = (float)y / _Height * _Scale + _OffsetY;

        var perlinNoise = Mathf.PerlinNoise(xCoordinate, yCoordinate);
        var distance = Vector3.Distance(new Vector3(x, y, 0), new Vector3(_Width / 2, _Height / 2, 0));

        if (distance >= _Width / _BorderRadius || distance >= _Height / _BorderRadius)
        {
            if (distance > _Width / (_BorderRadius - 0.03f) || distance > _Height / (_BorderRadius - 0.03f))
            {
                return 0.3f;
            }

            return 0.4f;
        }

        return perlinNoise < 0.3f || perlinNoise > 0.7f ? perlinNoise : perlinNoise < 0.38f ? 0.4f : 0.5f;
    }
}
