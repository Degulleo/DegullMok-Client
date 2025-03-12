using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickGenButton : MonoBehaviour
{
   public void ClickConfirmPanel()
   {
      GameManager.Instance.OpenConfirmPanel("Click Gen Button",ClickConfirmButton);
   }

   void ClickConfirmButton()
   {
      Debug.Log("Click Confirm Button");
   }
}
