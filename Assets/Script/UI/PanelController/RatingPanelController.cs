using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RatingPanelController : ConfirmPanelController
{
    [SerializeField] private TMP_Text getPointsText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] Image[] threePointsImages;

    private bool _isWin;
    private int _requiredPoints;
    private int _currentPoints;
    private int _myRating;
    
    private Color32 _minusColor = new Color32(255, 0, 0, 255);
    private Color32 _plusColor = new Color32(34, 87, 255, 255);
    private Color32 _defaultColor = new Color32(176, 176, 176, 255);

    /// <summary>
    /// 텍스트 초기화, 승급포인트 계산
    /// </summary>
    /// <param name="isWin"></param>
    public void InitRatingPanel(bool isWin)
    {
        //network에 스코어 요청
        _isWin = isWin; 
        _myRating= UserManager.Instance.Rating;
        if (_myRating >= 10 && _myRating <= 18) {// 10~18급은 3점 필요
            
        } else if (_myRating >= 5 && _myRating <= 9) {// 5~9급은 5점 필요
            
        } else if (_myRating >= 1 && _myRating <= 4) {// 1~4급은 10점 필요
            
        }
        string win = _isWin ? "승리" : "패배";
        string get = _isWin ? "얻었습니다." : "잃었습니다.";

        getPointsText.text = $"게임에서 {win}했습니다.\n{Constants.RAING_POINTS} 승급 포인트를 {get}";
    }

    void Start()
    {
        InitRatingPanel(false);
    }

}
