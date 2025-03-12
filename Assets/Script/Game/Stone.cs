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
    

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // private void Start()
    // {
    //     SetStone(Enums.StoneType.Black);
    //     SetState(Enums.StoneState.Selected);
    // }
    
    public void InitStone(int stoneIndex, OnStoneClicked onStoneClicked)
    {
        _index = stoneIndex;
        SetStone(Enums.StoneType.None);
        SetState(Enums.StoneState.None);
        _onStoneClicked = onStoneClicked;
    }
    
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

    public void SetState(Enums.StoneState stoneState)
    {
        switch (stoneState)
        {
            case Enums.StoneState.None:
                _spriteRenderer.sprite = stoneStateSprites[0];
                break;
            case Enums.StoneState.Selected:
                _spriteRenderer.sprite = stoneStateSprites[1];
                break;
            case Enums.StoneState.Blocked:
                _spriteRenderer.sprite = stoneStateSprites[2];
                break;
            case Enums.StoneState.LastPositioned:
                _spriteRenderer.sprite = stoneStateSprites[3];
                break;
        }
    }

    private void OnMouseUpAsButton()
    {
        _onStoneClicked?.Invoke(_index);
    }
}
