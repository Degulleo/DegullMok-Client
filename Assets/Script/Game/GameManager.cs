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
    
    [SerializeField] private GameObject panelManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;
    
    [NonSerialized] public PanelManager panelManager;
    [NonSerialized] public AudioManager audioManager;

    protected override void Awake()
    {
        base.Awake();
        InitPanels();
    }
    
    private void Start()
    {
        //게임 씬에서 확인하기 위한 임시 코드
        _gameType = Enums.GameType.SinglePlay;

        //게임 씬에서 확인하기 위한 임시 코드
        // _canvas = canvas.GetComponent<Canvas>();
        // _stoneController = GameObject.FindObjectOfType<StoneController>();
        // _stoneController.InitStones();
        // var fioTimer = FindObjectOfType<FioTimer>();
        // _gameLogic = new GameLogic(_stoneController, _gameType, fioTimer);
    }

    private void InitPanels()
    {
        panelManager = Instantiate(panelManagerPrefab).GetComponent<PanelManager>();
        audioManager = Instantiate(audioManagerPrefab).GetComponent<AudioManager>();
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
            if (_gameType == Enums.GameType.Replay)
            {
                //TODO: 리플레이를 위한 초기화
            }
            _stoneController = GameObject.FindObjectOfType<StoneController>();
            _stoneController.InitStones();
            var fioTimer = FindObjectOfType<FioTimer>();
            _omokBoardImage = GameObject.FindObjectOfType<SpriteRenderer>().gameObject;
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
    
}