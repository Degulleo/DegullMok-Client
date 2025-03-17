using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemController : MonoBehaviour
{
    ShopItem _shopItem;
    public Sprite[] profileSprites;

    public void Init(ShopItem shopItem)
    {
        _shopItem = shopItem;
        var itemImage = GetComponentsInChildren<Image>()[1];
        var itemText = GetComponentsInChildren<TextMeshProUGUI>();
            
        itemImage.sprite = profileSprites[this._shopItem.ItemSpriteIndex];
        itemText[0].text = this._shopItem.Name;
        itemText[1].text = this._shopItem.Price;
    }
    
    public void OnClickShopItem()
    {
        Debug.Log(_shopItem.Name + "의 가격은" + _shopItem.Price);
    }
}
