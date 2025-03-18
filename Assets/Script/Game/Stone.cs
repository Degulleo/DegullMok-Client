using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    [SerializeField] private SpriteRenderer stoneMarkerSpriteRenderer;
    [SerializeField] private Sprite[] stoneTypeSprites;
    [SerializeField] private Sprite[] stoneStateSprites;

    private int _index;
    private SpriteRenderer _spriteRenderer;
    public delegate void OnStoneClicked(int index);
    private OnStoneClicked _onStoneClicked;
    private Enums.StoneState _currentStoneState;
    

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    //Stone 초기화 함수
    public void InitStone(int stoneIndex, OnStoneClicked onStoneClicked)
    {
        _index = stoneIndex;
        SetStone(Enums.StoneType.None);
        SetState(Enums.StoneState.None);
        _onStoneClicked = onStoneClicked;
    }
    
    //Stone 이미지 설정 함수(0: 없음, 1:흑, 2: 백)
    public void SetStone(Enums.StoneType stoneType)
    {
        switch (stoneType)
        {
            case Enums.StoneType.None:
                stoneMarkerSpriteRenderer.sprite = stoneTypeSprites[0];
                break;
            case Enums.StoneType.Black:
                stoneMarkerSpriteRenderer.sprite = stoneTypeSprites[1];
                break;
            case Enums.StoneType.White:
                stoneMarkerSpriteRenderer.sprite = stoneTypeSprites[2];
                break;
        }
    }

    //Stone 상태 이미지 설정 함수 (0: 없음, 1: 선택된, 2: 금수, 3: 마지막 배치된)
    public void SetState(Enums.StoneState stoneState)
    {
        switch (stoneState)
        {
            case Enums.StoneState.None:
                _currentStoneState = Enums.StoneState.None;
                _spriteRenderer.sprite = stoneStateSprites[0];
                break;
            case Enums.StoneState.Selected:
                _currentStoneState = Enums.StoneState.Selected;
                _spriteRenderer.sprite = stoneStateSprites[1];
                break;
            case Enums.StoneState.Blocked:
                _currentStoneState = Enums.StoneState.Blocked;
                _spriteRenderer.sprite = stoneStateSprites[2];
                break;
            case Enums.StoneState.LastPositioned:
                _currentStoneState = Enums.StoneState.LastPositioned;
                _spriteRenderer.sprite = stoneStateSprites[3];
                break;
        }
    }
    
    //Stone 상태 확인
    public Enums.StoneState GetStoneState()
    {
        return _currentStoneState;
    }

    private void OnMouseUpAsButton()
    {
        _onStoneClicked?.Invoke(_index);
    }
}
