using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainPanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI ratingText;
    [SerializeField] private Button signOutButton;
    [SerializeField] private GameObject[] profileImages;

    private int _selectedImageIndex;
    
    private void Awake()
    {
        // 만약 할당 안되어있으면 추가
        if (signOutButton != null)
        {
            signOutButton.onClick.AddListener(OnSignOutClick);
        }
    }

    public void UpdateUserInfo()
    {
        if (UserManager.Instance == null) return;

        nicknameText.text = UserManager.Instance.Nickname;
        ratingText.text = $"{UserManager.Instance.Rating}급";

        // 프로필 이미지 갱신
        _selectedImageIndex = UserManager.Instance.imageIndex;
        if (_selectedImageIndex < 0 || _selectedImageIndex >= profileImages.Length)
        {
            return;
        }

        // 모든 프로필 이미지 비활성화 후, 선택한 이미지만 활성화
        foreach (var img in profileImages)
        {
            img.SetActive(false);
        }
        profileImages[_selectedImageIndex].SetActive(true);
    }

    public void OnSignOutClick()
    {
        signOutButton.interactable = false; // 중복 클릭 방지
        NetworkManager.Instance.SignOut(() =>
        {
            Debug.Log("로그아웃 성공");

            GameManager.Instance.OpenSigninPanel();
            
            // 로그아웃 버튼 메서드 삭제
            signOutButton.onClick.RemoveAllListeners();

            // 아직 메인 패널이 살아있다면 삭제
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }, () =>
        {
            Debug.Log("로그아웃 실패");
            // 입력 내용 누락 팝업 표시
            GameManager.Instance.OpenConfirmPanel("로그아웃을 실패했습니다.", () =>
            {
                signOutButton.interactable = true; // 실패 시 다시 활성화
            });
        });
    }
    
    //대국 시작 버튼 클릭
    public void OnClickGameStart()
    {
        GameManager.Instance.ChangeToGameScene(Enums.GameType.SinglePlay);
    }
}
