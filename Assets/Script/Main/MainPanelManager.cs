using UnityEngine;
using System;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainPanelManager : MonoBehaviour
{
    [SerializeField] private LoadingPanelController loadingPanelController;
    [SerializeField] private MainPanelController mainPanelController;
    
    private UserManager _userManager;  // UserManager 인스턴스 관리

    private void Start()
    {
        if (UserManager.Instance == null)
        {
            GameObject userManagerObj = new GameObject("UserManager");
            _userManager = userManagerObj.AddComponent<UserManager>();
        }
        // 로딩 화면 추가(자동 로그인 응답 전까지)
        GameManager.Instance.panelManager.OpenLoadingPanel(false, true, true);
  
        // 자동 로그인
        TryAutoSignin();
    }

    private void TryAutoSignin()
    {
        NetworkManager.Instance.GetInfo((userInfo) =>
        {
            Debug.Log("자동 로그인 성공");
            
            UserManager.Instance.SetUserInfo(userInfo);
            
            UpdateMainPanelUI(GameManager.Instance.panelManager.OpenMainPanel);

            loadingPanelController.StopLoading();
        }, () =>
        {
            Debug.Log("자동 로그인 실패");
            // 로딩 멈추기
            loadingPanelController.StopLoading();
            // 로그인 화면
            GameManager.Instance.panelManager.OpenSigninPanel();
        });
    }
    
    /// <summary>
    /// 유저 별명, 급수를 서버에서 가져온 정보로 업데이트하여 메인화면에 표시
    /// </summary>
    public void UpdateMainPanelUI(Action success = null)
    {
        mainPanelController.UpdateUserInfo();
        success?.Invoke();
    }
}
