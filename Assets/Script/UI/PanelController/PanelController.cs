using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class PanelController : MonoBehaviour
{
    [SerializeField] private RectTransform panelRectTransform;      //팝업창 UI를 조작할 변수 // 자기 자신 할당
    
    private CanvasGroup backGroundCanvasGroup;                     // 배경 페이드 효과를 위한 변수
    
    public delegate void PanelControllerHideDelegate();
    
    private void Awake()
    {
        backGroundCanvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Panel 표시 함수
    /// 알파값과 크기를 0으로 줄였다가 1로 페이드
    /// </summary>
    public void Show()
    {
        backGroundCanvasGroup.alpha = 0;
        panelRectTransform.localScale = Vector3.zero;
        
        backGroundCanvasGroup.DOFade(1, 0.3f).SetEase(Ease.Linear);
        panelRectTransform.DOScale(1, 0.3f).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Panel 숨기기 함수
    /// 알파값과 크기를 0으로 페이드
    /// </summary>
    public void Hide(PanelControllerHideDelegate hideDelegate = null)
    {
        backGroundCanvasGroup.alpha = 1;
        panelRectTransform.localScale = Vector3.one;
        
        backGroundCanvasGroup.DOFade(0, 0.3f).SetEase(Ease.Linear);
        panelRectTransform.DOScale(0, 0.3f)
            .SetEase(Ease.InBack).OnComplete(() =>
            {
                hideDelegate?.Invoke();
                Destroy(gameObject);
            });
    }
}
