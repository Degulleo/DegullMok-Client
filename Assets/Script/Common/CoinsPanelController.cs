using System;
using UnityEngine;
using DG.Tweening;
using TMPro;
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
    
    // 1. 코인 추가 연출
    // 2. 코인 감소 연출
    // 3. 코인 부족 연출

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _coinsColor = coinsRemoveImageObject.GetComponent<Image>().color;
    }

    private void Start()
    {
        coinsRemoveImageObject.SetActive(false);

        // TODO : 코인 수량 초기화
        InitCoinsCount(0);
        
        // TODO: 임시 코드, 해당 플레이어의 코인을 서버에서 가져오면 삭제 가능. 맨 처음에 InitCoinsCount를 500으로 하면 텍스트 정렬 이슈 있음
        AddCoins(5, () => { });
    }

    /// <summary>
    /// Coins Panel에 코인 수 초기화
    /// </summary>
    /// <param name="coinsCount">코인 수</param>
    public void InitCoinsCount(int coinsCount)
    {
        _coinsCount = coinsCount;
        coinsCountText.text = _coinsCount.ToString();
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
                coinsCountText.text = (int.Parse(currentHeartCount) + 100).ToString();
                // 코인 텍스트 100씩 증가
            }
            else
            {
                var currentHeartCount = coinsCountText.text;
                coinsCountText.text = (int.Parse(currentHeartCount) - 100).ToString();
                // 코인 텍스트 100씩 감소
            }
            
            // Coins Panel의 Width를 글자 수에 따라 변경
            var textLength = coinsCountText.text.Length;
            GetComponent<RectTransform>().sizeDelta = new Vector2(100 + textLength * 30f, 100f);
            
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
    /// <param name="action">애니메이션 종료 후 동작 EX) 코인 수량 변경</param>
    public void AddCoins(int coinsCount, Action action)
    {
        Sequence sequence = DOTween.Sequence();

        // i += a  반복 횟수 조절, 100개 단위로 상승 차감 시 100으로 설정
        for (int i = 0; i < coinsCount; i+=1)
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
                // TODO: 코인 수량 감소
                // GameManager.Instance.heartCount--;
                action?.Invoke();
            }));   // 텍스트 떨어지는 연출
    }
}
