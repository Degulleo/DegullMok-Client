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
    public void OnConfirmPanelClick()
    {
        GameManager.Instance.OpenConfirmPanel("확인 패널 입니다.", () =>
        {
                Debug.Log("확인 버튼을 누르셨습니다.");
        });
        return;
    }
    
    public void OnSettingPanelClick()
    {
        GameManager.Instance.OpenSettingsPanel();
    }
    
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
        
        GameManager.Instance.OpenRankingPanel(rankingItems);
    }
    
    public void OnShopPanelClick()
    {
        
        List<ShopItem> shopItems = new List<ShopItem>();       //테스트 데이터 리스트 생성
        for (int i = 0; i < 30; i++)
        {
            ShopItem shopItem = new ShopItem
            {
                ItemSpriteIndex = Random.Range(0, 2),
                Name = i.ToString(),
                Price = (i * 1000)+ "원"
            };
            shopItems.Add(shopItem);
        }
        
        GameManager.Instance.OpenShopPanel(shopItems);
    }
    
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
        
        GameManager.Instance.OpenGiboPanel(giboItems);
    }

   
}
