using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 메인 패널 메뉴 버튼
/// </summary>
public class MainPanelButtonController : MonoBehaviour
{
    //상점 패널 생성
    public void OnShopPanelClick()
    {
        List<ShopItem> shopItems = new List<ShopItem>();       //상점 데이터 리스트 생성
        for (int i = 0; i < 5; i++)
        {
            if (i == 0)     //광고 항목
            {
                ShopItem shopItem = new ShopItem
                {
                    Name = "코인500개 ",
                    Price = "광고"
                };
                shopItems.Add(shopItem);
            }
            else
            {
                ShopItem shopItem = new ShopItem
                {
                    Name = "코인"+i*1000+"개 ",
                    Price = (i * 1000)+ "원"
                };
                shopItems.Add(shopItem);
            }
        }
 
        GameManager.Instance.panelManager.OpenShopPanel(shopItems);
    }

    public void OpenReplayPanelClick()
    {
        GameManager.Instance.panelManager.OpenReplayPanel();
    }

    public void OpenRankingPanelClick()
    {
        GameManager.Instance.panelManager.OnRankingPanelClick();
    }
}
