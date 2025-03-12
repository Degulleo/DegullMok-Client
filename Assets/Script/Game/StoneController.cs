using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneController : MonoBehaviour
{
    [SerializeField] private Stone[] stones;
    public delegate void OnStoneClicked(int row, int col);
    public OnStoneClicked OnStoneClickedDelegate;
    //네이밍 필요
    public const int BoardValue = 15;

    //Stone 객체들 초기화 함수 호출
    public void InitStones()
    {
        for (int i = 0; i < stones.Length; i++)
        {
            stones[i].InitStone(i, (index) =>
            {
                var rowIndex = index / BoardValue;
                var colIndex = index % BoardValue;
                OnStoneClickedDelegate?.Invoke(rowIndex, colIndex);
            });
        }
    }

    public void SetStoneType(Enums.StoneType stoneType, int row, int col)
    {
        
        var index = BoardValue * row + col;
        stones[index].SetStone(stoneType);
    }
    
    public void SetStoneState(Enums.StoneState state,int row, int col)
    {
        var index = BoardValue * row + col;
        stones[index].SetState(state);
    }
}
