using PlasticApps;
using Unity.Entities;
using UnityEngine;

public class Attacker : MonoBehaviour
{
    private EntityManager _EntityManager;
    private EntityArchetype _AttackArchetype;
    private Entity _AttackEntity;
    private Entity _Source;
    private int _Damage = 20;

    public void SetSource(Entity entity)
    {
        _Source = entity;
    }

    public void SetDamage(int value)
    {
        _Damage = value;
    }
    
    private void Start()
    {
        gameObject.GetComponent<SphereCollider>().enabled = false;
        Tween.Delay(1f, () =>
        {
            GameObject go;
            (go = gameObject).GetComponent<SphereCollider>().enabled = true;
            Tween.Delay(0.1f, () =>
            {
                go.GetComponent<SphereCollider>().enabled = false;
                Destroy(go, 0.4f);
            });
        });
        _EntityManager = World.Active.EntityManager;
        _AttackArchetype = _EntityManager.CreateArchetype(typeof(AttackData));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            _AttackEntity = _EntityManager.CreateEntity(_AttackArchetype);
            _EntityManager.SetComponentData(_AttackEntity, new AttackData
            {
                Damage = _Damage,
                Source = _Source,
                Target = other.GetComponent<GameObjectEntity>().Entity
            });
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_EntityManager.Exists(_AttackEntity))
        {
            _EntityManager.DestroyEntity(_AttackEntity);
        }
    }
}
