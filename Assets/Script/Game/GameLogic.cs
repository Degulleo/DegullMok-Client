using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    private Enums.PlayerType[,] _board;
    private StoneController _stoneController;
    public Enums.PlayerType currentTurn;
    public int selectedRow;
    public int selectedCol;
    
    public GameLogic(StoneController stoneController, Enums.GameType gameType)
    {
        //보드 초기화
        _board = new Enums.PlayerType[15, 15];
        _stoneController = stoneController;
        
        SetTurn(Enums.PlayerType.PlayerA);
    }

    private void SetTurn(Enums.PlayerType player)
    {
        currentTurn = player;
        _stoneController.OnStoneClickedDelegate = (row, col) =>
        {
            if (selectedRow != row || selectedCol != col)
            {
                SetNewStoneState(Enums.StoneState.None,selectedRow, selectedCol);
            }
            selectedRow = row;
            selectedCol = col;
            SetNewStoneState(Enums.StoneState.Selected, row, col);
        };
    }
    
    
    public void SetNewBoardValue(Enums.PlayerType playerType, int row, int col)
    {
        if (playerType == Enums.PlayerType.PlayerA)
        {
            _stoneController.SetStoneType(Enums.StoneType.Black, row, col);
            _stoneController.SetStoneState(Enums.StoneState.LastPositioned, row, col);
            _board[row, col] = Enums.PlayerType.PlayerA;
        }
        if (playerType == Enums.PlayerType.PlayerB)
        {
            _stoneController.SetStoneType(Enums.StoneType.Black, row, col);
            _stoneController.SetStoneState(Enums.StoneState.LastPositioned, row, col);
            _board[row, col] = Enums.PlayerType.PlayerB;
        }
        
    }

    private void SetNewStoneState(Enums.StoneState state, int row, int col)
    {
        _stoneController.SetStoneState(state, row, col);
    }
}
