using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DrawConfirmPanelController : PanelController
{
    [SerializeField] private TMP_Text messageText;  //자식 텍스트 변수

    public delegate void OnConfirmButtonClick();
    private OnConfirmButtonClick onConfirmButtonClick;
    
    public delegate void OnContradictButtonClick();
    private OnContradictButtonClick onContradictButtonClick;

    public void Show(string message, OnConfirmButtonClick onConfirmButtonClick, OnContradictButtonClick onContradictButtonClick)
    {
        messageText.text = message;
        this.onConfirmButtonClick = onConfirmButtonClick;
        this.onContradictButtonClick = onContradictButtonClick;
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
        Hide(() => onContradictButtonClick?.Invoke());
    }
}