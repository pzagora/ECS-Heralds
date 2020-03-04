using DG.Tweening;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Tween = PlasticApps.Tween;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Image OverlayImage;
    [SerializeField] private Text GameNameText;
    [SerializeField] private Text CreatorNameText;
    [SerializeField] private Text InfoText;
    [SerializeField] private GameObject LoginInfoObject;
    [SerializeField] private InputField Login;
    [SerializeField] private InputField Password;
    [SerializeField] private Image GameStartedFade;
    [SerializeField] private Color ErrorColor;
    [SerializeField] private Color PositiveColor;

    private float _ScreenHeight;

    private readonly string _MissingInfo = "You have to enter both login and password";
    private readonly string _WrongPassword = "Invalid password";
    private readonly string _Registered = "New account registered";
    private readonly string _LoggedIn = "Logged in as";

    void Start()
    {
        InfoText.gameObject.SetActive(false);
        GameStartedFade.gameObject.SetActive(false);
        LoginInfoObject.SetActive(false);
        _ScreenHeight = Screen.height;
        ShowOverlay();
    }

    public void OnLoginButtonClick()
    {
        LogInOrRegister();
    }

    public void OnGuestPlayClick()
    {
        PlayerPrefs.SetString("Nick", $"Player{Random.Range(500, 99999)}");
        LoadGame();
    }

    private void LoadGame()
    {
        GameStartedFade.gameObject.SetActive(true);
        GameStartedFade.DOFade(1f, 1f).OnComplete(() => { SceneManager.LoadScene(1); });
    }

    public void OnExitButtonClick()
    {
        Application.Quit();
    }

    private void ShowOverlay()
    {
        PlasticApps.Tween.Delay(1.5f, () =>
        {
            GameNameText.DOFade(1f, 3f);
            CreatorNameText.DOFade(1f, 3f);
            OverlayImage.DOFade(0.6f, 2f)
                .OnComplete(() =>
                {
                    PlasticApps.Tween.Delay(2f, () =>
                    {
                        var position = GameNameText.transform.position;
                        GameNameText.transform.DOMove(new Vector3(position.x, position.y + _ScreenHeight / 5), 1f)
                            .OnComplete(() => LoginInfoObject.SetActive(true));
                    });
                });
        });
    }

    private void LogInOrRegister()
    {
        var user = Login.text;
        var password = Password.text;
        
        if (user.Length > 0 && password.Length > 0)
        {
            var request = new LoginWithPlayFabRequest
            {
                Username = user,
                Password = password,
                TitleId = PlayFabSettings.TitleId
            };

            PlayFabClientAPI.LoginWithPlayFab(request, OnLoginResult, OnLoginError);
        }
        else
        {
            InfoText.color = ErrorColor;
            InfoText.text = _MissingInfo;
            InfoText.gameObject.SetActive(true);
            Tween.Delay(1.5f, () => { InfoText.gameObject.SetActive(false); });
        }
    }
        
    private void OnRegisterResult(RegisterPlayFabUserResult result)
    {
        var text = Login.text;
        var user = text;
        var password = Password.text;
        
        InfoText.color = PositiveColor;
        InfoText.text = _Registered;
        InfoText.gameObject.SetActive(true);
        
        var request = new LoginWithPlayFabRequest
        {
            Username = user,
            Password = password,
            TitleId = PlayFabSettings.TitleId
        };

        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginResult, OnLoginError);
    }
        
    private void OnLoginResult(LoginResult result) //LoginResult
    {
        InfoText.color = PositiveColor;
        InfoText.text = $"{_LoggedIn} <b>{Login.text}</b>";
        InfoText.gameObject.SetActive(true);
        PlayerPrefs.SetString("Nick", Login.text);
        Tween.Delay(1.5f, LoadGame);
    }

    private void OnLoginError(PlayFabError error)
    {
        if (error.Error == PlayFabErrorCode.InvalidParams && error.ErrorDetails.ContainsKey("Password"))
        {
            InfoText.color = ErrorColor;
            InfoText.text = _WrongPassword;
            InfoText.gameObject.SetActive(true);
            Tween.Delay(1.5f, () => { InfoText.gameObject.SetActive(false); });
        }
        else if (error.Error == PlayFabErrorCode.InvalidParams && error.ErrorDetails.ContainsKey("Username") ||
                 error.Error == PlayFabErrorCode.InvalidUsername || error.Error == PlayFabErrorCode.AccountNotFound)
        {
            var request = new RegisterPlayFabUserRequest
            {
                TitleId = PlayFabSettings.TitleId,
                Username = Login.text,
                Password = Password.text,
                RequireBothUsernameAndEmail = false
            };
            PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterResult, OnLoginError);   
        }
        else if (error.Error == PlayFabErrorCode.AccountBanned)
        {
            Application.Quit();
        }
        else
        {
            InfoText.color = ErrorColor;
            InfoText.text = $"Error {error.HttpCode}: {error.ErrorMessage}";
            InfoText.gameObject.SetActive(true);
            Tween.Delay(1.5f, () => { InfoText.gameObject.SetActive(false); });
        }
    }
}
