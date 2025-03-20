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
        
        itemText[0].text = this._shopItem.Name;
        itemText[1].text = this._shopItem.Price;
    }
    
    public void OnClickShopItem()
    {
        if (_shopItem.Price == "광고")
        {
            //보상형 전면 광고 로드
            FindObjectOfType<AdManager>().ShowRewardedInterstitialAd(); //Todo FindOf 함수 수정
        }
        else
        {
            //todo 가격별로 구매하기
        }
    }
}
