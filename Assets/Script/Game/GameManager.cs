using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject signinPanel;
    [SerializeField] private GameObject signupPanel;
    
    private Canvas _canvas;
    
    private void Start()
    {
        // 로그인
        OpenSigninPanel();
    }
    
    public void OpenSigninPanel()
    {
        if (_canvas != null)
        {
            var signinPanelObject = Instantiate(signinPanel, _canvas.transform);
        }
    }

    public void OpenSignupPanel()
    {
        if (_canvas != null)
        {
            var signupPanelObject = Instantiate(signupPanel, _canvas.transform);
        }
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _canvas = GameObject.FindObjectOfType<Canvas>();
    }
}
