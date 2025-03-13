using UnityEngine;
using UnityEngine.SceneManagement;

public class UserInfoResult
{
    public string id;
    public string email;
    public string nickname;
    public int rating;
    public int score;
    public int imageIndex;
    public int win;
    public int lose;
}

public class UserManager : Singleton<UserManager>
{
    public string UserId { get; private set; }
    public string Email { get; private set; }
    public string Nickname { get; private set; }
    public int Rating { get; private set; }
    public int Score { get; private set; }
    public int ImageIndex { get; private set; }
    public int Win { get; private set; }
    public int Lose { get; private set; }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }

    public void SetUserInfo(UserInfoResult userData)
    {
        if (userData == null)
        {
            Debug.Log("유저 데이터가 비어있습니다");
            return;
        }
        Debug.Log("유저정보"+userData.nickname);
        
        UserId = userData.id;
        Email = userData.email;
        Nickname = userData.nickname;
        Rating = userData.rating;
        Score = userData.score;
        ImageIndex = userData.imageIndex;
        Win = userData.win;
        Lose = userData.lose;
    }
}