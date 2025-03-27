using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanelController : PanelController
{
    [SerializeField] private TMP_Text messageText;  //자식 텍스트 변수
    [SerializeField] private GameObject closeButton; // X 버튼
    [SerializeField] private GameObject confirmButton;
    
    public delegate void OnConfirmButtonClick();
    private OnConfirmButtonClick onConfirmButtonClick;
    
    public void Show(string message, OnConfirmButtonClick onConfirmButtonClick, bool isClose, bool isConfirm)
    {
        messageText.text = message;
        this.onConfirmButtonClick = onConfirmButtonClick;
        closeButton.SetActive(isClose);
        confirmButton.SetActive(isConfirm);
        base.Show();
    }
    
    /// <summary>
    /// Confirm 버튼 클릭시 호출되는 함수
    /// </summary>
    public void OnClickConfirmButton()
    {
        Hide(() => onConfirmButtonClick?.Invoke());
    }

    /// <summary>
    /// X 버튼 클릭시 호출되는 함수
    /// </summary>
    public void OnClickCloseButton()
    {
        Hide();
    }
}