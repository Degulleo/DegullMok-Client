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
    [SerializeField] private GameObject rankingPanel;
    
    //[SerializeField] private GameObject scrollPanel;
    
    public Sprite[] profileSprites = new Sprite[2];   //테스트용 스프라이트 배열
    
    public Canvas canvas;

    public enum ScrollType
    {
        Ranking,
        Shop,
        Gibo
    }
    
    
    
    
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
        if (canvas != null)
        {
            var confirmPanelObject = Instantiate(confirmPanel, canvas.transform);
            confirmPanelObject.GetComponent<ConfirmPanelController>()
                .Show(message, onConfirmButtonClick);
        }
    }
    
    //세팅 패널 여는 함수
    public void OpenSettingsPanel()
    {
        if (canvas != null)
        {
            var settingsPanelObject = Instantiate(settingsPanel, canvas.transform);
            settingsPanelObject.GetComponent<PanelController>().Show();
        }
    }
    
    //랭킹 스크롤 패널 여는 함수
    public void OpenRankingScrollPanel(List<RankingItem> items)
    {
        if (canvas != null)
        {
            var scrollPanelObject = Instantiate(rankingPanel, canvas.transform);
            scrollPanelObject.GetComponent<RankingPanelController>().Show(items);
        }
    }
    
    //상점 스크롤 패널 여는 함수
    // public void OpenShopScrollPanel(List<ScrollItem> items)
    // {
    //     if (canvas != null)
    //     {
    //         var scrollPanelObject = Instantiate(scrollPanel, canvas.transform);
    //         scrollPanelObject.GetComponent<ScrollPanelController>().Show(items);
    //     }
    // }
    
    //기보 스크롤 패널 여는 함수
    // public void OpenGiboScrollPanel(List<ScrollItem> items)
    // {
    //     if (canvas != null)
    //     {
    //         var scrollPanelObject = Instantiate(scrollPanel, canvas.transform);
    //         scrollPanelObject.GetComponent<ScrollPanelController>().Show(items);
    //     }
    // }
    
    //결과 패널 여는 함수..
    
    
    
}
