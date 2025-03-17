using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 패널 생성 테스트 코드
/// 버튼을 누르면 팝업 생성
/// 상점, 랭크, 기보 패널은 각 데이터타입 리스트를 전달해야함
/// </summary>
public class TestPanelController : MonoBehaviour
{
    //게임매니저 없이 동작하도록 작성한 테스트 코드  
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject rankingPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject giboPanel;
    public Canvas _canvas;
    public void OpenConfirmPanel(string message, ConfirmPanelController.OnConfirmButtonClick onConfirmButtonClick)
    {
        if (_canvas != null)
        {
            var confirmPanelObject = Instantiate(confirmPanel, _canvas.transform);
            confirmPanelObject.GetComponent<ConfirmPanelController>()
                .Show(message, onConfirmButtonClick);
        }
    }
    
    public void OpenSettingsPanel()
    {
        if (_canvas != null)
        {
            var settingsPanelObject = Instantiate(settingsPanel, _canvas.transform);
            settingsPanelObject.GetComponent<PanelController>().Show();
        }
    }
    
    public void OpenRankingPanel(List<RankingItem> rankingItems)
    {
        if (_canvas != null)
        {
            var settingsPanelObject = Instantiate(rankingPanel, _canvas.transform);
            settingsPanelObject.GetComponent<RankingPanelController>().Show(rankingItems);
        }
    }
    
    public void OpenShopPanel(List<ShopItem> shopItems)
    {
        if (_canvas != null)
        {
            var settingsPanelObject = Instantiate(shopPanel, _canvas.transform);
            settingsPanelObject.GetComponent<ShopPanelController>().Show(shopItems);
        }
    }
    
    public void OpenGiboPanel(List<GiboItem> giboItems)
    {
        if (_canvas != null)
        {
            var settingsPanelObject = Instantiate(giboPanel, _canvas.transform);
            settingsPanelObject.GetComponent<GiboPanelController>().Show(giboItems);
        }
    }
    
    
    
    //확인 패널 생성
    public void OnConfirmPanelClick()
    {
        OpenConfirmPanel("확인 패널 입니다.", () =>
        {
                Debug.Log("확인 버튼을 누르셨습니다.");
        });
        return;
    }
    
    //세팅 패널 생성
    public void OnSettingPanelClick()
    {
        OpenSettingsPanel();
    }
    
    //랭킹 패널 생성
    public void OnRankingPanelClick()
    {
        
        List<RankingItem> rankingItems = new List<RankingItem>();       //테스트 데이터 리스트 생성
        for (int i = 0; i < 30; i++)
        {
            RankingItem rankingItem = new RankingItem
            {
                ProfileSpriteIndex = Random.Range(0, 2),
                Name = i.ToString(),
                WinRate = Random.Range(0f, 1f)
            };
            rankingItems.Add(rankingItem);
        }
        
        OpenRankingPanel(rankingItems);
    }
    
    //상점 패널 생성
    public void OnShopPanelClick()
    {
        
        List<ShopItem> shopItems = new List<ShopItem>();       //테스트 데이터 리스트 생성
        for (int i = 0; i < 30; i++)
        {
            ShopItem shopItem = new ShopItem
            {
                ItemSpriteIndex = Random.Range(0, 1),
                Name = "코인"+i+"개",
                Price = (i * 1000)+ "원"
            };
            shopItems.Add(shopItem);
        }
        
        OpenShopPanel(shopItems);
    }
    
    //기보 패널 생성
    public void OnGiboPanelClick()
    {
        
        List<GiboItem> giboItems = new List<GiboItem>();       //테스트 데이터 리스트 생성
        for (int i = 0; i < 30; i++)
        {
            GiboItem giboItem = new GiboItem
            {
                WinLoseSpriteIndex = Random.Range(0, 2),
                Date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                Name = i.ToString(),
            };
            
            giboItems.Add(giboItem);
        }
        
        OpenGiboPanel(giboItems);
    }

   
}
