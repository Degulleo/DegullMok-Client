using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    [SerializeField] private SpriteRenderer stoneMarkerSpriteRenderer;
    [SerializeField] private Sprite[] stoneSprites;

    private int index;
    private SpriteRenderer _spriteRenderer;
    

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
