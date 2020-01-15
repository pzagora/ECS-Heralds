using System.Collections.Generic;
using PlasticApps;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ZoneShrinkingSystem : ComponentSystem
{
    private List<Entity> _TweenRecord;
    private EntityQuery _ZoneQuery;

    private bool firstShrink = true;
    private float3 nextZoneCenter = Vector3.zero;

    protected override void OnCreate()
    {
        _TweenRecord = new List<Entity>();

        _ZoneQuery = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<NonUniformScale>(),
            ComponentType.ReadOnly<ZoneData>());
    }

    protected override void OnUpdate()
    {

        Entities.With(_ZoneQuery).ForEach(
            (Entity entity, ref Translation translation, ref NonUniformScale scale) =>
            {
                if (!_TweenRecord.Contains(entity))
                {
                    var timeToShrink = HeraldsBootstrap.Settings.ZoneShrinkTime;

                    var translationValue = firstShrink ? translation.Value : nextZoneCenter;
                    var scaleValue = scale.Value.x / 2f;
                    
                    _TweenRecord.Add(entity);
                    Tween.MoveEntity(entity, timeToShrink,
                        firstShrink 
                            ? translation.Value 
                            : nextZoneCenter, translation.Value);
                    Tween.ScaleEntity(entity, timeToShrink, scale.Value / 2f, scale.Value)
                        .OnTweenComplete(() =>
                        {
                            firstShrink = false;
                            nextZoneCenter = CalculateNewPosition(translationValue, scaleValue);
                            Tween.Delay(HeraldsBootstrap.Settings.TimeBetweenShrinks,
                                () => { _TweenRecord.Remove(entity); });
                        });
                }
            });
    }

    private float3 CalculateNewPosition(float3 currentPosition, float radius)
    {
        var x = UnityEngine.Random.Range(currentPosition.x - radius / 4f, currentPosition.x + radius / 4f);
        var z = UnityEngine.Random.Range(currentPosition.z - radius / 4f, currentPosition.z + radius / 4f);
            
        return new float3(x, 0, z);
    }
}
