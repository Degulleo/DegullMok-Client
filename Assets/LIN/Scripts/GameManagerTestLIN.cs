using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManagerTestLIN : Singleton<GameManagerTestLIN>
{
    [SerializeField] private GameObject signinPanel;
    [SerializeField] private GameObject signupPanel;
    
    [SerializeField] private Canvas canvas;
    private UserManager _userManager;  // UserManager 인스턴스 관리
    
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
            
            //TODO: 게임 내에서 기보 타입 적용하기
            _gameType = Enums.GameType.Replay;
        }
    }
    
    private void Start()
    {
        //TODO: 기보 타입으로 들어왔을 때 데이터 로드 테스트 수정할것
        ReplayManager.Instance.InitReplayBoard(ReplayManager.Instance.LoadReplayDatas()[9]);
        
        
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

    public void OnClickReplayNextButton()
    {
        Move nextMove = ReplayManager.Instance.GetNextMove();
        if (nextMove != null)
        {
            if (nextMove.stoneType.Equals(Enums.StoneType.Black.ToString()))
            {
                _gameLogic.SetNewBoardValue(Enums.PlayerType.PlayerA, nextMove.columnIndex, nextMove.rowIndex);

            }
            else if (nextMove.stoneType.Equals(Enums.StoneType.White.ToString()))
            {
                _gameLogic.SetNewBoardValue(Enums.PlayerType.PlayerB, nextMove.columnIndex, nextMove.rowIndex);
            }
        }
    }

    public void OnClickReplayUndoButton()
    {
        
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
}