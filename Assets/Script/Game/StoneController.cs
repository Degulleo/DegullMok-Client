using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneController : MonoBehaviour
{
    [SerializeField] private Stone[] stone;
    private delegate void OnStoneClicked(int row, int col);
    private OnStoneClicked _onStoneClickedDelegate;

    private void Start()
    {
        InitStones();
    }

    //Stone 객체들 초기화 함수 호출
    private void InitStones()
    {
        for (int i = 0; i < stone.Length; i++)
        {
            stone[i].InitStone(i, (index) =>
            {
                var rowIndex = index / 15;
                var colIndex = index % 15;
                _onStoneClickedDelegate?.Invoke(rowIndex, colIndex);
                
                //임시 확인 코드
                stone[index].SetStone(Enums.StoneType.White);
            });
        }
    }
}
