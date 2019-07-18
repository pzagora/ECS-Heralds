using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEditor.Experimental.GraphView;
using Random = Unity.Mathematics.Random;

public class Test : MonoBehaviour
{
    [SerializeField] private Mesh _Mesh;
    [SerializeField] private Material _Material;

    private void Start()
    {
        var entityManager = World.Active.EntityManager;

        var levelArchetype = entityManager.CreateArchetype(
            typeof(LevelComponent), 
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld)
            );

        var entityArray = new NativeArray<Entity>(9+16, Allocator.Temp);
        entityManager.CreateEntity(levelArchetype, entityArray);


        var boundary = 0;
        var currentPosition = new float3(0, 0, 0);

        for (var i = 0; i < entityArray.Length; i++)
        {
            var entity = entityArray[i];
            entityManager.SetComponentData(entity, new LevelComponent { Level = i });
            entityManager.SetComponentData(entity, new Translation { Value = CalculatePosition(ref currentPosition, ref boundary) });

            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = _Mesh,
                material = _Material
            });
        }

        entityArray.Dispose();
    }

    private float3 CalculatePosition(ref float3 currentPosition, ref int boundary)
    {
        var positionBeforeChange = currentPosition;

        if (boundary == 0)
        {
            boundary++;
            currentPosition.x = -1;
            currentPosition.z = 1;
            return positionBeforeChange;
        }

        if (currentPosition.x < boundary && currentPosition.z == boundary)
        {
            currentPosition.x++;
        }
        else if (currentPosition.x == boundary && currentPosition.z == boundary || currentPosition.x == boundary && currentPosition.z < boundary && currentPosition.z > -boundary)
        {
            currentPosition.z--;
        }
        else if ((currentPosition.x == boundary && currentPosition.z == -boundary) || (currentPosition.x < boundary && currentPosition.z == -boundary && currentPosition.x > -boundary))
        {
            currentPosition.x--;
        }
        else if ((currentPosition.x == -boundary && currentPosition.z == -boundary) || (currentPosition.x == -boundary && currentPosition.z > -boundary && currentPosition.z < boundary - 1))
        {
            currentPosition.z++;
        }
        else if (currentPosition.x == -boundary && currentPosition.z == boundary - 1)
        {
            currentPosition.x--;
            currentPosition.z += 2;
            boundary++;
        }

        return positionBeforeChange;
    }
}
