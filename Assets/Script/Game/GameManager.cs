using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

public class GameManager : Singleton<GameManager>
{
    private Enums.GameType _gameType;
    private GameLogic _gameLogic;
    private StoneController _stoneController;
    private GameObject _camera;
    private GameUIController _gameUIController;
    
    [SerializeField] private GameObject panelManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;
    
    [NonSerialized] public PanelManager panelManager;
    [NonSerialized] public AudioManager audioManager;
    
    private MultiplayManager _multiplayManager;

    protected override void Awake()
    {
        base.Awake();
        InitPanels();
    }
    
    public MultiplayManager GetMultiplayManager()
    {
        _multiplayManager = _gameLogic.MultiPlayManager;
        if (_multiplayManager == null) Debug.Log("MultiplayManager가 null입니다");
        return _multiplayManager;
    }
    
    private void InitPanels()
    {
        if (panelManager == null)
        {
            panelManager = Instantiate(panelManagerPrefab).GetComponent<PanelManager>();
        }
        if (audioManager == null)
        {
            audioManager = Instantiate(audioManagerPrefab).GetComponent<AudioManager>();
        }
    }
    
    public void OnClickConfirmButton()
    {
        if (_gameLogic.SelectedRow != -1 && _gameLogic.SelectedCol != -1)
        {
            _gameLogic.OnConfirm();
        }
        else
        {
            if (_camera != null)
            {
                _camera.transform.DOShakePosition(0.5f, 0.5f).OnComplete(() =>
                {
                    _camera.transform.position = new Vector3(0,0,-10);
                });
            }
        }
    }
    
    // 멀티 플레이를 위한 코드
    public void ChangeToGameScene(Enums.GameType gameType)
    {
        _gameType = gameType;
        SceneManager.LoadScene("Game");
    }

    public void ChangeToMainScene()
    {
        _gameType = Enums.GameType.None;
        // TODO: 추후 혹시 모를 존재하는 socket 통신 종료 필요 - _gameLogic?.Dispose에서 LeaveRoom 호출하긴 하는데 서버에서 이미 해당 방을 삭제했을 경우 동작 확인 필요
        // _gameLogic?.Dispose();
        SceneManager.LoadScene("Main");
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            _stoneController = GameObject.FindObjectOfType<StoneController>();
            _stoneController.InitStones();
            var fioTimer = FindObjectOfType<FioTimer>();
            _camera = GameObject.FindObjectOfType<Camera>().gameObject;
            _gameUIController = GameObject.FindObjectOfType<GameUIController>();
            _gameLogic = new GameLogic(_stoneController, _gameType, fioTimer);
        }
        InitPanels();
    }
    //임시 재시작 재대결
    public void RetryGame()
    {
        if (_gameLogic == null) return;
        _gameLogic.ResetBoard();
        _stoneController.InitStones();
        _gameLogic.SetState(_gameLogic.FirstPlayerState);
    }
    //유저 이름 Game UI에 초기화
    public void InitPlayersName(string playerNameA, string playerNameB)
    {
        if (_gameUIController == null) return;
        _gameUIController.InitPlayersName(playerNameA, playerNameB);
    }
    //유저 프로필 이미지 Game UI에 초기화
    public void InitProfileImages(int profileImageIndexA, int profileImageIndexB)
    {
        if (_gameUIController == null) return;
        _gameUIController.InitProfileImages(profileImageIndexA, profileImageIndexB);
    }
    
    public void SetTurnIndicator(bool isFirstPlayer)
    {
        if (_gameUIController == null) return;
        _gameUIController.SetTurnIndicator(isFirstPlayer);
    }

    public bool GetRequestDrawChance()
    {
        if (_gameLogic == null) return false;
        return _gameLogic.RequestDrawChance;
    }

    public void SetRequestDrawChanceFalse()
    {
        if (_gameLogic == null) return;
        _gameLogic.RequestDrawChance = false;
    }

    public bool CheckIsSinglePlay()
    {
        if (_gameLogic == null) return false;
        return _gameLogic.GameType == Enums.GameType.SinglePlay;
    }

    public void SurrenderSinglePlay()
    {
        if(_gameLogic == null) return;
        panelManager.OpenEffectPanel(Enums.GameResult.Lose);
        _gameLogic.EndGame(Enums.GameResult.Lose);
    }

    public void DrawSinglePlay()
    {
        if(_gameLogic == null) return;
        panelManager.OpenEffectPanel(Enums.GameResult.Draw);
        _gameLogic.EndGame(Enums.GameResult.Draw);
    }

    
    private void OnApplicationQuit()
    {
        Debug.Log("앱 종료 감지: 소켓 연결 정리 중...");
        
        if(_gameLogic.GameInProgress)
            _gameLogic?.ForceQuit();
        else 
            _gameLogic?.Dispose();
        
    }
}