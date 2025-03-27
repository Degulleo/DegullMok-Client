using UnityEngine;
using UnityEngine.UI;

public class SingleInteractableButtonHandler : MonoBehaviour
{
    [Tooltip("이 버튼이 한 번만 클릭되도록 제한할지 여부")]
    [SerializeField] private bool enableOneTimeClick = true;

    private Button _button;

    private bool hasBeenClicked = false;

    private void Awake()
    {
        // 버튼 컴포넌트 가져오기
        _button = GetComponent<Button>();

        if (_button != null && enableOneTimeClick)
        {
            // 기존 onClick 이벤트를 저장
            Button.ButtonClickedEvent originalOnClick = _button.onClick;

            _button.onClick = new Button.ButtonClickedEvent();

            _button.onClick.AddListener(() =>
            {
                if (!hasBeenClicked)
                {
                    hasBeenClicked = true;

                    for (int i = 0; i < originalOnClick.GetPersistentEventCount(); i++)
                    {
                        originalOnClick.Invoke();
                    }

                    // 버튼 비활성화
                    _button.interactable = false;
                }
            });
        }
    }
    // 버튼 상태 리셋
    public void ResetButton()
    {
        hasBeenClicked = false;
        if (_button != null)
        {
            _button.interactable = true;
        }
    }

}