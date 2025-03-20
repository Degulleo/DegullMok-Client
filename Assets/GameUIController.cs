using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    public void OnClickConfirmButton()
    {
        GameManager.Instance.OnClickConfirmButton();
    }

    public void OnClickRetryButton()
    {
        GameManager.Instance.RetryGame();
    }
}
