using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

    private bool _emailValid = false;

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
        // 현재 토글을 끄려고 할 때 (value가 false)
        if (!value && index == _selectedImageIndex)
        {
            // 현재 켜져 있는 토글을 다시 켜지 않게 하고
            imageSelectToggles[index].onValueChanged.RemoveAllListeners();

            // 다른 토글을 켬 (현재 선택된 인덱스가 아닌 다음 토글을 켬)
            int nextIndex = (index + 1) % imageSelectToggles.Length;
            _selectedImageIndex = nextIndex;
            imageSelectToggles[nextIndex].isOn = true;

            // 이벤트 리스너 다시 추가
            int capturedIndex = index;
            imageSelectToggles[index].onValueChanged.AddListener((bool val) => OnValueChanged(val, capturedIndex));
            return;
        }

        // 새로운 토글을 선택했을 때
        if (value)
        {
            int previousIndex = _selectedImageIndex;
            _selectedImageIndex = index;

            // 선택된 토글이 변경되었을 때만 이전 토글을 끔
            if (previousIndex != index)
            {
                imageSelectToggles[previousIndex].isOn = false;
            }
        }
    }

    public void OnClickConfirmButton()
    {
        var email = emailInputField.text;
        var nickname = nicknameInputField.text;
        var password = passwordInputField.text;
        var confirmPassword = confirmPasswordInputField.text;

        if (string.IsNullOrEmpty(email) || !_emailValid || string.IsNullOrEmpty(nickname) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            // 입력 내용 누락 팝업 표시
            GameManager.Instance.panelManager.OpenConfirmPanel("입력 내용이 누락되었습니다.", () => {});
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
                GameManager.Instance.panelManager.OpenConfirmPanel("회원가입에 실패했습니다.", () =>
                {
                    emailInputField.text = "";
                    nicknameInputField.text = "";
                    passwordInputField.text = "";
                    confirmPasswordInputField.text = "";
                });
            });
        }
        else
        {
            // 비밀번호 오류 팝업 표시
            Debug.Log("비밀번호가 서로 다릅니다.");
            GameManager.Instance.panelManager.OpenConfirmPanel("비밀번호가 서로 다릅니다.", () =>
            {
                passwordInputField.text = "";
                confirmPasswordInputField.text = "";
            });
        }
    }

    public void OnClickCancelButton()
    {
        Destroy(gameObject);
    }

    // 이메일 입력을 완료하면 유효성 검사를 진행함
    public void OnChangeEndEmail(string emailText)
    {
        const string emailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

        // 입력이 끝났을 때 이메일 형식 검사
        _emailValid = Regex.IsMatch(emailText, emailPattern);

        // 이메일이 비어있지 않은 경우에만 색상 변경
        if (!string.IsNullOrEmpty(emailText))
        {
            if (_emailValid) return;
            emailInputField.textComponent.color = Color.red;

            // 이메일 유효하지 않음
            GameManager.Instance.panelManager.OpenConfirmPanel("올바른 이메일 주소를 입력하세요.", () => {});
        }

    }

    // 이메일 인풋을 선택했을 때 이메일이 유효하지 않았으면 텍스트를 초기화함
    public void OnSelectEmail()
    {
        if (!_emailValid)
        {
            emailInputField.textComponent.color = Color.black;
            emailInputField.text = String.Empty;
        }
    }

    // 닉네임 글자 6자 초과하면 마지막 글자를 잘라서 6자로 만듬
    public void OnChangeNickname(string nicknameText)
    {
        const int maxNicknameLength = 6;

        if (nicknameText.Length > maxNicknameLength)
        {
            // 글자수가 제한을 초과하면 처음 6글자만 남김
            string limitedText = nicknameText.Substring(0, maxNicknameLength);
            nicknameInputField.text = limitedText;
        }
    }

    // 비밀번호 유효성 검사
    public void OnChangeEndPassword(string passwordText)
    {
        // 비밀번호가 비어 있으면 검사하지 않음
        if (string.IsNullOrEmpty(passwordText)) return;

        // 비밀번호 글자수 제한 확인
        if (passwordText.Length < 6 || passwordText.Length > 18)
        {
            // 비밀번호 글자 수 제한
            GameManager.Instance.panelManager.OpenConfirmPanel("비밀번호는 6자 이상 18자 이하로 입력해주세요.", () => {});
        }
    }
}
