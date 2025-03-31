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
      
    public void OnClickConfirmButton()
    {
        Hide();
    }

    public void Show(Enums.GameResult gameResult)
    {
        base.Show(RatingPanelStart(gameResult));
    }

    private PanelControllerShowDelegate RatingPanelStart(Enums.GameResult gameResult)
    {
        StartCoroutine(UpdateScore(gameResult));
        return null;
    }

    private IEnumerator UpdateScore(Enums.GameResult gameResult)
    {
        //기존 점수로 애니메이션 보여줄 때까지 기다림
        yield return InitRatingPanel(gameResult);
        
        switch (gameResult)
        {
            case (Enums.GameResult.Win):
                NetworkManager.Instance.UpdateScore(1 , (scoreResultInfo) =>
                {
                    //유저 인포 업데이트
                    UserManager.Instance.UpdateUserScoreInfo(scoreResultInfo);
                    
                    //결과화면 띄우기
                    if (scoreResultInfo.isAdvancement == 1)
                    {
                        GameManager.Instance.panelManager.OpenRatingEffectPanel(1);
                        
                        //패널의 포인트들 초기화
                        _ratingPointsController.InitRatingUpPoints();
                    }
                },() => { });
                break;
            case (Enums.GameResult.Lose):
                NetworkManager.Instance.UpdateScore(-1, (scoreResultInfo) =>
                {
                    UserManager.Instance.UpdateUserScoreInfo(scoreResultInfo);
                    
                    if (scoreResultInfo.isAdvancement == -1)
                    {
                        GameManager.Instance.panelManager.OpenRatingEffectPanel(-1);
                        
                        _ratingPointsController.InitRatingDownPoints();
                    }
                }, () => { });
                break;
        }
    }

    /// <summary>
    /// 텍스트 초기화, 승급포인트 계산
    /// </summary>
    /// <param name="isWin"></param>
    private IEnumerator InitRatingPanel(Enums.GameResult gameResult)
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
        
        // 게임 전 스코어로 초기화
        NetworkManager.Instance.GetInfo((userInfo) =>
            {
                _oldScore = userInfo.score;
                // 1급이고 이미 10승 이상인 경우
                if (_myRating == 1 && userInfo.score >= 10 )
                {
                    // 10승에서 패배한 경우 점수 잃는 애니메이션
                    if (gameResult == Enums.GameResult.Lose && userInfo.score == 10)
                    {
                        _ratingPointsController.InitRatingPoints(_oldScore,_gameResult,requiredScore);
                    }
                    else
                    {
                        if(gameResult == Enums.GameResult.Lose)
                            _ratingPointsController.SetRatingUpLimit(_oldScore-1);
                        else
                            _ratingPointsController.SetRatingUpLimit(_oldScore+1);
                    }
                }
                // 18급이고 이미 3패 이상인 경우
                else if (_myRating == 18 && userInfo.score <= -3)
                {
                    //3승에서 승리한 경우 점수 얻는 애니메이션
                    if (gameResult == Enums.GameResult.Win && userInfo.score == -3)
                    {
                        _ratingPointsController.InitRatingPoints(_oldScore,_gameResult,requiredScore);
                    }
                    else
                    {
                        if(gameResult == Enums.GameResult.Lose)
                            _ratingPointsController.SetRatingDownLimit(_oldScore-1);
                        else
                            _ratingPointsController.SetRatingDownLimit(_oldScore+1);
                    }
                }
                else
                {
                    _ratingPointsController.InitRatingPoints(_oldScore,_gameResult,requiredScore);
                    
                }

            }, () =>
            { });

        string win = _gameResult == Enums.GameResult.Win ? "승리" : "패배";
        string get = _gameResult == Enums.GameResult.Win  ? "얻었습니다." : "잃었습니다.";
        
        if(_gameResult == Enums.GameResult.Draw)
        {
            getPointsText.text = "무승부입니다.";
        }
        else
        {
            getPointsText.text = $"게임에서 {win}했습니다.\n{Constants.RAING_POINTS} 승급 포인트를 {get}";
        }
        // 애니메이션 실행 완료를 위한 wait
        yield return new WaitForSecondsRealtime(1.8f);
    }
}
