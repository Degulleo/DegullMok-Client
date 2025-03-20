using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainPanelControllerTestLIN : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI ratingText;
    [SerializeField] private Button signOutButton;
    [SerializeField] private GameObject replayPanel;

    public void UpdateUserInfo()
    {
        if (UserManager.Instance == null) return;

        nicknameText.text = UserManager.Instance.Nickname;
        ratingText.text = $"{UserManager.Instance.Rating}급";
    }

    public void OnSignOutClick()
    {
        NetworkManager.Instance.SignOut(() =>
        {
            Debug.Log("로그아웃 성공");
            
            // 로그인 화면
            GameManager.Instance.panelManager.OpenSigninPanel();
        }, () =>
        {
            Debug.Log("로그아웃 실패");
            // OpenConfirmPanel("로그아웃 되었습니다.", () => { });
        });
    }

    public void OnclickRecordButton()
    {
        Instantiate(replayPanel, GetComponent<Transform>());
    }
}
