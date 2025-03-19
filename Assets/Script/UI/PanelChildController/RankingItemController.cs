using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingItemController : MonoBehaviour
{
   ScoreInfo ScoreInfo;
   public Sprite[] profileSprites;

   public void Init(ScoreInfo ScoreInfo)
   {
      this.ScoreInfo = ScoreInfo;
      var itemImage = GetComponentsInChildren<Image>()[1];
      var itemText = GetComponentsInChildren<TextMeshProUGUI>();
            
      // itemImage.sprite = this.ScoreInfo.profileImageIndex;
      itemText[0].text = this.ScoreInfo.nickname;
      itemText[1].text = this.ScoreInfo.winRate.ToString();
   }

   public void OnClickRankingItem()
   {
      Debug.Log(ScoreInfo.nickname + "의 승률은" + ScoreInfo.winRate);
   }
}
