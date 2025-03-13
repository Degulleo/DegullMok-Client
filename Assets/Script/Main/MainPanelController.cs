using System;
using TMPro;
using UnityEngine;

public class MainPanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI ratingText;

    public void UpdateUserInfo()
    {
        if (UserManager.Instance == null) return;

        nicknameText.text = UserManager.Instance.Nickname;
        ratingText.text = $"{UserManager.Instance.Rating}급";
    }
}
