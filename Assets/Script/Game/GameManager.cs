using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject signinPanel;
    [SerializeField] private GameObject signupPanel;
    
    [SerializeField] private Canvas canvas;
    private UserManager _userManager;  // UserManager 인스턴스 관리
    
    private void Awake()
    {
        // UserManager가 없으면 생성
        if (UserManager.Instance == null)
        {
            GameObject userManagerObj = new GameObject("UserManager");
            _userManager = userManagerObj.AddComponent<UserManager>();
        }
    }
    
    private void Start()
    {
        // 자동 로그인
        TryAutoSignin();
    }
    
    private void TryAutoSignin()
    {
        NetworkManager.Instance.GetInfo((userInfo) =>
        {
            Debug.Log("자동 로그인 성공");
            
            UpdateMainPanelUI();
            // ScoreData.SetScore(userInfo.score);
            // OpenConfirmPanel(userInfo.nickname + "님 로그인 성공하였습니다.", () => { });
        }, () =>
        {
            Debug.Log("자동 로그인 실패");
            // 로그인 화면
            OpenSigninPanel();
        });
    }
    
    private void UpdateMainPanelUI()
    {
        MainPanelController mainPanel = FindObjectOfType<MainPanelController>();
        if (mainPanel != null)
        {
            mainPanel.UpdateUserInfo();
        }
    }
    
    public void OpenSigninPanel()
    {
        if (canvas != null)
        {
            var signinPanelObject = Instantiate(signinPanel, canvas.transform);
        }
    }

    public void OpenSignupPanel()
    {
        if (canvas != null)
        {
            var signupPanelObject = Instantiate(signupPanel, canvas.transform);
        }
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }
}
