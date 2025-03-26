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
        
        //TODO: 무승부인 경우 남은 승리 수 계산
        if (gameResult == Enums.GameResult.Draw)
        {
            _newRequiredScore = defaultRequiredScore-oldScore;
        }

        SetScoreCountText(_newRequiredScore,defaultRequiredScore);
    }

    private void SetScoreCountText(int scoreCount,int defaultRequiredScore)
    {
        if (scoreCount == 0 || scoreCount >= defaultRequiredScore * 2)
            scoreCountText.text = "";
        else
            scoreCountText.text = $"{scoreCount} 게임을 승리하면 승급하게 됩니다.";
        //강등되는 경우에도 텍스트 처리
    }
    
    //승급, 강등시 패널을 초기화해서 띄워주는 함수 추가
}
