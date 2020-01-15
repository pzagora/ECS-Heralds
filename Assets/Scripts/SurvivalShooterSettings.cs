using Unity.Mathematics;
using UnityEngine;

public class SurvivalShooterSettings : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    public GameObject UpgradePrefab;
    public GameObject GroundCube;
    public GameObject PropCube;
    public Mesh GroundCubeMesh;
    public Material[] GroundCubeMaterial;
    public AudioClip PlayerDeathClip;
    public AudioClip EnemyDeathClip;

    [Header("Attacks")] 
    public GameObject DamagePrefab;
    public int AttackDamage = 20;
    public float MinTimeBetweenEnemyAttacks = 0.5f;
    public float MaxTimeBetweenEnemyAttacks = 1.5f;
    
    
    [Header("Zone")] 
    public GameObject ZonePrefab;
    public Mesh ZoneMesh;
    public Material ZoneMaterial;
    public float ZoneShrinkTime = 20f;
    public float TimeBetweenShrinks = 15f;
    public int ZoneDamagePerTick = 1;
    public float TimePerTick = 0.2f;

    [HideInInspector]
    public GameUi GameUi;

    [Space(15)]
    public float PlayerMoveSpeed = 0.2f;
    public int StartingPlayerHealth = 100;

    public int StartingEnemyHealth = 100;
    public float EnemySinkSpeed = 2.5f;
    public float TimeBetweenEnemyAttacks = 0.5f;

    public float TimeBetweenBullets = 0.15f;
    public float GunRange = 100f;
    public float GunEffectsDisplayTime = 0.2f;

    public int DamagePerShot = 20;

    public int ScorePerDeath = 25;
    public int ScorePerUpgrade = 7;

    [Header("Camera")]
    public float CamRayLen = 100f;
    public float CamSmoothing = 5f;

    [Header("Map")]
    public int MapWidth = 100;
    public int MapHeight = 100;    
    public float Scale = 20f;
    public float BorderRadius = 2.1f;
    public int SpawnsAmount = 20;

    [Header("Floor Object")] 
    public GameObject Floor;
    
    [Header("Enemy")]
    public float EnemyMinMovePause = 0.2f;
    public float EnemyMaxMovePause = 1.4f;
    public float2 EnemyMovePauseOutsideRange;
    public float EnemyMinShootingDelay = 1f;
    public float EnemyMaxShootingDelay = 4f;

    [Header("Props")] public GameObject SpawnPlatform;
    public GameObject UpgradePlatform;
    public Mesh[] MountainMeshes;
    public Material[] MountainMaterials;
    public Mesh[] DesertMeshes;
    public Material DesertMaterial;
    public float[] DesertScales;
    public Mesh[] ForestMeshes;
    public Material[] BushMaterials;
    public Material[] LogMaterials;
    public Material[] TreeMaterials;
    public Vector2[] ForestScales;
    public Mesh GrassMesh;
    public Material[] GrassMaterials;
    public float2 GrassScale;
    
    
    [Header("Misc.")] 
    public Transform PlayerParent;
    public Transform UpgradeParent;
}
