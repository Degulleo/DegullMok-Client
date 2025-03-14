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
    public int imageIndex;
}

public class SignupPanelController : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField confirmPasswordInputField;
    [SerializeField] private Toggle[] imageSelectToggles;
    private int _selectedImageIndex = 0;
    
    private void Start()
    {
        SetToggleInit();
    }

    private void SetToggleInit()
    {
        // 각 프로필 이미지 선택 버튼에 클릭 이벤트 추가
        for (int i = 0; i < imageSelectToggles.Length; i++)
        {
            int index = i; // 클로저 문제 방지
            imageSelectToggles[i].onValueChanged.AddListener((bool value) => OnValueChanged(value, index));
        }
        
        // 선택된 버튼 초기화
        imageSelectToggles[_selectedImageIndex].isOn = true;
    }

    public void OnValueChanged(bool value, int index)
    {
        if (value)
        {
            _selectedImageIndex = index;
            int previousIndex = (_selectedImageIndex == 0) ? 1 : 0;
            imageSelectToggles[previousIndex].isOn = false;
        }
    }

    
    public void OnClickConfirmButton()
    {
        var email = emailInputField.text;
        var nickname = nicknameInputField.text;
        var password = passwordInputField.text;
        var confirmPassword = confirmPasswordInputField.text;

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
            signupData.imageIndex = _selectedImageIndex;
            
            // 서버로 SignupData 전달하면서 회원가입 진행
            NetworkManager.Instance.Signup(signupData, () =>
            {
                Destroy(gameObject);
            }, () =>
            {
                emailInputField.text = "";
                nicknameInputField.text = "";
                passwordInputField.text = "";
                confirmPasswordInputField.text = "";
            });
        }
        else
        {
            // TODO: 비밀번호 오류 팝업 표시
            Debug.Log("비밀번호가 서로 다릅니다.");
            // GameManager.Instance.OpenConfirmPanel("비밀번호가 서로 다릅니다.", () =>
            // {
            //     passwordInputField.text = "";
            //     confirmPasswordInputField.text = "";
            // });
        }
    }

    public void OnClickCancelButton()
    {
        Debug.Log("OnClickCancelButton");
        Destroy(gameObject);
    }
}
