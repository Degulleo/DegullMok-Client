using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingItemController : MonoBehaviour
{
   RankingItem _rankingItem;
   public void Init(RankingItem rankingItem)
   {
      _rankingItem = rankingItem;
      var itemImage = GetComponentsInChildren<Image>()[1];
      var itemText = GetComponentsInChildren<TextMeshProUGUI>();
            
      itemImage.sprite = GameManager.Instance.profileSprites[this._rankingItem.ProfileSpriteIndex];
      itemText[0].text = this._rankingItem.Name;
      itemText[1].text = this._rankingItem.WinRate.ToString();
   }

   public void OnClickRankingItem()
   {
      Debug.Log(_rankingItem.Name + "의 승률은" + _rankingItem.WinRate);
   }
}
