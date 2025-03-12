using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 테스트용 싱글톤 게임매니저
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject settingsPanel;
    
    public Canvas _canvas;
    
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    
    //확인 패널 여는 함수
    public void OpenConfirmPanel(string message, ConfirmPanelController.OnConfirmButtonClick onConfirmButtonClick)
    {
        if (_canvas != null)
        {
            var confirmPanelObject = Instantiate(confirmPanel, _canvas.transform);
            confirmPanelObject.GetComponent<ConfirmPanelController>()
                .Show(message, onConfirmButtonClick);
        }
    }
    
    //세팅 패널 여는 함수
    public void OpenSettingsPanel()
    {
        if (_canvas != null)
        {
            var settingsPanelObject = Instantiate(settingsPanel, _canvas.transform);
            settingsPanelObject.GetComponent<PanelController>().Show();
        }
    }
    
    //스크롤 패널 여는 함수
    
    
    //결과 패널 여는 함수
    
    
    
}
