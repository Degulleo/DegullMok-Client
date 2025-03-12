using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

public struct SignupData
{
    public string email;
    public string nickname;
    public string password;
    public int imageindex;
}

public class SignupPanelController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _emailInputField;
    [SerializeField] private TMP_InputField _nicknameInputField;
    [SerializeField] private TMP_InputField _passwordInputField;
    [SerializeField] private TMP_InputField _confirmPasswordInputField;
    [SerializeField] private Button[] imageSelectButtons;
    private int _selectedImageIndex = 0;
    
    private void Start()
    {
        // 각 프로필 이미지 선택 버튼에 클릭 이벤트 추가
        for (int i = 0; i < imageSelectButtons.Length; i++)
        {
            int index = i; // 클로저 문제 방지
            imageSelectButtons[i].onClick.AddListener(() => SelectImage(index));
        }
    }
    
    private void SelectImage(int index)
        {
            _selectedImageIndex = index;
            Debug.Log($"이미지 {index} 선택됨");
        }
    
    public void OnClickConfirmButton()
    {
        var email = _emailInputField.text;
        var nickname = _nicknameInputField.text;
        var password = _passwordInputField.text;
        var confirmPassword = _confirmPasswordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(nickname) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            // TODO: 입력 내용 누락 팝업 표시
            // GameManager.Instance.OpenConfirmPanel("입력 내용이 누락되었습니다.", () =>
            // {
            //     
            // });
            Debug.Log("입력 내용이 누락되었습니다.");
            return;
        }

        if (password.Equals(confirmPassword))
        {
            SignupData signupData = new SignupData();
            signupData.email = email;
            signupData.nickname = nickname;
            signupData.password = password;
            signupData.imageindex = _selectedImageIndex;
            
            // 서버로 SignupData 전달하면서 회원가입 진행
            StartCoroutine(NetworkManager.Instance.Signup(signupData, () =>
            {
                Destroy(gameObject);
            }, () =>
            {
                _emailInputField.text = "";
                _nicknameInputField.text = "";
                _passwordInputField.text = "";
                _confirmPasswordInputField.text = "";
            }));
        }
        else
        {
            // TODO: 비밀번호 오류 팝업 표시
            Debug.Log("비밀번호가 서로 다릅니다.");
            // GameManager.Instance.OpenConfirmPanel("비밀번호가 서로 다릅니다.", () =>
            // {
            //     _passwordInputField.text = "";
            //     _confirmPasswordInputField.text = "";
            // });
        }
    }

    public void OnClickCancelButton()
    {
        Debug.Log("OnClickCancelButton");
        Destroy(gameObject);
    }
}
