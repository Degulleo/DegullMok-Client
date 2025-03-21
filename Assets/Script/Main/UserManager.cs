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
    public int coins;
}

public class CoinsInfoResult
{
    public string result;
    public int coins;
}

/// <summary>
/// 코인 구매 응답 클래스 
/// </summary>
public class CoinsPurchaseResult
{
    public string result;      
    public string message;     
    public int purchased;      // 충전된 코인량
    public int currentCoins;
}
/// <summary>
/// 광고 시청 응답 클래스
/// </summary>
public class CoinsAdResult
{
    public string result;      
    public string message;     
    public int recharged;      // 충전된 코인량
    public int currentCoins;
}

/// <summary>
/// 코인 구매 요청 데이터 클래스
/// </summary>
public class PurchaseData
{
    public int amount;
    public string paymentId;
    public string paymentType;

    public PurchaseData(int amount, string paymentId, string paymentType)
    {
        this.amount = amount;
        this.paymentId = paymentId;
        this.paymentType = paymentType;
    }
}

/// <summary>
/// 코인 차감 응답 데이터 클래스
/// </summary>
public class DeductCoinsResult
{
    public string result;
    public string message;
    public int deducted;
    public int remainingCoins;
}



public class UserManager : Singleton<UserManager>
{
    public string UserId { get; private set; }
    public string Email { get; private set; }
    public string Nickname { get; private set; }
    public int Rating { get; private set; }
    public int Score { get; private set; }
    public int imageIndex { get; private set; }
    public int Win { get; private set; }
    public int Lose { get; private set; }
    public int Coins { get; private set; }

    // 효과음 재생 여부
    public static bool IsPlaySFX
    {
        get { return PlayerPrefs.GetInt("IsPlaySFX", 1) == 1; }
        set { PlayerPrefs.SetInt("IsPlaySFX", value ? 1 : 0); }
    }
    
    // 배경음악 재생 여부
    public static bool IsPlayBGM
    {
        get { return PlayerPrefs.GetInt("IsPlayBGM", 1) == 1; }
        set { PlayerPrefs.SetInt("IsPlayBGM", value ? 1 : 0); }
    }
    
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }

    public void UserInfoInit()
    {
        UserId = "";
        Email = "";
        Nickname = "";
        Rating = 0;
        imageIndex = 0;
        Coins = 0;
    }

    /// <summary>
    /// GetInfo 호출 시
    /// </summary>
    /// <param name="userData"></param>
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
        imageIndex = userData.imageIndex;
        // Win = userData.win;
        // Lose = userData.lose;
        Coins = userData.coins;
        
        // 유저 정보를 PlayerPrefs에 저장
        SaveUserInfoToPlayerPrefs();
    }
    
    
    /// <summary>
    /// Signin 호출 시 : 로그인 정보만 반영
    /// </summary>
    /// <param name="signinResult"></param>
    public void SetUserInfo(SigninResult signinResult)
    {
        Nickname = signinResult.nickname;
        Rating = signinResult.rating;
        // Score = signinResult.score;
        imageIndex = signinResult.imageIndex;
        Coins = signinResult.coins;
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
            imageIndex = imageIndex,
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

    public void SetCoinsInfo()
    {
        NetworkManager.Instance.GetCoinsInfo((coinsResult) =>
        {
            Coins = coinsResult.coins;
        }, () =>
        {
            Debug.Log("서버에서 코인 불러오기 실패");
        });
    }
}