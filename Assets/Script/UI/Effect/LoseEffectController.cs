using System.Collections;
using TMPro;
using UnityEngine;

public class LoseEffectController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bannerText;
    
    [SerializeField] private string fullText = "패배했습니다"; // 원하는 문구를 인스펙터에서 설정 가능
    [SerializeField] private float interval = 0.1f; // 글자 추가 속도 조정 가능

    private int currentLength = 0;
    
    private void Start()
    {
        ShowWinEffect();
    }
    
    private void ShowWinEffect()
    {
        // 패널 활성화
        gameObject.SetActive(true);
        // cancellationTokenSource = new CancellationTokenSource();
        
        StartCoroutine(AnimateLoadingText()); // 텍스트 타이핑 효과
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
}
