using System.Collections;
using System.Threading;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class WinEffectController : EffectController
{
    [SerializeField] private GameObject haloEffectImg;
    [SerializeField] private GameObject characterImg;
    [SerializeField] private GameObject[] shineEffectImg;
    [SerializeField] private GameObject[] circleEffectImg;

    protected override string fullText => "승리했습니다!";

    public override void ShowEffect(OnEffectPanelEnded onEffectPanelEnd)
    {
        AudioManager.Instance.PlayWinSound(); // 사운드 추가
        gameObject.SetActive(true);
        cancellationTokenSource = new CancellationTokenSource();
        onEffectPanelEnded = onEffectPanelEnd;
        
        ShowPanel();
        StartCoroutine(AnimateLoadingText());
        RotateHaloObject();
        ScaleUpSparkles();
        Invoke(nameof(PopupObject), 0.3f);
    }

    protected override void ShowPanel()
    {
        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 1f);
        bannerObj.transform.DOScale(Vector3.zero, 0f);
        bannerObj.transform.DOScale(Vector3.one, 1f);
    }
    
    private void RotateHaloObject()
    {
        // 무한 회전 효과
        haloEffectImg.transform
            .DORotate(new Vector3(0f, 0f, 360f), 3f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void PopupObject()
    {
        if (!gameObject.activeInHierarchy) return; // 게임 오브젝트가 비활성화 상태면 실행 안 함
        
        characterImg.SetActive(true);

        // 초기 크기 및 위치 설정
        characterImg.transform.localScale = Vector3.zero;
        characterImg.transform.localPosition = new Vector3(0f, -100f, 0f);

        // 크기 확대 + 위치 이동
        characterImg.transform.DOScale(Vector3.one * 1.5f, 0.5f)
            .SetEase(Ease.OutElastic); // 더 부드러운 탄성 효과

        characterImg.transform.DOLocalMoveY(120f, 0.5f)
            .SetEase(Ease.OutExpo) // 감속 곡선 적용
            .OnComplete(() =>
            {
                characterImg.transform.DOLocalMoveY(80f, 0.3f).SetEase(Ease.InOutSine); // 너무 급격한 반동 대신 부드러운 조정
            });

        // 크기 자연스럽게 원래대로 줄이기
        characterImg.transform.DOScale(Vector3.one * 1.4f, 0.3f)
            .SetEase(Ease.InOutQuad)
            .SetDelay(0.5f); // 위의 애니메이션이 끝난 후 실행

        // 회전 흔들림 효과 (좀 더 부드럽게)
        characterImg.transform.DOShakeRotation(0.5f, new Vector3(0, 0, 8f), 10, 90)
            .SetDelay(0.2f); // 살짝 더 길게 흔들도록 설정
    }

    private void ScaleUpSparkles()
    {
        if (!gameObject.activeInHierarchy) return; // 게임 오브젝트가 비활성화 상태면 실행 안 함
        
        // 스파클 효과 실행
        StartCoroutine(ScaleUpSparklesCoroutine());
    }

    private IEnumerator ScaleUpSparklesCoroutine()
    {
        if (!gameObject.activeInHierarchy) yield return null; // 게임 오브젝트가 비활성화 상태면 실행 안 함
        
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            // 각 스파클 효과 실행
            yield return StartCoroutine(ScaleUpEffectCoroutine(shineEffectImg));
            yield return StartCoroutine(ScaleUpEffectCoroutine(circleEffectImg));
            
            yield return new WaitForSeconds(0.3f);
        }
    }

    private IEnumerator ScaleUpEffectCoroutine(GameObject[] effectArray)
    {
        foreach (GameObject effect in effectArray)
        {
            effect.transform.localScale = Vector3.zero;
            effect.transform.DOScale(Vector3.one * 1.5f, 0.3f).SetEase(Ease.OutBack);
            effect.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutBounce).SetDelay(0.3f);
            yield return new WaitForSeconds(0.3f);
        }
    }
}
