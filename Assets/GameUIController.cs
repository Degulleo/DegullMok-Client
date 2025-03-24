using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private GameObject retryButton;
    public void OnClickConfirmButton()
    {
        GameManager.Instance.OnClickConfirmButton();
    }

    public void OnClickRetryButton()
    {
        GameManager.Instance.RetryGame();
    }

    public void OnClickSurrenderButton()
    {
        GameManager.Instance.panelManager.OpenConfirmPanel("항복 하시겠습니까?", () =>
        {
            //TODO: 서버에 항복 전달 및 기타 등등
            
            GameManager.Instance.ChangeToMainScene();
        });
    }

    public void OnClickSettingsButton()
    {
        GameManager.Instance.panelManager.OpenSettingsPanel();
    }

    public void GameOver()
    {
        retryButton.SetActive(true);
    }
}
