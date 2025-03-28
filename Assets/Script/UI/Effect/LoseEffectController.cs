using System.Collections;
using System.Threading;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class LoseEffectController : EffectController
{
    [SerializeField] private GameObject characterOpenEyes;
    [SerializeField] private GameObject characterCloseEyes;
    [SerializeField] private GameObject depressedEffect;

    protected override string fullText => "패배했습니다";

    public override void ShowEffect(OnEffectPanelEnded onEffectPanelEnd)
    {
        AudioManager.Instance.PlayLoseSound(); //사운드 추가
        
        gameObject.SetActive(true);
        cancellationTokenSource = new CancellationTokenSource();
        onEffectPanelEnded = onEffectPanelEnd;

        ShowPanel();
        StartCoroutine(AnimateLoadingText());
        PopupDepressedEffect();
        Invoke(nameof(PopupBanner), 0.3f); // 0.3초 후에 배너 효과 실행
    }

    protected override void ShowPanel()
    {
        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 1f);
        bannerObj.transform.DOScale(Vector3.zero, 0f);
        bannerObj.transform.DOScale(Vector3.one, 1f);
    }

    private IEnumerator AnimateCharacterEyes()
    {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            characterOpenEyes.SetActive(false);
            characterCloseEyes.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            characterOpenEyes.SetActive(true);
            characterCloseEyes.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            characterOpenEyes.SetActive(false);
            characterCloseEyes.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            characterOpenEyes.SetActive(true);
            characterCloseEyes.SetActive(false);
            yield return new WaitForSeconds(2f);
        }
    }
    
    private void PopupBanner()
    {
        characterCloseEyes.SetActive(true);
        characterOpenEyes.SetActive(false);
        // 초기 크기 및 위치 설정
        characterCloseEyes.transform.localScale = Vector3.zero;
        characterCloseEyes.transform.localPosition = new Vector3(0f, -100f, 0f);

        // 크기 확대 + 위치 이동
        characterCloseEyes.transform.DOScale(Vector3.one * 1.5f, 0.5f)
            .SetEase(Ease.OutElastic); // 더 부드러운 탄성 효과

        characterCloseEyes.transform.DOLocalMoveY(120f, 0.5f)
            .SetEase(Ease.OutExpo) // 감속 곡선 적용
            .OnComplete(() =>
            {
                characterCloseEyes.transform.DOLocalMoveY(80f, 0.3f).SetEase(Ease.InOutSine); // 너무 급격한 반동 대신 부드러운 조정
            });

        // 크기 자연스럽게 원래대로 줄이기
        characterCloseEyes.transform.DOScale(Vector3.one * 1.4f, 0.3f)
            .SetEase(Ease.InOutQuad)
            .SetDelay(0.5f); // 위의 애니메이션이 끝난 후 실행

        // 회전 흔들림 효과 (좀 더 부드럽게)
        characterCloseEyes.transform.DOShakeRotation(0.5f, new Vector3(0, 0, 8f), 10, 90)
            .SetDelay(0.2f) // 살짝 더 길게 흔들도록 설정
            .OnComplete(() =>
        {
            // 애니메이션이 끝난 후 눈 깜빡이는 효과 실행
            StartCoroutine(AnimateCharacterEyes());
        });
    }

    private void PopupDepressedEffect()
    {
        depressedEffect.SetActive(true);
        RectTransform rectTransform = depressedEffect.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 초기 위치 설정
            rectTransform.anchoredPosition = new Vector2(0f, 500f);
            // 밑으로 내려오는 효과 설정
            rectTransform.DOAnchorPosY(150f, 1f).SetEase(Ease.OutExpo);
        }
    }
}
