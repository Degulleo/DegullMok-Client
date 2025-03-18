using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject signinPanel;
    [SerializeField] private GameObject signupPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject rankingPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject giboPanel;
    
    [SerializeField] private Canvas canvas;
    private UserManager _userManager;  // UserManager 인스턴스 관리
    private CoinsPanelController _coinsPanel;
    
    
    private Enums.GameType _gameType;
    private GameLogic _gameLogic;
    private StoneController _stoneController;
    private Canvas _canvas;

    public Sprite[] profileSprites; //패널에서 사용할 테스트 배열
    
    private void Awake()
    {
        // UserManager가 없으면 생성
        if (UserManager.Instance == null)
        {
            GameObject userManagerObj = new GameObject("UserManager");
            _userManager = userManagerObj.AddComponent<UserManager>();
        }
        //게임 씬에서 확인하기 위한 임시 코드
        _gameType = Enums.GameType.SinglePlay;
    }
    
    private void Start()
    {
        // TODO: 로딩 화면 추가(자동 로그인 응답 전까지)
        
        // 자동 로그인
        // TryAutoSignin();
        
        //게임 씬에서 확인하기 위한 임시 코드
        _canvas = canvas.GetComponent<Canvas>();
        _stoneController = GameObject.FindObjectOfType<StoneController>();
        _stoneController.InitStones();
        var fioTimer = FindObjectOfType<FioTimer>();
        _gameLogic = new GameLogic(_stoneController, _gameType, fioTimer);
    }
    
    private void TryAutoSignin()
    {
        NetworkManager.Instance.GetInfo((userInfo) =>
        {
            Debug.Log("자동 로그인 성공");
            UserManager.Instance.SetUserInfo(userInfo);

            UpdateMainPanelUI(OpenMainPanel);
            // ScoreData.SetScore(userInfo.score);
            // OpenConfirmPanel(userInfo.nickname + "님 로그인 성공하였습니다.", () => { });
        }, () =>
        {
            Debug.Log("자동 로그인 실패");
            // 로그인 화면
            OpenSigninPanel();
        });
    }
    
    /// <summary>
    /// 유저 별명, 급수를 서버에서 가져온 정보로 업데이트하여 메인화면에 표시
    /// </summary>
    public void UpdateMainPanelUI(Action success = null)
    {
        MainPanelController mainPanelController = mainPanel.GetComponent<MainPanelController>();
        
        if (mainPanelController == null) return;
        
        mainPanelController.UpdateUserInfo();

        success?.Invoke();
    }
    
    public void OpenMainPanel()
    {
        if (canvas != null)
        {
            var mainPanelObject = Instantiate(mainPanel, canvas.transform);
            
            // 메인 화면 아래의 코인 패널에 서버에서 가져 온 코인 값 업데이트
            _coinsPanel = mainPanelObject.GetComponentInChildren<CoinsPanelController>();
            
            if (_coinsPanel != null)
            {
                _coinsPanel.InitCoinsCount(UserManager.Instance.Coins);
            }

        }
    }
    
    public void OpenSigninPanel()
    {
        if (canvas != null)
        {
            var signinPanelObject = Instantiate(signinPanel, canvas.transform);
        }
    }

    public void OpenSignupPanel()
    {
        if (canvas != null)
        {
            var signupPanelObject = Instantiate(signupPanel, canvas.transform);
        }
    }
    
    public void OnClickConfirmButton()
    {
        if (_gameLogic.selectedRow != -1 && _gameLogic.selectedCol != -1)
        {
            _gameLogic.OnConfirm();
        }
        else
        {
            Debug.Log("착수 위치를 선택 해주세요");
            //TODO: 착수할 위치를 선택하라는 동작
        }
    }
    
    public void ChangeToGameScene(Enums.GameType gameType)
    {
        _gameType = gameType;
        SceneManager.LoadScene("Game");
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            if (_gameType == Enums.GameType.Replay)
            {
                //TODO: 리플레이를 위한 초기화
            }
            _stoneController = GameObject.FindObjectOfType<StoneController>();
            _stoneController.InitStones();
            var fioTimer = FindObjectOfType<FioTimer>();
            _gameLogic = new GameLogic(_stoneController, _gameType, fioTimer);
        }
        _canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
    }
    //임시 재시작 재대결
    public void RetryGame()
    {
        _gameLogic.ResetBoard();
        _stoneController.InitStones();
        _gameLogic.SetState(_gameLogic.firstPlayerState);
    }
    public void OpenConfirmPanel(string message, ConfirmPanelController.OnConfirmButtonClick onConfirmButtonClick)
    {
        if (_canvas != null)
        {
            var confirmPanelObject = Instantiate(confirmPanel, _canvas.transform);
            confirmPanelObject.GetComponent<ConfirmPanelController>()
                .Show(message, onConfirmButtonClick);
        }
    }
    
    public void OpenSettingsPanel()
    {
        if (_canvas != null)
        {
            var settingsPanelObject = Instantiate(settingsPanel, _canvas.transform);
            settingsPanelObject.GetComponent<PanelController>().Show();
        }
    }
    
    public void OpenRankingPanel(List<RankingItem> rankingItems)
    {
        if (_canvas != null)
        {
            var settingsPanelObject = Instantiate(rankingPanel, _canvas.transform);
            settingsPanelObject.GetComponent<RankingPanelController>().Show(rankingItems);
        }
    }
    
    public void OpenShopPanel(List<ShopItem> shopItems)
    {
        if (_canvas != null)
        {
            var settingsPanelObject = Instantiate(shopPanel, _canvas.transform);
            settingsPanelObject.GetComponent<ShopPanelController>().Show(shopItems);
        }
    }
    
    public void OpenGiboPanel(List<GiboItem> giboItems)
    {
        if (_canvas != null)
        {
            var settingsPanelObject = Instantiate(giboPanel, _canvas.transform);
            settingsPanelObject.GetComponent<GiboPanelController>().Show(giboItems);
        }
    }
}