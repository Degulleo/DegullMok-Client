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

    public void OnClickSigninButton()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            // TODO: 누락된 값 입력 요청 팝업 표시
            return;
        }

        var signinData = new SigninData { email = email, password = password };
        
        NetworkManager.Instance.Signin(signinData, (signinResult) =>
        {
            Destroy(gameObject);
            
            // 유저 정보 저장
            UserManager.Instance.SetUserInfo(signinResult);
            
            // 메인 패널 정보 갱신
            GameManager.Instance.UpdateMainPanelUI(GameManager.Instance.OpenMainPanel);
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
        GameManager.Instance.OpenSignupPanel();
    }
}