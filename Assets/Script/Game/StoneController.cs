using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneController : MonoBehaviour
{
    [SerializeField] private Stone[] stone;

    private void Start()
    {
        InitStones();
    }

    private void InitStones()
    {
        for (int i = 0; i < stone.Length; i++)
        {
            stone[i].InitStone(i, (index) =>
            {
                stone[index].SetStone(Enums.StoneType.White);
            });
        }
    }
}
