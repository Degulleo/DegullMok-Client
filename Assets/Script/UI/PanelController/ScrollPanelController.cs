using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ScrollPanelController : PanelController
{
    protected const int MAX_COUNT = 20; //스크롤 아이템 최대 개수
        
    [SerializeField]protected GameObject scrollItemPrefab;
    [SerializeField]protected GameObject content;
    [SerializeField] private GameObject closeButton;

    public void Show()
    {
        closeButton.GetComponent<SingleInteractableButtonHandler>().ResetButton();
        base.Show();
    }
    
    public void OnClickCloseButton()
    {
        Hide();
    }
    
}
