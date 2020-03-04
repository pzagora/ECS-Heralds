using System.Collections.Generic;
using DG.Tweening;
using Enums;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Tween = PlasticApps.Tween;

public class GameUi : MonoBehaviour
{
    public Image FadeScreen;
    public Text GameOverText;
    public Slider HealthSlider;
    public Image DamageImage;
    public Text ScoreText;
    public Transform OptionsPanel;

    [SerializeField] private Sprite[] FirstUpgradeImages;
    [SerializeField] private string[] FirstUpgradeNames;
    [SerializeField] private Sprite[] SecondUpgradeImages;
    [SerializeField] private string[] SecondUpgradeNames;
    [SerializeField] private Sprite[] BorderImages;

    [SerializeField] private Image FirstUpgrade;
    [SerializeField] private Image FirstUpgradeBorder;
    [SerializeField] private Text FirstUpgradeName;
    [SerializeField] private Image SecondUpgrade;
    [SerializeField] private Image SecondUpgradeBorder;
    [SerializeField] private Text SecondUpgradeName;

    [SerializeField] private Transform NotificationParent;
    [SerializeField] private GameObject NotificationPrefab;

    public float FlashSpeed = 5f;
    public Color FlashColor = new Color(1f, 0f, 0f, 0.2f);

    private const string VictoryText = "Victory";
    private const string DeathText = "Game Over";
    [SerializeField] private Color VictoryColour;
    [SerializeField] private Color DeathColour;

    [SerializeField] private Text PlayersAliveText;
    
    private int _Score;
    private Dictionary<int, List<int>> _KillLog;
    
    private const float FadeDuration = 2f;
    private bool damaged;

    private readonly string[] _Nicknames = new string[21];
    private bool _PlayerDied;

    private Vector3 CurrentOptionsPosition;

    private void Awake()
    {
        var position = OptionsPanel.position;
        CurrentOptionsPosition = position;
        position = new Vector3(position.x, -300, position.z);
        OptionsPanel.position = position;

        OptionsPanel.gameObject.SetActive(false);
        GameOverText.gameObject.SetActive(false);
        ScoreText.gameObject.SetActive(false);
        _PlayerDied = false;
        _Score = 0;
        _KillLog = new Dictionary<int, List<int>>();
        
        for (int i = 0; i < _Nicknames.Length; i++)
        {
            _Nicknames[i] = i == 0 
                ? "Zone" 
                : i == 1 
                    ? PlayerPrefs.GetString("Nick", "?") 
                    : $"Player{Random.Range(500, 99999)}";
            _KillLog.Add(i, new List<int>());
        }
    }

    private void Update()
    {
        DamageImage.color = damaged ? FlashColor : Color.Lerp(DamageImage.color, Color.clear, FlashSpeed * Time.deltaTime);

        damaged = false;
    }

    public void FadeAway()
    {
        Tween.Delay(2f, () =>
        {
            GameOverText.gameObject.SetActive(false);
            FadeScreen.DOFade(0f, FadeDuration).OnComplete(() =>
            {
                World.Active.GetExistingSystem<EnemyMovementSystem>().Enabled = true;
                World.Active.GetExistingSystem<EnemyShootingSystem>().Enabled = true;
                World.Active.GetExistingSystem<PlayerMovementSystem>().Enabled = true;
                World.Active.GetExistingSystem<PlayerShootingSystem>().Enabled = true;
                World.Active.GetExistingSystem<PlayerTurningSystem>().Enabled = true;
                World.Active.GetExistingSystem<ZoneShrinkingSystem>().Enabled = true;
            });
        });
    }
        
    public void OnPlayerTookDamage(float newHealth)
    {
        HealthSlider.value = newHealth;
        damaged = true;
    }

    private void AddScore(int score)
    {
        _Score += score;
    }

    private void TriggerGameOver(Color color, string text)
    {
        FadeScreen.DOFade(1f, FadeDuration).OnComplete(() =>
        {
            ScoreText.text = $"<size=35>Prestige:</size> <b>{_Score}</b>";
            GameOverText.text = text;
            GameOverText.color = color;
            GameOverText.gameObject.SetActive(true);
            ScoreText.gameObject.SetActive(true);
            OptionsPanel.gameObject.SetActive(true);
            OptionsPanel.DOMoveY(CurrentOptionsPosition.y, 2f);
        });
    }

    public void OnPlayerKilled()
    {
        _PlayerDied = true;
        TriggerGameOver(DeathColour, DeathText);
    }

    private void OnVictory()
    {
        TriggerGameOver(VictoryColour, VictoryText);
    }

    public void UpdateLoadingText(string loadingType, int current, int max)
    {
        //GameOverText.gameObject.SetActive(true);
        //GameOverText.text = $"Generating {loadingType}: {current} / {max}";
    }

    public void LogKill(int killer, int killed)
    {
        _KillLog[killer].Add(killed);

        if (killer == 1)
        {
            AddScore(HeraldsBootstrap.Settings.ScorePerDeath * (killed == 1 ? -1 : 1));
        }

        var killAmount = 0;
        foreach (var killList in _KillLog.Values)
        {
            killAmount += killList.Count;
        }

        FadeScreen.DOFade(0f, 0).OnComplete(() =>
        {
            var notification = Instantiate(NotificationPrefab, NotificationParent);
            notification.AddComponent<Notification>().Init(_Nicknames[killer], _Nicknames[killed]);

            if (PlayersAliveText != null)
            {
                PlayersAliveText.text = $"Players Alive: <i><b><size=18>{20 - killAmount}</size></b> / 20</i>";
            }
        });
        
        if (killAmount == 19 && !_PlayerDied)
        {
            OnVictory();
        }
    }

    public void OnUpgradeSwap(FirstUpgrade firstUpgrade, SecondUpgrade secondUpgrade)
    {
        Debug.Log("Upgrade Swapped");
        
        if (firstUpgrade != Enums.FirstUpgrade.None)
        {
            if (FirstUpgradeImages != null && FirstUpgradeNames != null && BorderImages != null)
            {
                var index = (int)firstUpgrade;
                FirstUpgrade.sprite = FirstUpgradeImages[index > 3 ? 2 : index > 0 ? 1 : 0];
                FirstUpgradeBorder.sprite = BorderImages[index % 3 == 0 ? 2 : index % 2 == 0 ? 1 : 0];
                FirstUpgradeName.text = FirstUpgradeNames[index];
            }
        }
        else
        {
            if (SecondUpgradeImages != null && SecondUpgradeNames != null && BorderImages != null)
            {
                var index = (int)secondUpgrade;
                SecondUpgrade.sprite = SecondUpgradeImages[index > 3 ? 2 : index > 0 ? 1 : 0];
                SecondUpgradeBorder.sprite = BorderImages[index % 3 == 0 ? 2 : index % 2 == 0 ? 1 : 0];
                SecondUpgradeName.text = SecondUpgradeNames[index];
            }
        }
        
        AddScore(HeraldsBootstrap.Settings.ScorePerUpgrade);
    }

    private void DisposeGameWorld()
    {
        var entityManager = World.Active.EntityManager;
        entityManager.DestroyEntity(entityManager.UniversalQuery);
    }
    
    public void RestartGame()
    {
        DisposeGameWorld();
        SceneManager.LoadScene(1);
    }
    
    public void GoToMenu()
    {
        DisposeGameWorld();
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        DisposeGameWorld();
        Application.Quit();
    }
}
