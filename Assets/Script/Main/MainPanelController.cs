using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainPanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;  // 사용자 닉네임 텍스트
    [SerializeField] private TextMeshProUGUI ratingText;  // 사용자 등급 텍스트
    [SerializeField] private Button signOutButton;  // 로그아웃 버튼
    [SerializeField] private Button leaderboardButton;  // 랭킹 버튼
    [SerializeField] private GameObject[] profileImages;  // 프로필 이미지 배열

    private int _selectedImageIndex;
    
    private void Awake()
    {
        // 만약 할당 안 되어 있으면 추가
        if (signOutButton != null)
        {
            signOutButton.onClick.AddListener(OnSignOutClick);
        }

        // 랭킹 버튼 클릭 리스너 추가
        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(OnLeaderboardButtonClick);
        }
    }

    // 사용자 정보 업데이트 (닉네임, 등급, 프로필 이미지)
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

    // 로그아웃 클릭 시 호출되는 메서드
    public void OnSignOutClick()
    {
        signOutButton.interactable = false;  // 중복 클릭 방지
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
            signOutButton.interactable = true;  // 실패 시 다시 활성화
        });
    }

    // 랭킹 버튼 클릭 시 호출되는 메서드
    public void OnLeaderboardButtonClick()
    {
        GameManager.Instance.OpenLeaderboardPanel();  // GameManager를 통해 리더보드 열기
    }
}