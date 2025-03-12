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
    
    public void OpenConfirmPanel(string message, ConfirmPanelController.OnConfirmButtonClick onConfirmButtonClick)
    {
        if (_canvas != null)
        {
            var confirmPanelObject = Instantiate(confirmPanel, _canvas.transform);
            confirmPanelObject.GetComponent<ConfirmPanelController>()
                .Show(message, onConfirmButtonClick);
        }
    }
}
