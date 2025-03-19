using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemController : MonoBehaviour
{
    ShopItem _shopItem;
    AdManager _adManager;

    public void Init(ShopItem shopItem)
    {
        _shopItem = shopItem;
        var itemImage = GetComponentsInChildren<Image>()[1];
        var itemText = GetComponentsInChildren<TextMeshProUGUI>();
        
        itemText[0].text = this._shopItem.Name;
        itemText[1].text = this._shopItem.Price+"원";
        
    }
    
    public void OnClickShopItem()
    {
        var shopPanel = GetComponentInParent<CanvasGroup>();    //코인 구매시 상점 패널의 캔버스 그룹 raycast를 비활성화하여 중복클릭 방지.

        if (_shopItem.Price == 0)
        {
            //보상형 전면 광고 로드
            _adManager = GetComponent<AdManager>();
            _adManager.ShowRewardedInterstitialAd(shopPanel);
        }
        else
        {
            
            NetworkManager.Instance.PurchaseCoins(
                _shopItem.Price,            // 충전할 코인 개수
                _shopItem.Name, // 결제 ID
                "GooglePay",     // 결제 방식 (GooglePay, PayPal 등)
                (coins) => {
                    GameManager.Instance.panelManager.UpdateCoinsPanelUI(coins,shopPanel);
                },
                () => {
                    Debug.LogError("결제 후 코인 충전 실패!");
                }
            );
        }
    }
}
