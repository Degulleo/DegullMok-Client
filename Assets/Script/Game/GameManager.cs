using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class GameManager : Singleton<GameManager>
{
    private Enums.GameType _gameType;
    private GameLogic _gameLogic;
    private StoneController _stoneController;
    
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
        else if (scene.name == "Replay")
        {
            _stoneController = GameObject.FindObjectOfType<StoneController>();
            _stoneController.InitStones();
            _gameLogic = new GameLogic(_stoneController, Enums.GameType.Replay);
        }
    }
    //임시 재시작 재대결
    public void RetryGame()
    {
        _gameLogic.ResetBoard();
        _stoneController.InitStones();
        _gameLogic.SetState(_gameLogic.firstPlayerState);
    }
    
    #region ReplayControll

    public void ReplayNext(Move nextMove )
    {
        // 보드에 돌을 설정하기 위해 gameLogic의 SetNewBoardValue호출
        if (nextMove.stoneType.Equals(Enums.StoneType.Black.ToString()))
        {
            _gameLogic.SetNewBoardValue(Enums.PlayerType.PlayerA, nextMove.columnIndex, nextMove.rowIndex);

        }
        else if (nextMove.stoneType.Equals(Enums.StoneType.White.ToString()))
        {
            _gameLogic.SetNewBoardValue(Enums.PlayerType.PlayerB, nextMove.columnIndex, nextMove.rowIndex);
        }
        // 돌이 놓인 내역을 ReplayManager에도 반영
        ReplayManager.Instance.PushMove(nextMove);
    }
    
    public void ReplayUndo(Move targetMove)
    {
        if (targetMove.stoneType.Equals(Enums.StoneType.Black.ToString()))
        {
            _gameLogic.SetNewBoardValue(Enums.PlayerType.PlayerA, targetMove.columnIndex, targetMove.rowIndex);

        }
        else if (targetMove.stoneType.Equals(Enums.StoneType.White.ToString()))
        {
            _gameLogic.SetNewBoardValue(Enums.PlayerType.PlayerB, targetMove.columnIndex, targetMove.rowIndex);
        }
        ReplayManager.Instance.PushUndoMove(targetMove);
        //TODO: 화면상에서 돌 치우기
    }
    

    #endregion
}