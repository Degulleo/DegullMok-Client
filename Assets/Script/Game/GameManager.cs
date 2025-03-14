using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject signinPanel;
    [SerializeField] private GameObject signupPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject rankingPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject giboPanel;
    
    [SerializeField] private Canvas canvas;
    private UserManager _userManager;  // UserManager 인스턴스 관리
    
    private Enums.GameType _gameType;
    private GameLogic _gameLogic;
    private StoneController _stoneController;
    private Canvas _canvas;

    public Sprite[] profileSprites; //스크롤 패널에서 사용할 테스트 배열
    
    private void Awake()
    {
        // UserManager가 없으면 생성
        if (UserManager.Instance == null)
        {
            GameObject userManagerObj = new GameObject("UserManager");
            _userManager = userManagerObj.AddComponent<UserManager>();
            
            //게임 씬에서 확인하기 위한 임시 코드
            _gameType = Enums.GameType.SinglePlay;
        }
    }
    
    private void Start()
    {
        // 자동 로그인
        TryAutoSignin();
        
        //게임 씬에서 확인하기 위한 임시 코드
        _stoneController = GameObject.FindObjectOfType<StoneController>();
        _stoneController.InitStones();
        _gameLogic = new GameLogic(_stoneController, _gameType);
    }
    
    private void TryAutoSignin()
    {
        NetworkManager.Instance.GetInfo((userInfo) =>
        {
            Debug.Log("자동 로그인 성공");
            
            UpdateMainPanelUI();
            // ScoreData.SetScore(userInfo.score);
            // OpenConfirmPanel(userInfo.nickname + "님 로그인 성공하였습니다.", () => { });
        }, () =>
        {
            Debug.Log("자동 로그인 실패");
            // 로그인 화면
            OpenSigninPanel();
        });
    }
    
    private void UpdateMainPanelUI()
    {
        MainPanelController mainPanel = FindObjectOfType<MainPanelController>();
        if (mainPanel != null)
        {
            mainPanel.UpdateUserInfo();
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
        _gameLogic.SetNewBoardValue(_gameLogic.currentTurn, _gameLogic.selectedRow,_gameLogic.selectedCol);
    }
    
    private void ChangeToGameScene(Enums.GameType gameType)
    {
        _gameType = gameType;
        SceneManager.LoadScene("Game");
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            _stoneController = GameObject.FindObjectOfType<StoneController>();
            _stoneController.InitStones();
            _gameLogic = new GameLogic(_stoneController, _gameType);
        }
        _canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
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