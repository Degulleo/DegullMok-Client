using System.Collections;
using System.Threading;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class WinEffectController : MonoBehaviour
{
    [SerializeField] private GameObject haloEffectImg;
    [SerializeField] private GameObject characterImg;
    [SerializeField] private GameObject[] shineEffectImg;
    [SerializeField] private GameObject[] circleEffectImg;
    [SerializeField] private TextMeshProUGUI bannerText;
    
    [SerializeField] private string fullText = "승리했습니다!"; // 원하는 문구를 인스펙터에서 설정 가능
    [SerializeField] private float interval = 0.1f; // 글자 추가 속도 조정 가능

    private int currentLength = 0;
    private CancellationTokenSource cancellationTokenSource;
    
    private void Start()
    {
        ShowWinEffect();
    }
    
    private void ShowWinEffect()
    {
        // 패널 활성화
        gameObject.SetActive(true);
        cancellationTokenSource = new CancellationTokenSource();
        
        ShowPanel(); // 패널 크기 확대 효과
        StartCoroutine(AnimateLoadingText()); // 텍스트 타이핑 효과
        RotateHaloObject(); // 돌아가는 광선 효과
        ScaleUpSparkles(); // 반짝이 효과
        Invoke(nameof(PopupObject), 0.3f); // 0.3초 후에 배너 효과 실행
    }

    // 패널 크기 및 페이드 변화
    private void ShowPanel()
    {
        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();  // CanvasGroup이 없다면 추가
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 1f);
        transform.DOScale(Vector3.zero, 0f);
        transform.DOScale(Vector3.one, 1f);
    }

    // 글자 하나씩 나타나는 타이핑 효과
    private IEnumerator AnimateLoadingText()
    {
        yield return new WaitForSeconds(1f);
        while (currentLength != fullText.Length)
        {
            currentLength = (currentLength + 1) % (fullText.Length + 1); // 글자 하나씩 추가
            bannerText.text = fullText.Substring(0, currentLength); // 부분 문자열 표시
            yield return new WaitForSeconds(interval);
        }
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
        // 스파클 효과 실행
        StartCoroutine(ScaleUpSparklesCoroutine());
    }

    private IEnumerator ScaleUpSparklesCoroutine()
    {
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

    public void HideWinEffect()
    {
        // 코루틴 취소 및 패널 숨기기
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
        }
        gameObject.SetActive(false);
    }
}
