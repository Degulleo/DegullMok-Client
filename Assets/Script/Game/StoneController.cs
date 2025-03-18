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
    public const int MaxCellCount = 15;

    //Stone 객체들 초기화 함수 호출
    public void InitStones()
    {
        for (int i = 0; i < stones.Length; i++)
        {
            stones[i].InitStone(i, (index) =>
            {
                var rowIndex = index / MaxCellCount;
                var colIndex = index % MaxCellCount;
                OnStoneClickedDelegate?.Invoke(rowIndex, colIndex);
            });
        }
    }
    //스톤 타입변경
    public void SetStoneType(Enums.StoneType stoneType, int row, int col)
    {
        var index = MaxCellCount * row + col;
        stones[index].SetStone(stoneType);
    }
    
    //스톤상태변경
    public void SetStoneState(Enums.StoneState state,int row, int col)
    {
        var index = MaxCellCount * row + col;
        stones[index].SetState(state);
    }
    
    //스톤상태 확인
    public Enums.StoneState GetStoneState(int row, int col)
    {
        var index = MaxCellCount * row + col;
        return stones[index].GetStoneState();
    }
}
