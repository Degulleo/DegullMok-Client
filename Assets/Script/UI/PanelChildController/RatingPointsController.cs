using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RatingPointsController : MonoBehaviour
{
    [SerializeField] GameObject[] minusImages;
    [SerializeField] GameObject[] plusImage;
    [SerializeField] private float flipDuration = 0.3f;
    
    private Color32 _minusColor = new Color32(255, 0, 0, 255);
    private Color32 _plusColor = new Color32(34, 87, 255, 255);
    private Color32 _defaultColor = new Color32(176, 176, 176, 255);

    public void InitRatingPoints(int oldScore)
    {
        if (oldScore == 0)
        {
            return;
        }else if (oldScore > 0)
        {
            for (int i = 0; i < oldScore; i++)
            {
                plusImage[i].GetComponent<Image>().color = _plusColor;
            }
        }
        else if (oldScore < 0)
        {
            for (int i = oldScore; i < 0; i++)
            {
                minusImages[minusImages.Length+i].GetComponent<Image>().color = _minusColor;
            }
        }
    }
}
