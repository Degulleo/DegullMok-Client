using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickGenButton : MonoBehaviour
{
   
   //확인 패널 여는 함수 여는 함수
   public void ClickConfirmPanel()
   {
      GameManager.Instance.OpenConfirmPanel("Click Gen Button",ClickConfirmButton);
   }
   void ClickConfirmButton()
   {
      Debug.Log("Click Confirm Button");
   }
   
   //세팅 패널 여는 함수 여는 함수
   public void ClickSettingsPanel()
   {
      GameManager.Instance.OpenSettingsPanel();
   }
   
   //스크롤 패널 여는 함수 여는 함수

   
   //결과 패널 여는 함수 여는 함수

   
   
   
}
