using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class RatingPointsController : MonoBehaviour
{
    [SerializeField] GameObject[] minusImages;
    [SerializeField] GameObject[] plusImage;
    [SerializeField] TMP_Text scoreCountText;
    
    private float flipDuration = 1f;
    private Color32 _minusColor = new Color32(255, 0, 0, 255);
    private Color32 _plusColor = new Color32(34, 87, 255, 255);
    private Color32 _defaultColor = new Color32(176, 176, 176, 255);

    private int _oldRequiredScore;
    private int _newRequiredScore;
    private int _oldScore;

    /// <summary>
    /// 포인트 오르고 내려가는 애니메이션 재생
    /// </summary>
    /// <param name="imageOBject"></param>
    /// <param name="color"></param>
    private void ChangeImageColor(GameObject imageOBject, Color32 color)
    {
        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(
            imageOBject.GetComponent<Transform>().DOLocalRotate(new Vector3(0f, 90f, 0f), flipDuration).SetEase(Ease.InExpo));
        sequence.Append(
            imageOBject.GetComponent<Transform>().DOLocalRotate(Vector3.zero, flipDuration).SetEase(Ease.OutExpo));
        sequence.Join(
            imageOBject.GetComponent<Image>().DOColor(color, flipDuration/2).SetEase(Ease.OutExpo));
        
        sequence.OnComplete(()=>
        {
            SetScoreCountText();
        });
    }
    public void InitRatingPoints(int oldScore,Enums.GameResult gameResult, int defaultRequiredScore)
    {
        // TODO: [인덱스계산 ㅇㅖ외처리 ] 계산한 값 절대값이 defaultRequiredScore보다 큰 경우 return. 근데 이런 값이 나온다는게 이미 계산 오류가 어디서 생긴 것이겠죠..?
        _oldScore = oldScore;
        _oldRequiredScore = defaultRequiredScore;
        if (_oldScore == 0)
        {
            if (gameResult == Enums.GameResult.Win)
            {
                ChangeImageColor(plusImage[0], _plusColor);
                //승급까지 남은 판수 계산
                _newRequiredScore = defaultRequiredScore-1;
            }
            else if(gameResult == Enums.GameResult.Lose)
            {
                ChangeImageColor(minusImages[defaultRequiredScore-1],_minusColor);
                
                _newRequiredScore = defaultRequiredScore+1;
            }
        }
        // 이번 게임 전 기존 점수가 플러스 였을 경우
        else if (_oldScore > 0)
        {
            for (int i = 0; i < _oldScore; i++)
            {
                plusImage[i].GetComponent<Image>().color = _plusColor;
            }
            if (gameResult == Enums.GameResult.Win)
            {
                ChangeImageColor(plusImage[_oldScore], _plusColor);
                
                _newRequiredScore = defaultRequiredScore-oldScore-1;
            }
            else if(gameResult == Enums.GameResult.Lose)
            {
                ChangeImageColor(plusImage[_oldScore-1], _defaultColor);
                
                //승급까지 남은 판수 계산
                _newRequiredScore = defaultRequiredScore-oldScore+1;
            }
        }
        
        // 이번 게임 전 기존 점수가 마이너스 였을 경우
        else
        {
            for (int i = _oldScore; i < 0; i++)
            {
                minusImages[minusImages.Length+i].GetComponent<Image>().color = _minusColor;
            }
            if (gameResult == Enums.GameResult.Win)
            {
                ChangeImageColor(minusImages[minusImages.Length+_oldScore], _defaultColor);
                
                _newRequiredScore = defaultRequiredScore-oldScore-1;
            }
            else if(gameResult == Enums.GameResult.Lose)
            {
                ChangeImageColor(minusImages[minusImages.Length+_oldScore-1], _minusColor);
                
                _newRequiredScore = defaultRequiredScore-oldScore+1;
            }
        }
        
        if (gameResult == Enums.GameResult.Draw)
        {
            _newRequiredScore = defaultRequiredScore-oldScore;
        }

        SetScoreCountText();
    }

    /// <summary>
    /// 승급까지 남은 승수 계산
    /// </summary>
    /// <param name="_newRequiredScore">새로 업데이트 된 승급까지 필요한 승 수</param>
    /// <param name="_oldRequiredScore">해당 급수에서 0에서 승급까지 필요한 승수</param>
    private void SetScoreCountText()
    {
        // 남은 승리수가 0인 경우 승급점수 도달 혹은 강등점수 도달
        if (_newRequiredScore == 0 || _newRequiredScore == _oldRequiredScore * 2)
        {
            //새로운 급수에 맞춰서 패널 초기화하기
            scoreCountText.text = "";
        }
        else if (_newRequiredScore < 0)
        {
            scoreCountText.text = "더 이상 승급 할 수 없습니다.";
        }
        else if (_newRequiredScore > _oldRequiredScore * 2)
        {
            scoreCountText.text = "더이상 강등 될 수 없습니다.";
        }
        else
        {
            scoreCountText.text = $"{_newRequiredScore} 게임을 승리하면 승급하게 됩니다.";
        }
    }

    public void SetRatingUpLimit(int winCount)
    {
        for (int i = 0; i < 10; i++)
        {
            plusImage[i].GetComponent<Image>().color = _plusColor;
            scoreCountText.text = $"더 이상 승급 할 수 없습니다.\n누적 {winCount} 승 하셨습니다.";
        }
    }

    public void SetRatingDownLimit(int loseCount)
    {
        for (int i = 0; i < 3; i++)
        {
            minusImages[i].GetComponent<Image>().color = _minusColor;
            scoreCountText.text = $"더 이상 강등 될 수 없습니다.\n누적 {loseCount*-1} 패 하셨습니다.";
        }
    }
    
    //승급, 강등시 패널을 초기화해서 띄워주는 함수 추가
}
