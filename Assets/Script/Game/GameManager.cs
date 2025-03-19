using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class GameManager : Singleton<GameManager>
{
    [Header("Panel")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject signinPanel;
    [SerializeField] private GameObject signupPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject rankingPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject giboPanel;
    [SerializeField] private GameObject loadingPanel;
    
    [Header("Sound")]
    [SerializeField] private AudioClip mainBgm;
    private AudioSource audioSource;
    
    private LoadingPanelController loadingPanelController;
    
    private UserManager _userManager;  // UserManager 인스턴스 관리
    private CoinsPanelController _coinsPanel;
    
    
    private Enums.GameType _gameType;
    private GameLogic _gameLogic;
    private StoneController _stoneController;
    private Canvas _canvas;
    
    private void Start()
    {
        // TODO: 음악 관련은 AuidoManager로 분리?
        PlayMainBGM();
            
        // UserManager가 없으면 생성
        if (UserManager.Instance == null)
        {
            GameObject userManagerObj = new GameObject("UserManager");
            _userManager = userManagerObj.AddComponent<UserManager>();
        }
        //
        // //게임 씬에서 확인하기 위한 임시 코드
        // _gameType = Enums.GameType.SinglePlay;
        //
        // if (_canvas == null)
        // {
        //     _canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        // }
        // // 로딩 화면 추가(자동 로그인 응답 전까지)
        // OpenLoadingPanel(false, false, true);
        //
        // // 자동 로그인
        // TryAutoSignin();

        //게임 씬에서 확인하기 위한 임시 코드
        // _canvas = canvas.GetComponent<Canvas>();
        // _stoneController = GameObject.FindObjectOfType<StoneController>();
        // _stoneController.InitStones();
        // var fioTimer = FindObjectOfType<FioTimer>();
        // _gameLogic = new GameLogic(_stoneController, _gameType, fioTimer);
    }

    // 배경음악 시작
    public void PlayMainBGM()
    {
        // AudioSource 컴포넌트 가져오기
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null && mainBgm != null)
        {
            // 배경음악이 설정되면 재생
            audioSource.clip = mainBgm; // 음악 클립 설정
            audioSource.loop = true; // 반복 재생
            audioSource.volume = 0.4f; // 볼륨
            audioSource.Play(); // 음악 시작
        }
    }

    // 배경음악 멈추기
    public void StopMainBGM()
    {
        if (audioSource != null)
        {
            audioSource.Stop(); // 배경음악 멈추기
        }
    }
    
    private void TryAutoSignin()
    {
        NetworkManager.Instance.GetInfo((userInfo) =>
        {
            Debug.Log("자동 로그인 성공");
            
            UserManager.Instance.SetUserInfo(userInfo);
            
            UpdateMainPanelUI(OpenMainPanel);
            // ScoreData.SetScore(userInfo.score);
            OpenConfirmPanel(userInfo.nickname + "님" + "\n" + "자동 로그인 되었습니다", () => { });
            
            loadingPanelController.StopLoading();
        }, () =>
        {
            Debug.Log("자동 로그인 실패");
            // 로딩 멈추기
            loadingPanelController.StopLoading();
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
        if (_canvas != null)
        {
            var mainPanelObject = Instantiate(mainPanel, _canvas.transform);
            
            // 메인 화면 아래의 코인 패널에 서버에서 가져 온 코인 값 업데이트
            _coinsPanel = mainPanelObject.GetComponentInChildren<CoinsPanelController>();
            
            if (_coinsPanel != null)
            {
                _coinsPanel.InitCoinsCount(UserManager.Instance.Coins);
            }

        }
    }

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
    
    public void OpenLoadingPanel(bool rotateImage = false, bool animatedText = false, bool flipImage = false)
    {
        if (_canvas != null)
        {
            var loadingPanelObject = Instantiate(loadingPanel, _canvas.transform);
        
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
            var signinPanelObject = Instantiate(signinPanel, _canvas.transform);
        }
    }

    public void OpenSignupPanel()
    {
        if (_canvas != null)
        {
            var signupPanelObject = Instantiate(signupPanel, _canvas.transform);
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
            settingsPanelObject.GetComponent<ReplayPanelController>().Show();
        }
    }

}