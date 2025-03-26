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
    private GameObject loadingPanelObject;
    
    private Dictionary<string, GameObject> panelPrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> effectPanelPrefabs = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        SetCanvas();
        // Prefabs 폴더에서 모든 패널 프리팹 로드
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/Panels");

        foreach (GameObject prefab in prefabs)
        {
            panelPrefabs[prefab.name] = prefab;
        }
        
        //게임결과 이펙트 패널
        GameObject[] effectPrefabs = Resources.LoadAll<GameObject>("Prefabs/Effects");
        foreach (GameObject effect in effectPrefabs)
        {
            effectPanelPrefabs[effect.name] = effect;
        }

        Debug.Log($"총 {panelPrefabs.Count}개의 패널이 로드됨.");
    }
    
    void SetCanvas()
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

    #region 게임결과 이펙트 패널 관련

    public GameObject GetEffectPanel(string panelName)
    {
        if (effectPanelPrefabs.TryGetValue(panelName, out GameObject prefab))
        {
            return Instantiate(prefab, _canvas.transform);
        }
        else
        {
            Debug.LogError($"패널 '{panelName}'을 찾을 수 없습니다.");
        }

        return null;
    }

    public void OpenEffectPanel(Enums.GameResult gameResult)
    {
        switch (gameResult)
        {
            case Enums.GameResult.Win:
                if (_canvas != null)
                {
                    var winEffectPanelObject = GetEffectPanel("Win Effect Panel");
                    winEffectPanelObject.GetComponent<WinEffectController>().ShowEffect(OnEffectPanelEnded);
                }
                break;
            case Enums.GameResult.Lose:
                if (_canvas != null)
                {
                    var loseEffectPanelObject = GetEffectPanel("Lose Effect Panel");
                    loseEffectPanelObject.GetComponent<LoseEffectController>().ShowEffect(OnEffectPanelEnded);
                }
                break;
            case Enums.GameResult.Draw:
                if (_canvas != null)
                {
                    var drawEffectPanelObject = GetEffectPanel("Draw Effect Panel");
                    drawEffectPanelObject.GetComponent<DrawEffectController>().ShowEffect(OnEffectPanelEnded);
                }
                break;
        }
    }

    private void OnEffectPanelEnded()
    {
        //Todo: 승급패널 오픈
        OpenRatingPanel();
    }
    #endregion
    
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
        SetCanvas();
        if (_canvas != null)
        {
            if (loadingPanelObject != null && loadingPanelObject.activeSelf)
            {
                // 기존 로딩 패널이 활성화되어 있으면 먼저 닫기
                CloseLoadingPanel();
            }
            
            loadingPanelObject = GetPanel("Loading Panel");
            
            // 로딩 화면이 생성된 후, 원하는 애니메이션 활성화
            loadingPanelController = loadingPanelObject.GetComponent<LoadingPanelController>();
            if (loadingPanelController != null)
            {
                loadingPanelController.StartLoading(rotateImage, animatedText, flipImage);
            }
        }
    }

    public void CloseLoadingPanel()
    {
        if (loadingPanelObject != null && loadingPanelObject.activeSelf && loadingPanelController != null)
        {
            loadingPanelController.StopLoading();
        }
        else
        {
            Debug.Log("로딩 패널이 이미 닫혔거나 비활성화 상태입니다.");
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
        }
    }
    
    public void OpenRankingPanel()
    {
        if (_canvas != null)
        {
            var settingsPanelObject = GetPanel("LeaderboardPanel");
            var leaderboardController = settingsPanelObject.GetComponent<LeaderBoardController>();

            if (leaderboardController != null)
            {
                leaderboardController.OnClickLeaderboardButton();  // 이 메서드를 호출하여 데이터를 로드하고 Show()가 실행되도록 합니다.
            }
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
            replayPanelObject.GetComponent<ReplayPanelItemsController>().Show();
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
    
    //승급 패널 생성
    public void OpenRatingPanel()
    {
        if (_canvas != null)
        {
            var replayPanelObject = GetPanel("Rating Panel");
            replayPanelObject.GetComponent<RatingPanelController>().Show();
        }
    }
    
    //코인 패널 코인 갱신
    public void UpdateCoinsPanelUI(int coinsChanged)
    {
        if (_coinsPanel != null)
        {
            _coinsPanel.AddCoins(coinsChanged, () =>
            {
                
            });
        }
        else
        {
            Debug.Log("코인 패널이 null 입니다.");
        }
    }
    
    public void RemoveCoinsPanelUI(Action onComplete)
    {
        NetworkManager.Instance.DeductCoins((i) =>
        {
            //Todo: 감소된 코인 값으로 확장할 기능 추가
            
        }, (failMessage) =>
        {
            Debug.Log(failMessage);
        });
        
        _coinsPanel.RemoveCoins((() =>
        {
            onComplete?.Invoke();
        }));
    }

   
}
