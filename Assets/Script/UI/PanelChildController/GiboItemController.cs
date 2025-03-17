using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiboItemController : MonoBehaviour
{
   GiboItem _giboItem;
   public Sprite[] profileSprites;
   public void Init(GiboItem giboItem)
   {
      _giboItem = giboItem;
      
      var itemImage = GetComponentsInChildren<Image>()[1];
      var itemText = GetComponentsInChildren<TextMeshProUGUI>();
            
      itemImage.sprite = profileSprites[this._giboItem.WinLoseSpriteIndex];
      itemText[0].text = this._giboItem.Date;
      itemText[1].text = this._giboItem.Name;
   }
   
   public void OnClickGiboItem()
   {
      Debug.Log(_giboItem.Name + "님과 대국 날짜는" + _giboItem.Date);
   }
}
