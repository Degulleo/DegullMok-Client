using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class SwitchController : MonoBehaviour
{
    [SerializeField] private Image handleImage;
    
    //스위치에 상태 변경 시 호출할 콜백 함수
    public delegate void OnSwitchChangedDelegate(bool isOn);
    public OnSwitchChangedDelegate OnSwitchChanged;
    
    private static readonly Color32 OnColor = new Color32(164, 220, 223, 255);
    private static readonly Color32 OffColor = new Color32(70, 93, 117, 255);
    
    private RectTransform _handleRectTransform;
    private Image _backgroundImage;
    
    private bool _isOn;
    
    private void Awake()
    {
        _handleRectTransform = handleImage.GetComponent<RectTransform>();
        _backgroundImage = GetComponent<Image>();
    }

    private void Start()
    {
        _isOn = gameObject.name == "SFX Switch" ? UserManager.IsPlaySFX :
            gameObject.name == "BGM Switch" ? UserManager.IsPlayBGM : false;

        _handleRectTransform.anchoredPosition = _isOn ? new Vector2(14, 0) : new Vector2(-14, 0);
        _backgroundImage.color = _isOn ? OnColor : OffColor;
    }

    //스위치 상태 변경 함수
    private void SetOn(bool isOn)
    {
        _handleRectTransform.DOAnchorPosX(isOn ? 14 : -14, 0.2f);
        _backgroundImage.DOColor(isOn ? OnColor : OffColor, 0.2f);
        AudioManager.Instance.PlayClickSound();
        
        //이벤트 호출
        OnSwitchChanged?.Invoke(isOn);
        _isOn = isOn;
    }

    public void OnClickSwitch()
    {
        SetOn(!_isOn);
    }
    
    public void SetSwitch(bool isOn)
    {
        SetOn(isOn);
    }
}