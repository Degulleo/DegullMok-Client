using System;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class CoinsPanelController : MonoBehaviour
{
    [SerializeField] private GameObject _coinsRemoveImageObject;
    [SerializeField] private TMP_Text _coinsCountText;
    
    [SerializeField] private AudioClip _coinsRemoveAudioClip;
    [SerializeField] private AudioClip _coinsAddAudioClip;
    [SerializeField] private AudioClip _coinsEmptyAudioClip;
    
    private Color _coinsColor;
    
    private AudioSource _audioSource;
    
    private int _coinsCount;
    
    // 1. 코인 추가 연출
    // 2. 코인 감소 연출
    // 3. 코인 부족 연출

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _coinsColor = _coinsRemoveImageObject.GetComponent<Image>().color;
    }

    private void Start()
    {
        _coinsRemoveImageObject.SetActive(false);

        // TODO : 코인 수량 초기화
        // InitCoinsCount(<어디선가 가져와야함!>);
    }

    /// <summary>
    /// Coins Panel에 코인 수 초기화
    /// </summary>
    /// <param name="coinsCount">코인 수</param>
    public void InitCoinsCount(int coinsCount)
    {
        _coinsCount = coinsCount;
        _coinsCountText.text = _coinsCount.ToString();
    }

    private void ChangeTextAnimation(bool isAdd, Action action)
    {
        float duration = 0.2f;
        float yPos = 40f;
        
        _coinsCountText.rectTransform.DOAnchorPosY(-yPos, duration);
        _coinsCountText.DOFade(0, duration).OnComplete(() =>
        {
            if (isAdd)
            {
                var currentHeartCount = _coinsCountText.text;
                _coinsCountText.text = (int.Parse(currentHeartCount) + 1).ToString();
            }
            else
            {
                var currentHeartCount = _coinsCountText.text;
                _coinsCountText.text = (int.Parse(currentHeartCount) - 1).ToString();
            }
            
            // Coins Panel의 Width를 글자 수에 따라 변경
            var textLength = _coinsCountText.text.Length;
            GetComponent<RectTransform>().sizeDelta = new Vector2(100 + textLength * 30f, 100f);
            
            // 새로운 코인 수 추가 애니메이션
            _coinsCountText.rectTransform.DOAnchorPosY(yPos, 0);
            _coinsCountText.rectTransform.DOAnchorPosY(0, duration);
            _coinsCountText.DOFade(1, duration).OnComplete(() =>
            {
                action?.Invoke();
            });
        });
    }

    public void AddCoins(int coinsCount, Action action)
    {
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < coinsCount; i++)
        {
            sequence.AppendCallback(() =>
            {
                ChangeTextAnimation(true, ()=>
                {
                    // TODO : 코인 수량 업데이트
                    action?.Invoke();
                });
                
                // 효과음 재생
                // if (UserInformation.IsPlaySFX)
                //     _audioSource.PlayOneShot(_coinsAddAudioClip);
            });
            sequence.AppendInterval(0.5f);
        }
    }

    public void EmptyCoins()
    {
        // 효과음 재생
        // if (UserInformation.IsPlaySFX)
        //     _audioSource.PlayOneShot(_coinsEmptyAudioClip);
        
        GetComponent<RectTransform>().DOPunchPosition(new Vector3(20f, 0, 0), 1f, 7);
    }

    public void RemoveCoins(Action action)
    {
        // 효과음 재생
        // if (UserInformation.IsPlaySFX)
        //     _audioSource.PlayOneShot(_coinsRemoveAudioClip);
        
        // 코인 사라지는 연출
        _coinsRemoveImageObject.SetActive(true);
        _coinsRemoveImageObject.transform.localScale = Vector3.zero;
        _coinsRemoveImageObject.GetComponent<Image>().color = _coinsColor;
        
        _coinsRemoveImageObject.transform.DOScale(3f, 1f);
        _coinsRemoveImageObject.GetComponent<Image>().DOFade(0f, 1f)
            .OnComplete( ()=>ChangeTextAnimation(false, ()=>
            {
                // 코인 수량 감소
                // GameManager.Instance.heartCount--;
                action?.Invoke();
            }));   // 하트 수 텍스트 떨어지는 연출
    }
}
