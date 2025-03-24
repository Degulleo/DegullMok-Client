using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public struct SigninData
{
    public string email;
    public string password;
}

public struct SigninResult
{
    public int result;
    public string nickname;
    public int imageIndex;
    public int rating;
    public int score;
    public int coins;
}

[Serializable]
public struct ScoreInfo
{
    public string email;
    public string nickname;
    public int score;
    public float winRate;
    public int win;
    public int lose;
    public int totalGames;
    public int imageIndex;
}

[Serializable]
public struct Scores
{
    public ScoreInfo[] scores;
}

public class SigninPanelController : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;

    private MainPanelManager mainPanel;

    private void Awake()
    {
        if (mainPanel == null) mainPanel = FindObjectOfType<MainPanelManager>();
    }
    
    public void OnClickSigninButton()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            GameManager.Instance.panelManager.OpenConfirmPanel("입력 내용이 누락되었습니다.", () => {});
            return;
        }

        var signinData = new SigninData { email = email, password = password };
        
        NetworkManager.Instance.Signin(signinData, (signinResult) =>
        {
            // 유저 정보 저장
            UserManager.Instance.SetUserInfo(signinResult);
            
            // 메인 패널 정보 갱신
            mainPanel.UpdateMainPanelUI(GameManager.Instance.panelManager.OpenMainPanel);
            
            Destroy(gameObject);
        }, result =>
        {
            if (result == 0)
            {
                emailInputField.text = "";
            }
            else if (result == 1)
            {
                passwordInputField.text = "";
            }
        });
    }

    public void OnClickSignupButton()
    {
        emailInputField.text = "";
        passwordInputField.text = "";
        GameManager.Instance.panelManager.OpenSignupPanel();
    }
}