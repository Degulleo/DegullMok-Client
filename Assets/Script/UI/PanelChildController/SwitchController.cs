using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(AudioSource))]
public class SwitchController : MonoBehaviour
{
    [SerializeField] private Image handleImage;
    [SerializeField] private AudioClip clickSound;
    
    //스위치에 상태 변경 시 호출할 콜백 함수
    public delegate void OnSwitchChangedDelegate(bool isOn);
    public OnSwitchChangedDelegate OnSwitchChanged;
    
    private static readonly Color32 OnColor = new Color32(242, 68, 149, 255);
    private static readonly Color32 OffColor = new Color32(70, 93, 117, 255);
    
    private RectTransform _handleRectTransform;
    private Image _backgroundImage;
    private AudioSource _audioSource;
    
    private bool _isOn;
    
    private void Awake()
    {
        _handleRectTransform = handleImage.GetComponent<RectTransform>();
        _backgroundImage = GetComponent<Image>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        //초기 상태는 false
        _handleRectTransform.anchoredPosition = new Vector2(-14, 0);
        _backgroundImage.color = OffColor;
        _isOn = false;
    }

    //스위치 상태 변경 함수
    private void SetOn(bool isOn)
    {
        
        if (isOn)
        {
            _handleRectTransform.DOAnchorPosX(14, 0.2f);
            _backgroundImage.DOBlendableColor(OnColor, 0.2f);
        }
        else
        {
            _handleRectTransform.DOAnchorPosX(-14, 0.2f);
            _backgroundImage.DOBlendableColor(OffColor, 0.2f);
        }
        
        // 효과음 재생
        if (clickSound != null) 
            _audioSource.PlayOneShot(clickSound);
        
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