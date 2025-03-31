using System.Collections;
using System.Threading;
using UnityEngine;
using DG.Tweening;

public class DrawEffectController : EffectController
{
    [SerializeField] private GameObject dragonOpenEyes;
    [SerializeField] private GameObject dragonCloseEyes;
    [SerializeField] private GameObject tigerOpenEyes;
    [SerializeField] private GameObject tigerCloseEyes;
    [SerializeField] private float flipDuration = 0.3f;
    protected override string fullText => "무승부 입니다";
    
    public override void ShowEffect(OnEffectPanelEnded onEffectPanelEnd)
    {
        AudioManager.Instance.PlayDrawSound(); // 사운드 추가
        gameObject.SetActive(true);
        
        cancellationTokenSource = new CancellationTokenSource();
        onEffectPanelEnded = onEffectPanelEnd;
        
        ShowPanel();
        StartCoroutine(AnimateLoadingText());
        Invoke(nameof(PopupBanner), 0.3f);
    }
    
    private IEnumerator AnimateCharacterEyes()
    {
        if (!gameObject.activeInHierarchy) yield return null; // 게임 오브젝트가 비활성화 상태면 실행 안 함

        while (!cancellationTokenSource.IsCancellationRequested)
        {
            yield return PlayBlinkAnimation(dragonOpenEyes, dragonCloseEyes);
        
            // Flip으로 캐릭터 눈감을 때 뒤집으면서 변경됨
            yield return dragonCloseEyes.transform.DOLocalRotate(new Vector3(0f, 90f, 0f), flipDuration / 2)
                .SetEase(Ease.InBounce)
                .WaitForCompletion();

            dragonCloseEyes.SetActive(false);
            tigerCloseEyes.transform.localPosition = new Vector3(0f, 80f, 0f);
            tigerCloseEyes.SetActive(true);

            yield return tigerCloseEyes.transform.DOLocalRotate(Vector3.zero, flipDuration / 2)
                .SetEase(Ease.OutBounce)
                .WaitForCompletion();

            yield return PlayBlinkAnimation(tigerOpenEyes, tigerCloseEyes);

            yield return tigerCloseEyes.transform.DOLocalRotate(new Vector3(0f, 90f, 0f), flipDuration / 2)
                .SetEase(Ease.InBounce)
                .WaitForCompletion();

            tigerCloseEyes.SetActive(false);
            dragonCloseEyes.SetActive(true);

            yield return dragonCloseEyes.transform.DOLocalRotate(Vector3.zero, flipDuration / 2)
                .SetEase(Ease.OutBounce)
                .WaitForCompletion();
        }
    }

    // 눈 깜빡이는 애니메이션을 메서드로 분리
    private IEnumerator PlayBlinkAnimation(GameObject openEye, GameObject closeEye)
    {
        if (!gameObject.activeInHierarchy) yield return null; // 게임 오브젝트가 비활성화 상태면 실행 안 함
        
        for (int i = 0; i < 2; i++)
        {
            openEye.SetActive(false);
            closeEye.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            openEye.SetActive(true);
            closeEye.SetActive(false);
            yield return new WaitForSeconds(1f);
        }
        openEye.SetActive(false);
        closeEye.SetActive(true);
        yield return new WaitForSeconds(0.5f);
    }
    
    private void PopupBanner()
    {
        if (!gameObject.activeInHierarchy) return; // 게임 오브젝트가 비활성화 상태면 실행 안 함
        
        tigerCloseEyes.SetActive(false);
        tigerOpenEyes.SetActive(false);
        dragonCloseEyes.SetActive(true);
        dragonOpenEyes.SetActive(false);
        
        // 초기 크기 및 위치 설정
        dragonCloseEyes.transform.localScale = Vector3.zero;
        dragonCloseEyes.transform.localPosition = new Vector3(0f, -100f, 0f);

        // 크기 확대 + 위치 이동
        dragonCloseEyes.transform.DOScale(Vector3.one * 1.5f, 0.5f)
            .SetEase(Ease.OutElastic); // 더 부드러운 탄성 효과

        dragonCloseEyes.transform.DOLocalMoveY(120f, 0.5f)
            .SetEase(Ease.OutExpo) // 감속 곡선 적용
            .OnComplete(() =>
            {
                dragonCloseEyes.transform.DOLocalMoveY(80f, 0.3f).SetEase(Ease.InOutSine); // 너무 급격한 반동 대신 부드러운 조정
            });

        // 크기 자연스럽게 원래대로 줄이기
        dragonCloseEyes.transform.DOScale(Vector3.one * 1.4f, 0.3f)
            .SetEase(Ease.InOutQuad)
            .SetDelay(0.5f); // 위의 애니메이션이 끝난 후 실행

        // 회전 흔들림 효과 (좀 더 부드럽게)
        dragonCloseEyes.transform.DOShakeRotation(0.5f, new Vector3(0, 0, 8f), 10, 90)
            .SetDelay(0.2f) // 살짝 더 길게 흔들도록 설정
            .OnComplete(() =>
            {
                if (gameObject.activeInHierarchy) // 실행 전에 다시 확인
                    StartCoroutine(AnimateCharacterEyes()); // 애니메이션이 끝난 후 눈 깜빡이는 효과 실행
            });
    }
}