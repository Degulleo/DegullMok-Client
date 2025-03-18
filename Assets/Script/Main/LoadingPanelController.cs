using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanelController : MonoBehaviour
{
    [SerializeField] GameObject dragonImg;
    [SerializeField] GameObject tigerImg;
    [SerializeField] TextMeshProUGUI loadingText;

    [SerializeField] private Sprite[] dragonSprites;
    [SerializeField] private Sprite[] tigerSprites;
    
    [SerializeField] private string fullText = "불러오는 중..."; // 원하는 문구를 인스펙터에서 설정 가능
    [SerializeField] private float interval = 0.4f; // 글자 추가 속도 조정 가능
    [SerializeField] float flipDuration = 0.3f; // 회전 시간
    [SerializeField] float delayBetweenFlips = 1f; // 이미지 변경 주기
    
    private int currentLength = 0;
    
    // 타 컴포넌트에서 애니메이션 효과 설정을 위해 호출(RotateImages와 FlipImages 혼용은 불가능: DORotate가 서로 충돌함)
    public void SetAnimation(bool animatedImage, bool animatedText, bool animatedFlip)
    {
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
        while (true)
        {
            // 글자 하나씩 추가
            currentLength = (currentLength + 1) % (fullText.Length + 1); // 글자 하나씩 추가 (0 ~ fullText.Length 반복)
            loadingText.text = fullText.Substring(0, currentLength); // 부분 문자열 표시
            yield return new WaitForSeconds(interval); // 0.2초마다 변경

            // 모든 글자가 다 표시되면 1초 대기
            if (currentLength == fullText.Length)
            {
                yield return new WaitForSeconds(1f); // 1초 대기
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

        while (true)
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
}
