using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RatingPanelController : PanelController
{
    [SerializeField] private TMP_Text getPointsText;
    [SerializeField] private GameObject threePointsIndicatorGameObject;
    [SerializeField] private GameObject fivePointsIndicatorGameObject;
    [SerializeField] private GameObject tenPointsIndicatorGameObject;

    private Enums.GameResult _gameResult;
    private int _oldScore;
    private int _newScore;
    private int _myRating;
    private RatingPointsController _ratingPointsController;
        
    public void Show(Enums.GameResult gameResult)
    {
        InitRatingPanel(gameResult);
        base.Show();
    }

    public void OnClickConfirmButton()
    {
        Hide();
    }

    public void OnClickRetryButton()
    {
        Hide(() => { });
    }
    
    
    /// <summary>
    /// 텍스트 초기화, 승급포인트 계산
    /// </summary>
    /// <param name="isWin"></param>
    public void InitRatingPanel(Enums.GameResult gameResult)
    {
        _gameResult = gameResult; 
        _myRating= UserManager.Instance.Rating;
        int requiredScore = 0;
        if (_myRating >= 10 && _myRating <= 18) // 10~18급은 3점 필요
        {
            requiredScore = 3;
            threePointsIndicatorGameObject.SetActive(true);
            _ratingPointsController = threePointsIndicatorGameObject.GetComponent<RatingPointsController>();
            
        } 
        else if (_myRating >= 5 && _myRating <= 9) // 5~9급은 5점 필요
        {
            requiredScore = 5;
            fivePointsIndicatorGameObject.SetActive(true);
            _ratingPointsController = fivePointsIndicatorGameObject.GetComponent<RatingPointsController>();
        } 
        else if (_myRating >= 1 && _myRating <= 4) // 1~4급은 10점 필요
        {
            requiredScore = 10;
            tenPointsIndicatorGameObject.SetActive(true);
            _ratingPointsController = tenPointsIndicatorGameObject.GetComponent<RatingPointsController>();
        }

        string win = _gameResult == Enums.GameResult.Win ? "승리" : "패배";
        string get = _gameResult == Enums.GameResult.Win  ? "얻었습니다." : "잃었습니다.";

        getPointsText.text = $"게임에서 {win}했습니다.\n{Constants.RAING_POINTS} 승급 포인트를 {get}";
        
        //게임 승패 이전의 rating과 score로 패널 초기화
        NetworkManager.Instance.GetInfo((userInfo) =>
        {
            _oldScore = userInfo.score;
            _ratingPointsController.InitRatingPoints(_oldScore,requiredScore);
        }, () =>
        { });
    }
    

}
