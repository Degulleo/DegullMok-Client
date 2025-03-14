using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ClickGenButton : MonoBehaviour
{
   List<RankingItem> RankingItems = new List<RankingItem>();   //테스트용 리스트 
   List<GiboItem> GiboItems = new List<GiboItem>();   //테스트용 리스트 
   List<ShopItem> ShopItems = new List<ShopItem>();   //테스트용 리스트 

   void Start()
   {
      for (int i = 0; i < 30; i++)
      {
         RankingItem scrollItem = new RankingItem
         {
            ProfileSpriteIndex = Random.Range(0,2),
            Name = i.ToString(),
            WinRate = Random.Range(0, 100)
         };
         RankingItems.Add(scrollItem);
      }
      
      for (int i = 0; i < 30; i++)
      {
         GiboItem scrollItem = new GiboItem
         {
            WinLoseSpriteIndex = Random.Range(0,2),
            Date = DateTime.Now.ToString("yyyy/MM/dd"),
            Name = i.ToString()
         };
         GiboItems.Add(scrollItem);
      }
      
      for (int i = 0; i < 30; i++)
      {
         ShopItem scrollItem = new ShopItem
         {
            ItemSpriteIndex = Random.Range(0,2),
            Name = i.ToString(),
            Price = (i*1000).ToString()

         };
         ShopItems.Add(scrollItem);
      }
      
      
   }
   
   
      
      
   //확인 패널 여는 함수 여는 함수
   public void ClickConfirmPanel()
   {
      GameManager.Instance.OpenConfirmPanel("Click Gen Button", () =>
      {
         Debug.Log("Click Confirm Button");
      });
   }
   
   
   //세팅 패널 여는 함수 여는 함수
   public void ClickSettingsPanel()
   {
      GameManager.Instance.OpenSettingsPanel();
   }
   
   
   //랭킹 스크롤 패널 여는 함수 여는 함수
   public void ClickRankingScrollPanel()
   {
      GameManager.Instance.OpenRankingScrollPanel(RankingItems);
   }
   
   
   //상점 스크롤 패널 여는 함수 여는 함수
   public void ClickShopScrollPanel()
   {
      GameManager.Instance.OpenShopScrollPanel(ShopItems);
   }
   
   
   //기보 스크롤 패널 여는 함수 여는 함수
   public void ClickGiboScrollPanel()
   {
      GameManager.Instance.OpenGiboScrollPanel(GiboItems);
   }
   
   
   //결과 패널 여는 함수 여는 함수

   
   
   
}
