using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 패널 생성 테스트 코드
/// 버튼을 누르면 팝업 생성
/// 상점, 랭크, 기보 패널은 각 데이터타입 리스트를 전달해야함
/// </summary>
public class PanelManager : MonoBehaviour
{
    private Canvas _canvas;
    private CoinsPanelController _coinsPanel;
    private LoadingPanelController loadingPanelController;
    
    private Dictionary<string, GameObject> panelPrefabs = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        // Prefabs 폴더에서 모든 패널 프리팹 로드
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/Panels");

        foreach (GameObject prefab in prefabs)
        {
            panelPrefabs[prefab.name] = prefab;
        }

        Debug.Log($"총 {panelPrefabs.Count}개의 패널이 로드됨.");
    }
    
    void Start()
    {
        if (_canvas == null)
        {
            _canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        }
    }

    public GameObject GetPanel(string panelName)
    {
        if (panelPrefabs.TryGetValue(panelName, out GameObject prefab))
        {
            return Instantiate(prefab, _canvas.transform);
        }
        else
        {
            Debug.LogError($"패널 '{panelName}'을 찾을 수 없습니다.");
        }

        return null;
    }
    
    public void OpenMainPanel()
    {
        if (_canvas != null)
        {
            var mainPanelObject = GetPanel("Main Panel");
            
            // 메인 화면 아래의 코인 패널에 서버에서 가져 온 코인 값 업데이트
            _coinsPanel = mainPanelObject.GetComponentInChildren<CoinsPanelController>();
            
            if (_coinsPanel != null)
            {
                _coinsPanel.InitCoinsCount(UserManager.Instance.Coins);
            }
        }
    }

    public void OpenLoadingPanel(bool rotateImage = false, bool animatedText = false, bool flipImage = false)
    {
        if (_canvas != null)
        {
            var loadingPanelObject = GetPanel("Loading Panel");
            
        
            // 로딩 화면이 생성된 후, 원하는 애니메이션 활성화
            loadingPanelController = loadingPanelObject.GetComponent<LoadingPanelController>();
            if (loadingPanelController != null)
            {
                loadingPanelController.StartLoading(rotateImage, animatedText, flipImage);
            }
        }
    }
    
    public void OpenSigninPanel()
    {
        if (_canvas != null)
        {
            var signinPanelObject = GetPanel("Signin Panel");
        }
    }

    public void OpenSignupPanel()
    {
        if (_canvas != null)
        {
            var signupPanelObject = GetPanel("Signup Panel");
        }
    }
    public void OpenConfirmPanel(string message, ConfirmPanelController.OnConfirmButtonClick onConfirmButtonClick)
    {
        if (_canvas != null)
        {
            var confirmPanelObject = GetPanel("Confirm Panel");
            confirmPanelObject.GetComponent<ConfirmPanelController>()
                .Show(message, onConfirmButtonClick);
        }
    }
    
    public void OpenSettingsPanel()
    {
        if (_canvas != null)
        {
            var settingsPanelObject = GetPanel("Setting Panel");
            settingsPanelObject.GetComponent<PanelController>().Show();
        }
    }
    
    public void OpenRankingPanel(List<RankingItem> rankingItems)
    {
        if (_canvas != null)
        {
            var settingsPanelObject = GetPanel("Ranking Panel");
            settingsPanelObject.GetComponent<RankingPanelController>().Show(rankingItems);
        }
    }
    
    public void OpenShopPanel(List<ShopItem> shopItems)
    {
        if (_canvas != null)
        {
            var shopPanelObject = GetPanel("Shop Panel");
            shopPanelObject.GetComponent<ShopPanelController>().Show(shopItems);
        }
    }
    
    public void OpenReplayPanel()
    {
        if (_canvas != null)
        {
            var replayPanelObject = GetPanel("Replay Panel");
            replayPanelObject.GetComponent<ReplayPanelController>().Show();
        }
    }
    
    //확인 패널 생성
    public void OnConfirmPanelClick()
    {
        OpenConfirmPanel("확인 패널 입니다.", () =>
        {
                Debug.Log("확인 버튼을 누르셨습니다.");
        });
        return;
    }
    
    //랭킹 패널 생성
    public void OnRankingPanelClick()
    {
        List<RankingItem> rankingItems = new List<RankingItem>();       //테스트 데이터 리스트 생성
        for (int i = 0; i < 30; i++)
        {
            RankingItem rankingItem = new RankingItem
            {
                ProfileSpriteIndex = Random.Range(0, 2),
                Name = i.ToString(),
                WinRate = Random.Range(0f, 1f)
            };
            rankingItems.Add(rankingItem);
        }
        
        OpenRankingPanel(rankingItems);
    }
    
    //상점 패널 생성
    public void OnShopPanelClick()
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
    
    //코인 패널 코인 갱신
    public void UpdateCoinsPanelUI(int coinsChanged, CanvasGroup shopPanel)
    {
        if (_coinsPanel != null)
        {
            _coinsPanel.AddCoins(coinsChanged, shopPanel, () =>
            {
                
            });
        }
        else
        {
            Debug.Log("코인 패널이 null 입니다.");
        }
    }

   
}
