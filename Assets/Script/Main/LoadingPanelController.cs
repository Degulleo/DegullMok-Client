using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanelController : MonoBehaviour
{
    [SerializeField] GameObject dragonImg;
    [SerializeField] GameObject tigerImg;
    [SerializeField] TextMeshProUGUI loadingText;

    [SerializeField] private Sprite[] dragonSprites;
    [SerializeField] private Sprite[] tigerSprites;
    
    private string _fullText = "불러오는 중...";
    private readonly string[] _loadingMessages = {
        "흑돌을 닦는 중...",
        "백돌을 닦는 중...",
        "알을 반짝반짝 닦는 중...",
        "방석을 터는 중...",
        "오목 룰북을 읽는 중...",
        "한 수 깊이 생각하는 중...",
        "흑돌과 백돌을 구분하는 중...",
        "돌이 동글동글한지 확인하는 중..."
    };

    private string _watingText = "상대방을 기다리는 중...";
    private float interval = 0.2f;
    [SerializeField] float flipDuration = 0.3f; // 회전 시간
    [SerializeField] float delayBetweenFlips = 1f; // 이미지 변경 주기

    [SerializeField] private GameObject imageBackground;
    [SerializeField] private GameObject simpleBackground;
    
    private int currentLength = 0;
    private CancellationTokenSource cancellationTokenSource;
    
    // 타 컴포넌트에서 애니메이션 효과 설정을 위해 호출(RotateImages와 FlipImages 혼용은 불가능: DORotate가 서로 충돌함)
    /// <summary>
    /// 로딩 패널 보이기
    /// </summary>
    /// <param name="animatedImage">캐릭터들이 좌우로 회전하는 효과</param>
    /// <param name="animatedText">한글자씩 나타나는 효과</param>
    /// <param name="animatedFlip">캐릭터들이 뒤집히면서 표정이 바뀌는 효과</param>
    /// <param name="isBackgroundImage">배경 이미지 여부 설정</param>
    public void StartLoading(bool animatedImage, bool animatedText, bool animatedFlip, bool isBackgroundImage)
    {
        // 패널 활성화
        gameObject.SetActive(true);
        cancellationTokenSource = new CancellationTokenSource();
        
        // 배경 이미지 여부 설정
        imageBackground.SetActive(isBackgroundImage);
        simpleBackground.SetActive(!isBackgroundImage);

        _fullText = !isBackgroundImage ? _watingText : _fullText;  // 인게임에서 사용하는 경우 맨 처음 텍스트 고정

        if (animatedImage) RotateImages(); // 캐릭터들이 좌우로 회전하는 효과
        if (animatedText) StartCoroutine(AnimateLoadingText()); // 한글자씩 나타나는 효과
        if (animatedFlip) FlipImages(); // 캐릭터들이 뒤집히면서 표정이 바뀌는 효과
    }

    // 캐릭터들이 좌우로 회전하는 효과
    private void RotateImages()
    {
        float maxRotation = 10f; // 한쪽으로 기울일 최대 각도
        float duration = 1f;     // 한 방향으로 가는 시간

        // 초기 회전값을 +15도로 설정
        dragonImg.transform.rotation = Quaternion.Euler(0f, 0f, maxRotation);
        tigerImg.transform.rotation = Quaternion.Euler(0f, 0f, maxRotation);

        // 좌우로 왕복 회전 애니메이션
        dragonImg.transform
            .DORotate(new Vector3(0f, 0f, -maxRotation), duration) // 15도 → -15도
            .SetEase(Ease.InOutBack)
            .SetLoops(-1, LoopType.Yoyo);

        tigerImg.transform
            .DORotate(new Vector3(0f, 0f, -maxRotation), duration) // -15도 → 15도
            .SetEase(Ease.InOutBack)
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    // 한글자씩 나타나는 효과
    private IEnumerator AnimateLoadingText()
    {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            // 글자 하나씩 추가
            currentLength = (currentLength + 1) % (_fullText.Length + 1); // 글자 하나씩 추가 (0 ~ fullText.Length 반복)
            loadingText.text = _fullText.Substring(0, currentLength); // 부분 문자열 표시
            yield return new WaitForSeconds(interval); // 0.2초마다 변경

            // 모든 글자가 다 표시되면 1초 대기
            if (currentLength == _fullText.Length)
            {
                yield return new WaitForSeconds(1f); // 1초 대기
                _fullText = _loadingMessages[Random.Range(0, _loadingMessages.Length)]; // 랜덤 메시지 선택
                currentLength = 0; // 다시 처음부터 시작
            }

        }
    }

    // 캐릭터들이 뒤집히면서 표정이 바뀌는 효과
    private void FlipImages()
    {
        GameObject[] images = { dragonImg, tigerImg };
        foreach (var image in images)
        {
            StartCoroutine(FlipRoutine(image, image == dragonImg ? dragonSprites : tigerSprites));
        }
    }
    
    private IEnumerator FlipRoutine(GameObject component, Sprite[] spriteSet)
    {
        var imageComponent = component.gameObject.GetComponent<Image>();

        while (!cancellationTokenSource.IsCancellationRequested)
        {
            yield return new WaitForSeconds(delayBetweenFlips);

            // 1️⃣ Y축 -90도 회전
            yield return component.transform.DOLocalRotate(new Vector3(0f, 90f, 0f), flipDuration / 2)
                .SetEase(Ease.InBounce)
                .WaitForCompletion();

            // 2️⃣ 랜덤한 스프라이트 선택하여 적용
            if (imageComponent != null) 
            {
                imageComponent.sprite = spriteSet[Random.Range(0, spriteSet.Length)];
            }

            // 3️⃣ Y축 0도로 회전
            yield return component.transform.DOLocalRotate(Vector3.zero, flipDuration / 2)
                .SetEase(Ease.OutBounce)
                .WaitForCompletion();
        }
    }
    
    public void StopLoading()
    {
        // 코루틴 취소
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
        }
        
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }
}
