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
    [SerializeField] private GameObject threePointsIndicatorGameObject;
    [SerializeField] private GameObject fivePointsIndicatorGameObject;
    [SerializeField] private GameObject tenPointsIndicatorGameObject;
    [SerializeField] private Transform pointsIndicatorParent;

    private bool _isWin;
    private int _requiredScore;
    private int _oldScore;
    private int _newScore;
    private int _myRating;
    private RatingPointsController _ratingPointsController;
    
    /// <summary>
    /// 텍스트 초기화, 승급포인트 계산
    /// </summary>
    /// <param name="isWin"></param>
    public void InitRatingPanel(bool isWin)
    {
        _isWin = isWin; 
        _myRating= UserManager.Instance.Rating;
        if (_myRating >= 10 && _myRating <= 18) // 10~18급은 3점 필요
        {
            var threePointsIndicator = Instantiate(threePointsIndicatorGameObject, pointsIndicatorParent);
            _requiredScore = 3;
            
            _ratingPointsController = threePointsIndicator.GetComponent<RatingPointsController>();
        } 
        else if (_myRating >= 5 && _myRating <= 9) // 5~9급은 5점 필요
        {
            var fivePointsIndicator = Instantiate(fivePointsIndicatorGameObject, pointsIndicatorParent);
            _requiredScore = 5;

            _ratingPointsController = fivePointsIndicator.GetComponent<RatingPointsController>();
        } 
        else if (_myRating >= 1 && _myRating <= 4) // 1~4급은 10점 필요
        {
            var tenPointsIndicator = Instantiate(tenPointsIndicatorGameObject, pointsIndicatorParent);
            _requiredScore = 10;
            _ratingPointsController = tenPointsIndicator.GetComponent<RatingPointsController>();
        }

        string win = _isWin ? "승리" : "패배";
        string get = _isWin ? "얻었습니다." : "잃었습니다.";

        getPointsText.text = $"게임에서 {win}했습니다.\n{Constants.RAING_POINTS} 승급 포인트를 {get}";
        
        //network에 oldScore 요청.
        //이 부분은 승,패 패널에서 처리 후 rating,store만 oldData를 가져와서 승급에 전달해주고
        //
        NetworkManager.Instance.GetInfo((userInfo) =>
        {
            _oldScore = userInfo.score;
            _ratingPointsController.InitRatingPoints(_oldScore);
        }, () =>
        { });
    }

    void Start()
    {
        InitRatingPanel(false);
    }

}
