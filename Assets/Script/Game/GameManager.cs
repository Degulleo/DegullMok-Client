using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : Singleton<GameManager>
{
    private Enums.GameType _gameType;
    private GameLogic _gameLogic;
    private StoneController _stoneController;
    private Canvas _canvas;

    private void Awake()
    {
        //게임 씬에서 확인하기 위한 임시 코드
        _gameType = Enums.GameType.SinglePlay;
    }

    private void Start()
    {
        //게임 씬에서 확인하기 위한 임시 코드
        _stoneController = GameObject.FindObjectOfType<StoneController>();
        _stoneController.InitStones();
        _gameLogic = new GameLogic(_stoneController, _gameType);
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
}
