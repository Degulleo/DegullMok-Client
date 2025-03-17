using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TestGameManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField rowText;
    [SerializeField] private TMP_InputField colText;
    [SerializeField] private TMP_Text boardText;

    private Enums.PlayerType[,] _board;

    private void Start()
    {
        _board = new Enums.PlayerType[15, 15];
        MiniMaxAIController.SetLevel(1); // 급수 설정 테스트
        ResultBoard();
    }

    public void OnClickGoButton()
    {
        int row = int.Parse(rowText.text);
        int col = int.Parse(colText.text);

        if (_board[row, col] != Enums.PlayerType.None)
        {
            Debug.Log("중복 위치");
            return;
        }
        
        _board[row, col] = Enums.PlayerType.PlayerA;
        Debug.Log($"Player's row: {row} col: {col}");
        
        // var isEnded = MiniMaxAIController.CheckGameWin(Enums.PlayerType.PlayerA, _board, row, col);
        // Debug.Log("PlayerA is Win: " + isEnded);
        
        // 인공지능 호출
        var result = MiniMaxAIController.GetBestMove(_board);
        
        if (result.HasValue)
        {
            Debug.Log($"AI's row: {result.Value.row} col: {result.Value.col}");
            _board[result.Value.row, result.Value.col] = Enums.PlayerType.PlayerB;
            
            // isEnded = MiniMaxAIController.CheckGameWin(Enums.PlayerType.PlayerB, _board, result.Value.row, result.Value.col);
            // Debug.Log("PlayerB is Win: " + isEnded);
        }
        
        ResultBoard();
    }
    
    private void ResultBoard()
    {
        boardText.text = "";
        
        // player 타입에 따라 입력받는 건 무조건 A로 해서 A, AI는 B로 나타내고 None은 _
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (_board[i, j] == Enums.PlayerType.PlayerA)
                {
                    boardText.text += 'A';
                }
                else if (_board[i, j] == Enums.PlayerType.PlayerB)
                {
                    boardText.text += 'B';
                } 
                else if (_board[i, j] == Enums.PlayerType.None)
                {
                    boardText.text += '_';
                }

                boardText.text += ' ';
            }

            boardText.text += '\n';
        }
    }
}
