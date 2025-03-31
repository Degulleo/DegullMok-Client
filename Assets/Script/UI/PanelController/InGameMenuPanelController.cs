using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenuPanelController : PanelController
{
    [SerializeField] private GameObject drawRegisterButton; // 무승부 요청
    [SerializeField] private GameObject settingsButton; // 설정 버튼
    [SerializeField] private GameObject closeButton;
    
    public delegate void OnInGameMenuButtonClick();
    private OnInGameMenuButtonClick onInGameMenuButtonClick;
    
    public void Show(OnInGameMenuButtonClick onInGameMenuButtonClick)
    {
        closeButton.GetComponent<SingleInteractableButtonHandler>().ResetButton();
        this.onInGameMenuButtonClick = onInGameMenuButtonClick;
        base.Show();
    }

    public void OnClickDrawRegisterButton()
    {
        GameManager.Instance.OnClickDrawRegisterButton();
    }
    
    public void OpenSettingsPanel()
    {
        GameManager.Instance.OpenSettingsPanel();
    }
    
    /// <summary>
    /// Confirm 버튼 클릭시 호출되는 함수
    /// </summary>
    public void OnClickConfirmButton()
    {
        Hide(() => onInGameMenuButtonClick?.Invoke());
    }

    /// <summary>
    /// X 버튼 클릭시 호출되는 함수
    /// </summary>
    public void OnClickCloseButton()
    {
        Hide();
    }
}