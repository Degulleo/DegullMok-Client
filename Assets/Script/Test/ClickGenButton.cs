using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickGenButton : MonoBehaviour
{
   List<RankingItem> RankingItems = new List<RankingItem>();   //테스트용 리스트 scrollItems
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
   // public void ClickShopScrollPanel()
   // {
   //    GameManager.Instance.OpenShopScrollPanel(scrollItems);
   // }
   
   
   // //기보 스크롤 패널 여는 함수 여는 함수
   // public void ClickGiboScrollPanel()
   // {
   //    GameManager.Instance.OpenGiboScrollPanel(scrollItems);
   // }
   
   
   //결과 패널 여는 함수 여는 함수

   
   
   
}
