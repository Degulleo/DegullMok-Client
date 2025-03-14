using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingPanelController : ScrollPanelController
{
    public virtual void Show(List<RankingItem> items)
    {
        for (int i = 0; i < items.Count && i <= MAX_COUNT; i++)
        {
            var scrollItem= Instantiate(scrollItemPrefab, content.transform);
            scrollItem.GetComponent<RankingItemController>().Init(items[i]);
            
        }
        base.Show();
    }
    
}
