using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private GameObject retryButton;
    [SerializeField] private TMP_Text playerANameText;
    [SerializeField] private TMP_Text playerBNameText;
    [SerializeField] private Image indicatorA;
    [SerializeField] private Image indicatorB;
    [SerializeField] private Image profileImageA;
    [SerializeField] private Image profileImageB;
    [SerializeField] private Sprite[] profileImageSprites;  //0. 기본 드래곤 1. 기본 호랑이 2.아이보리 드래곤 3. 아이보리 호랑이
    [SerializeField] private Sprite[] indicatorSprites; //0. active 1. inactive
    
    private Sprite _originalSpriteA;
    private Sprite _originalSpriteB;
    
    private MultiplayManager _multiplayManager;

    private void Start()
    {
        _multiplayManager = GameManager.Instance.GetMultiplayManager();
    }
    
    public void OnClickConfirmButton()
    {
        GameManager.Instance.OnClickConfirmButton();
    }

    public void OnClickRetryButton()
    {
        GameManager.Instance.RetryGame();
    }

    public void OnClickSurrenderButton()
    {
        GameManager.Instance.panelManager.OpenConfirmPanel("항복 하시겠습니까?", () =>
        {
            _multiplayManager.RequestSurrender();
        });
    }

    public void OnClickDrawRequestButton()
    {
        if (GameManager.Instance.GetRequestDrawChance())
        {
            GameManager.Instance.panelManager.OpenConfirmPanel("무승부 신청을 하시겠습니까?", () =>
            {
                _multiplayManager.RequestDraw();
            });
            GameManager.Instance.SetRequestDrawChanceFalse();
        }
        else
        {
            GameManager.Instance.panelManager.OpenConfirmPanel("무승부 요청이 제한돼있습니다.",()=>{});
        }
    }

    public void OnClickRevengeRequestButton()
    {
            GameManager.Instance.panelManager.OpenConfirmPanel("재대결 신청을 하시겠습니까?", () =>
            {
                _multiplayManager.RequestRevengeRequest();
            });
    }

    public void OnClickSettingsButton()
    {
        GameManager.Instance.panelManager.OpenSettingsPanel();
    }
    
    public void InitPlayersName(string playerNameA, string playerNameB)
    {
        playerANameText.text = playerNameA;
        playerBNameText.text = playerNameB;
    }

    public void InitProfileImages(int profileImageIndexA, int profileImageIndexB)
    {
        profileImageA.sprite = profileImageIndexA == 0 ? profileImageSprites[0] : profileImageSprites[profileImageIndexA];
        profileImageB.sprite = profileImageIndexB == 0 ? profileImageSprites[0] : profileImageSprites[profileImageIndexB];
        _originalSpriteA = profileImageA.sprite;
        _originalSpriteB = profileImageB.sprite;
    }

    public void SetTurnIndicator(bool isFirstPlayer)
    {
        if (isFirstPlayer)
        {
            SetPlayerTurn(profileImageA, _originalSpriteA);
            profileImageB.sprite = _originalSpriteB;
            indicatorA.sprite = indicatorSprites[0];
            indicatorB.sprite = indicatorSprites[1];
            
        }
        else
        {
            SetPlayerTurn(profileImageB, _originalSpriteB);
            profileImageA.sprite = _originalSpriteA;
            indicatorA.sprite = indicatorSprites[1];
            indicatorB.sprite = indicatorSprites[0];
            
        }
    }

    private void SetPlayerTurn(Image profileImage, Sprite _originalSprite)
    {
        if (_originalSprite == profileImageSprites[0])
        {
            profileImage.sprite = profileImageSprites[2];
        }
        else if (_originalSprite == profileImageSprites[1])
        {
            profileImage.sprite = profileImageSprites[3];
        }

        profileImage.transform.DOScale(1.5f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            profileImage.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        });
    }

}
