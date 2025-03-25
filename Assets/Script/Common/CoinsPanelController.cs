using System;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class CoinsPanelController : MonoBehaviour
{
    [SerializeField] private GameObject coinsRemoveImageObject;
    [SerializeField] private TMP_Text coinsCountText;
    
    [SerializeField] private AudioClip coinsRemoveAudioClip;
    [SerializeField] private AudioClip coinsAddAudioClip;
    [SerializeField] private AudioClip coinsEmptyAudioClip;

    private Color _coinsColor;
    private AudioSource _audioSource;
    private int _coinsCount;
    private RectTransform _coinsRect;
    private CanvasGroup _canvasGroup;   //부모 캔버스 그룹
    
    // 1. 코인 추가 연출
    // 2. 코인 감소 연출
    // 3. 코인 부족 연출

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _coinsColor = coinsRemoveImageObject.GetComponent<Image>().color;
        _coinsRect = GetComponent<RectTransform>();
        _canvasGroup = GetComponentInParent<CanvasGroup>();
    }

    private void Start()
    {
        coinsRemoveImageObject.SetActive(false);
    }

    /// <summary>
    /// Coins Panel에 코인 수 초기화
    /// </summary>
    /// <param name="coinsCount">코인 수</param>
    public void InitCoinsCount(int coinsCount)
    {
        _coinsCount = coinsCount;
        coinsCountText.text = _coinsCount.ToString();
        
        ResizingCoinsRect();
    }

    private void ResizingCoinsRect()
    {
        // Coins Panel의 Width를 글자 수에 따라 변경
        var textLength = coinsCountText.text.Length;
        _coinsRect.sizeDelta = new Vector2(100 + textLength * 30f, 100f);
    }

    private void ChangeTextAnimation(bool isAdd, Action action)
    {
        float duration = 0.2f;
        float yPos = 40f;
        
        coinsCountText.rectTransform.DOAnchorPosY(-yPos, duration);
        coinsCountText.DOFade(0, duration).OnComplete(() =>
        {
            if (isAdd)
            {
                var currentHeartCount = coinsCountText.text;
                coinsCountText.text = (int.Parse(currentHeartCount) + 500).ToString();
                // 코인 텍스트 100씩 증가
            }
            else
            {
                var currentHeartCount = coinsCountText.text;
                coinsCountText.text = (int.Parse(currentHeartCount) - 100).ToString();
                // 코인 텍스트 100씩 감소
            }
            
            // Coins Panel의 Width를 글자 수에 따라 변경
            ResizingCoinsRect();
            
            // 새로운 코인 수 추가 애니메이션
            coinsCountText.rectTransform.DOAnchorPosY(yPos, 0);
            coinsCountText.rectTransform.DOAnchorPosY(0, duration);
            coinsCountText.DOFade(1, duration).OnComplete(() =>
            {
                action?.Invoke();
            });
        });
    }

    /// <summary>
    /// 코인 추가 함수
    /// </summary>
    /// <param name="coinsCount"> 추가할 코인 수량</param>
    /// <param name="shopPanel">상점 패널 캔버스 그룹</param>
    /// <param name="action">애니메이션 종료 후 동작 EX) 코인 수량 변경</param>
    public void AddCoins(int coinsCount, Action action)
    {
        _canvasGroup.blocksRaycasts = false;   //코인 중복 추가 방지 코드
        
        Sequence sequence = DOTween.Sequence();
        // i += a  반복 횟수 조절, 100개 단위로 상승 차감 시 100으로 설정
        for (int i = 0; i < coinsCount; i+=500)
        {
            sequence.AppendCallback(() =>
            {
                ChangeTextAnimation(true, ()=>
                {
                    // TODO : 코인 수량 업데이트
                    action?.Invoke();
                });
                
                // 효과음 재생
                // TODO : if (UserInformation.IsPlaySFX)
                _audioSource.PlayOneShot(coinsAddAudioClip);
            });
            sequence.AppendInterval(0.5f);
        }
        sequence.OnComplete(() =>
        {
            _coinsCount += coinsCount;  //추가된 코인 적용
            _canvasGroup.blocksRaycasts = true;    //구매 후 클릭 활성화
        });
    }

    public void EmptyCoins()
    {
        // 효과음 재생
        // TODO: if (UserInformation.IsPlaySFX)
        _audioSource.PlayOneShot(coinsEmptyAudioClip);
        
        GetComponent<RectTransform>().DOPunchPosition(new Vector3(20f, 0, 0), 1f, 7);
    }

    /// <summary>
    /// 코인 제거 함수
    /// </summary>
    /// <param name="action"></param>
    public void RemoveCoins(Action action)
    {
        // --------------------------------------------------------------------
        // TODO : 임시 게임 매니저 혹은 별도 관리자가 관리해야함.
        if (_coinsCount < 100)
        {
            EmptyCoins();
            return;
        }

        // 효과음 재생
        // TODO: if (UserInformation.IsPlaySFX)
        _audioSource.PlayOneShot(coinsRemoveAudioClip);
        
        // 코인 사라지는 연출
        coinsRemoveImageObject.SetActive(true);
        coinsRemoveImageObject.transform.localScale = Vector3.zero;
        coinsRemoveImageObject.GetComponent<Image>().color = _coinsColor;
        
        coinsRemoveImageObject.transform.DOScale(3f, 1f);
        coinsRemoveImageObject.GetComponent<Image>().DOFade(0f, 1f)
            .OnComplete( ()=>ChangeTextAnimation(false, ()=>
            {
                //감소된 코인 적용
                _coinsCount -= 100;
                
                // GameManager.Instance.CoinsCount--;
                action?.Invoke();
            }));   // 텍스트 떨어지는 연출
    }
}
