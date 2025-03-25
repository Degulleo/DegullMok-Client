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
    private GameObject _omokBoardImage;
    private GameUIController _gameUIController;
    
    [SerializeField] private GameObject panelManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;
    
    [NonSerialized] public PanelManager panelManager;
    [NonSerialized] public AudioManager audioManager;

    protected override void Awake()
    {
        base.Awake();
        InitPanels();
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
        if (_gameLogic.selectedRow != -1 && _gameLogic.selectedCol != -1)
        {
            _gameLogic.OnConfirm();
        }
        else
        {
            if (_stoneController != null && _omokBoardImage != null)
            {
                _stoneController.GetComponent<Transform>().DOShakePosition(0.5f, 0.5f);
                _omokBoardImage.GetComponent<Transform>().DOShakePosition(0.5f, 0.5f);
            }
        }
    }
    
    public void ChangeToGameScene(Enums.GameType gameType)
    {
        _gameType = gameType;
        SceneManager.LoadScene("Game");
    }

    public void ChangeToMainScene()
    {
        _gameType = Enums.GameType.None;
        SceneManager.LoadScene("Main");
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            _stoneController = GameObject.FindObjectOfType<StoneController>();
            _stoneController.InitStones();
            var fioTimer = FindObjectOfType<FioTimer>();
            _omokBoardImage = GameObject.FindObjectOfType<SpriteRenderer>().gameObject;
            _gameUIController = GameObject.FindObjectOfType<GameUIController>();
            _gameLogic = new GameLogic(_stoneController, _gameType, fioTimer);
            
        }
        InitPanels();
    }
    //임시 재시작 재대결
    public void RetryGame()
    {
        _gameLogic.ResetBoard();
        _stoneController.InitStones();
        _gameLogic.SetState(_gameLogic.firstPlayerState);
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
}