using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiboPanelController : ScrollPanelController
{
    public virtual void Show(List<GiboItem> items)
    {
        for (int i = 0; i < items.Count && i <= MAX_COUNT; i++)
        {
            var scrollItem= Instantiate(scrollItemPrefab, content.transform);
            scrollItem.GetComponent<GiboItemController>().Init(items[i]);
            
        }
        base.Show();
    }
}
