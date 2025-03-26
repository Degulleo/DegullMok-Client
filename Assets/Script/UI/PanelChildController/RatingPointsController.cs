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
    public void InitRatingPoints(int oldScore,Enums.GameResult gameResult, int defaultRequiredScore)
    {
        // TODO: [인덱스계산 ㅇㅖ외처리 ] 계산한 값 절대값이 defaultRequiredScore보다 큰 경우 return. 근데 이런 값이 나온다는게 이미 계산 오류가 어디서 생긴 것이겠죠..?
        // 그런건 아니고 18급에서 강등 안됨 1급에서 승급 안됨 계산을 해야되네예.... 근데 이건 rating panel controller에서 걸러서 보내면 될듯.! 
        _oldScore = oldScore;
        Sequence sequence = DOTween.Sequence();
        if (_oldScore == 0)
        {
            if (gameResult == Enums.GameResult.Win)
            {
                sequence.Append(
                    plusImage[0].GetComponent<Transform>().DOLocalRotate(new Vector3(0f, 90f, 0f), flipDuration).SetEase(Ease.InExpo));
                sequence.Append(
                    plusImage[0].GetComponent<Transform>().DOLocalRotate(Vector3.zero, flipDuration).SetEase(Ease.OutExpo));
                sequence.Join(
                    plusImage[0].GetComponent<Image>().DOColor(_plusColor, flipDuration/2).SetEase(Ease.OutExpo));
                
                //승급까지 남은 판수 계산
                _newRequiredScore = defaultRequiredScore-1;
            }
            else if(gameResult == Enums.GameResult.Lose)
            {
                sequence.Append(
                    minusImages[defaultRequiredScore-1].GetComponent<Transform>().DOLocalRotate(new Vector3(0f, 90f, 0f), flipDuration).SetEase(Ease.InExpo));
                sequence.Append(
                    minusImages[defaultRequiredScore-1].GetComponent<Transform>().DOLocalRotate(Vector3.zero, flipDuration).SetEase(Ease.OutExpo));
                sequence.Join(
                    minusImages[defaultRequiredScore-1].GetComponent<Image>().DOColor(_minusColor, flipDuration/2).SetEase(Ease.OutExpo));
                
                //승급까지 남은 판수 계산
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
                sequence.Append(
                plusImage[_oldScore].GetComponent<Transform>().DOLocalRotate(new Vector3(0f, 90f, 0f), flipDuration).SetEase(Ease.InExpo));
                sequence.Append(
                    plusImage[_oldScore].GetComponent<Transform>().DOLocalRotate(Vector3.zero, flipDuration).SetEase(Ease.OutExpo));
                sequence.Join(
                    plusImage[_oldScore].GetComponent<Image>().DOColor(_plusColor, flipDuration/2).SetEase(Ease.OutExpo));
                
                //승급까지 남은 판수 계산
                _newRequiredScore = defaultRequiredScore-oldScore-1;
            }
            else if(gameResult == Enums.GameResult.Lose)
            {
                sequence.Append(
                    plusImage[_oldScore-1].GetComponent<Transform>().DOLocalRotate(new Vector3(0f, 90f, 0f), flipDuration).SetEase(Ease.InExpo));
                sequence.Append(
                    plusImage[_oldScore-1].GetComponent<Transform>().DOLocalRotate(Vector3.zero, flipDuration).SetEase(Ease.OutExpo));
                sequence.Join(
                    plusImage[_oldScore-1].GetComponent<Image>().DOColor(_defaultColor, flipDuration/2).SetEase(Ease.OutExpo));
                
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
                sequence.Append(
                    minusImages[minusImages.Length+_oldScore].GetComponent<Transform>().DOLocalRotate(new Vector3(0f, 90f, 0f), flipDuration).SetEase(Ease.InExpo));
                sequence.Append(
                    minusImages[minusImages.Length+_oldScore].GetComponent<Transform>().DOLocalRotate(Vector3.zero, flipDuration).SetEase(Ease.OutExpo));
                sequence.Join(
                    minusImages[minusImages.Length+_oldScore].GetComponent<Image>().DOColor(_defaultColor, flipDuration/2).SetEase(Ease.OutExpo));
                
                //승급까지 남은 판수 계산
                _newRequiredScore = defaultRequiredScore-oldScore-1;
            }
            else if(gameResult == Enums.GameResult.Lose)
            {
                sequence.Append(
                    minusImages[minusImages.Length+_oldScore-1].GetComponent<Transform>().DOLocalRotate(new Vector3(0f, 90f, 0f), flipDuration).SetEase(Ease.InExpo));
                sequence.Append(
                    minusImages[minusImages.Length+_oldScore-1].GetComponent<Transform>().DOLocalRotate(Vector3.zero, flipDuration).SetEase(Ease.OutExpo));
                sequence.Join(
                    minusImages[minusImages.Length+_oldScore-1].GetComponent<Image>().DOColor(_minusColor, flipDuration/2).SetEase(Ease.OutExpo));
                
                //승급까지 남은 판수 계산
                _newRequiredScore = defaultRequiredScore-oldScore+1;
            }
        }
        
        if (gameResult == Enums.GameResult.Draw)
        {
            _newRequiredScore = defaultRequiredScore-oldScore;
        }

        SetScoreCountText(_newRequiredScore,defaultRequiredScore);
    }

    private void SetScoreCountText(int scoreCount,int defaultRequiredScore)
    {
        // 남은 승리수가 0인 경우 승급
        if (scoreCount == 0)
        {
            scoreCountText.text = "";
        }
        else if (scoreCount < 0)
        {
            scoreCountText.text = "더 이상 승급 할 수 없습니다.";
        }
        else if (scoreCount >= defaultRequiredScore * 2)
        {
            scoreCountText.text = "더이상 강등 될 수 없습니다.";
        }
        else
        {
            scoreCountText.text = $"{scoreCount} 게임을 승리하면 승급하게 됩니다.";
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
