using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RatingPointsController : MonoBehaviour
{
    [SerializeField] Image[] minusImages;
    [SerializeField] Image[] plusImage;
    
    private Color32 _minusColor = new Color32(255, 0, 0, 255);
    private Color32 _plusColor = new Color32(34, 87, 255, 255);
    private Color32 _defaultColor = new Color32(176, 176, 176, 255);
    
}
