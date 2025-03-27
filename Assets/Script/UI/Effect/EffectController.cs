using System.Collections;
using System.Threading;
using DG.Tweening;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

[RequireComponent(typeof(CanvasGroup))]
public abstract class EffectController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] protected GameObject bannerObj;
    [SerializeField] protected TextMeshProUGUI bannerText;
    protected abstract string fullText { get; } // 각 클래스에서 다르게 설정할 값
    [SerializeField] protected float interval = 0.1f; // 타이핑 속도

    protected CancellationTokenSource cancellationTokenSource;
    protected int currentLength = 0;
    
    public delegate void OnEffectPanelEnded();
    protected OnEffectPanelEnded onEffectPanelEnded;

    // protected virtual void Start()
    // {
    //     ShowEffect();
    // }

    // 효과를 실행하는 메서드 (자식이 구현해야 함)
    public abstract void ShowEffect([CanBeNull] OnEffectPanelEnded onEffectPanelEnded);

    // 공통 UI 애니메이션 (패널 표시)
    protected virtual void ShowPanel()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 1f);
        transform.DOScale(Vector3.zero, 0f);
        transform.DOScale(Vector3.one, 1f);
    }

    // 텍스트 타이핑 효과
    protected IEnumerator AnimateLoadingText()
    {
        yield return new WaitForSeconds(1f);
        while (currentLength != fullText.Length)
        {
            currentLength = (currentLength + 1) % (fullText.Length + 1);
            bannerText.text = fullText.Substring(0, currentLength);
            yield return new WaitForSeconds(interval);
        }
    }

    // 효과 종료 (자식에서도 사용할 수 있도록 protected로 선언)
    public virtual void HideEffect()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
        
        onEffectPanelEnded?.Invoke();
        gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HideEffect();
    }
}