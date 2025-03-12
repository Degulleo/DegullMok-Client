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
}

public struct ScoreResult
{
    public string id;
    public string email;
    public string nickname;
    public int score;
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
    [SerializeField] private TMP_InputField _emailInputField;
    [SerializeField] private TMP_InputField _passwordInputField;

    public void OnClickSigninButton()
    {
        string email = _emailInputField.text;
        string password = _passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            // TODO: 누락된 값 입렵 요청 팝업 표시
            return;
        }

        var signinData = new SigninData();
        signinData.email = email;
        signinData.password = password;
        
        StartCoroutine(NetworkManager.Instance.Signin(signinData, () =>
        {
            Destroy(gameObject);
        }, result =>
        {
            if (result == 0)
            {
                _emailInputField.text = "";
            }
            else if (result == 1)
            {
                _passwordInputField.text = "";
            }
        }));
    }

    public void OnClickSignupButton()
    {
        _emailInputField.text = "";
        _passwordInputField.text = "";
        GameManager.Instance.OpenSignupPanel();
    }
}