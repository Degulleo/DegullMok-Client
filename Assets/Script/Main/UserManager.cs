using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public Image profileImage;
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
    public Image profileImage { get; private set; }

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
        
        UserId = userData.id;
        Email = userData.email;
        Nickname = userData.nickname;
        Rating = userData.rating;
        // Score = userData.score;
        ImageIndex = userData.imageIndex;
        // Win = userData.win;
        // Lose = userData.lose;
        
        // 유저 정보를 PlayerPrefs에 저장
        SaveUserInfoToPlayerPrefs();
    }
    
    public void SaveUserInfoToPlayerPrefs()
    {
        // UserInfoResult 객체를 JSON 문자열로 직렬화
        UserInfoResult userInfo = new UserInfoResult
        {
            id = UserId,
            email = Email,
            nickname = Nickname,
            // rating = Rating,
            // score = Score,
            imageIndex = ImageIndex,
            // win = Win,
            // lose = Lose
        };

        string json = JsonUtility.ToJson(userInfo);

        // PlayerPrefs에 저장
        PlayerPrefs.SetString("UserInfo", json);
        PlayerPrefs.Save();
    }
    
    public void LoadUserInfoFromPlayerPrefs()
    {
        // PlayerPrefs에서 유저 정보 가져오기
        string json = PlayerPrefs.GetString("UserInfo", "");

        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("저장된 유저 정보가 없습니다.");
            return;
        }

        // JSON 문자열을 UserInfoResult 객체로 역직렬화
        UserInfoResult userInfo = JsonUtility.FromJson<UserInfoResult>(json);

        // 역직렬화한 데이터로 UserManager의 값을 설정
        SetUserInfo(userInfo);
    }


}