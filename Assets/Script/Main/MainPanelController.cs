using System;
using System.Collections.Generic;
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
    [SerializeField] private Button rankingButton;
    [SerializeField] private Button gameStartButton;
    
    private int _selectedImageIndex;
    
    private void Awake()
    {
        // 만약 할당 안되어있으면 추가
        if (signOutButton != null)
        {
            signOutButton.onClick.AddListener(OnSignOutClick);
        }
        
        gameStartButton.GetComponent<SingleInteractableButtonHandler>().ResetButton();
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

            GameManager.Instance.panelManager.OpenSigninPanel();
            
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
            GameManager.Instance.panelManager.OpenConfirmPanel("로그아웃을 실패했습니다.", () =>
            {
                signOutButton.interactable = true; // 실패 시 다시 활성화
            });
        });
    }
    
    public void OnLeaderboardButtonClick()
    {
        GameManager.Instance.panelManager.OpenRankingPanel();  // GameManager를 통해 리더보드 열기
    }
    
    //대국 시작 버튼 클릭
    public void OnClickGameStart()
    {
        //코인 차감 후 게임 씬 로드
        GameManager.Instance.panelManager.RemoveCoinsPanelUI((() => 
        {
            GameManager.Instance.ChangeToGameScene(Enums.GameType.MultiPlay);
        }));
    }
    
    //상점 패널 생성
    public void OnShopButtonClick()
    {
        List<ShopItem> shopItems = new List<ShopItem>();       //상점 데이터 리스트 생성
        for (int i = 0; i < 5; i++)
        {
            if (i == 0)     //광고 항목
            {
                ShopItem shopItem = new ShopItem
                {
                    name = "광고) 코인500개 ",
                    price = 0
                };
                shopItems.Add(shopItem);
            }
            else
            {
                ShopItem shopItem = new ShopItem
                {
                    name = i*1000+"개 ",
                    price = i * 1000
                };
                shopItems.Add(shopItem);
            }
        }
        GameManager.Instance.panelManager.OpenShopPanel(shopItems);
    }

    public void OpenReplayButtonClick()
    {
        GameManager.Instance.panelManager.OpenReplayPanel();
    }
    
    public void OpenSettingButtonClick()
    {
        GameManager.Instance.panelManager.OpenSettingsPanel();
    }
}
