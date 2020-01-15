using System;
using Enums;
using Unity.Entities;
using UnityEngine;
using Random = System.Random;

public class ItemPicker : MonoBehaviour
{
    private GameObject _Player;
    private EntityManager _EntityManager;
    private EntityArchetype _ItemChangeArchetype;
    private Entity _ItemChangeEntity;

    private void Start()
    {
        RollForUpgrade();
        _Player = GameObject.FindGameObjectWithTag("Player");
        _EntityManager = World.Active.EntityManager;
        _ItemChangeArchetype = _EntityManager.CreateArchetype(typeof(ItemChange));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _Player)
        {
            if (FirstUpgrade == FirstUpgrade.None)
            {
                Debug.Log($"Can change upgrade - {SecondUpgrade}");
            }
            else
            {
                Debug.Log($"Can change upgrade - {FirstUpgrade}");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == _Player && Input.GetKeyDown(KeyCode.E))
        {
            _ItemChangeEntity = _EntityManager.CreateEntity(_ItemChangeArchetype);
            _EntityManager.SetComponentData(_ItemChangeEntity, new ItemChange
            {
                Target = _Player.GetComponent<GameObjectEntity>().Entity,
                FirstUpgrade = FirstUpgrade,
                SecondUpgrade = SecondUpgrade
            });
            HeraldsBootstrap.Settings.GameUi.OnUpgradeSwap(FirstUpgrade, SecondUpgrade);
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == _Player && _EntityManager.Exists(_ItemChangeEntity))
        {
            _EntityManager.DestroyEntity(_ItemChangeEntity);
            Debug.Log("Can't' change upgrade");
        }
    }

    private void RollForUpgrade()
    {
        var roll = UnityEngine.Random.Range(0, 100);
        if (roll < 50)
        {
            SecondUpgrade = SecondUpgrade.None;
            var values = Enum.GetValues(typeof(FirstUpgrade));
            FirstUpgrade = (FirstUpgrade)values.GetValue(UnityEngine.Random.Range(1, values.Length));
        }
        else
        {
            FirstUpgrade = FirstUpgrade.None;
            var values = Enum.GetValues(typeof(SecondUpgrade));
            SecondUpgrade = (SecondUpgrade)values.GetValue(UnityEngine.Random.Range(1, values.Length));
        }
    }

    public FirstUpgrade FirstUpgrade { get; set; }
    public SecondUpgrade SecondUpgrade { get; set; }
}
