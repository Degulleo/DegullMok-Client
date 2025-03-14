using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinUITestScript : MonoBehaviour
{
    [SerializeField] private CoinsPanelController coinsPanelController;

    public void OnClickAddCoin()
    {
        coinsPanelController.AddCoins(100, () =>
        {
            Debug.Log("Add coin 후 동작");
        });
    }

    public void OnClickRemoveCoin()
    {
        coinsPanelController.RemoveCoins(() =>
        {
            Debug.Log("코인을 제거한 후 동작");
        });
    }

}
