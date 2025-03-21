using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class NetworkManager : Singleton<NetworkManager>
{
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }
    
    public void Signup(SignupData signupData, Action success, Action failure)
    {
        StartCoroutine(SignupCoroutine(signupData, success, failure));
    }
    public IEnumerator SignupCoroutine(SignupData signupData, Action success, Action failure)
    {
        string jsonString = JsonUtility.ToJson(signupData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/signup", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + www.error);

                if (www.responseCode == 409)
                {
                    Debug.Log("중복사용자");
                    // 중복 사용자 생성 팝업 표시
                    GameManager.Instance.panelManager.OpenConfirmPanel("이미 존재하는 사용자입니다.", () =>
                    {
                        failure?.Invoke();
                    });
                }
            }
            else
            {
                var result = www.downloadHandler.text;
                success?.Invoke();
                
                // 회원가입 성공 팝업 표시
                GameManager.Instance.panelManager.OpenConfirmPanel("회원 가입이 완료 되었습니다.", () =>
                {
                    success?.Invoke();
                });
            }
        }
    }
    
    public void Signin(SigninData signinData, Action<SigninResult> success, Action<int> failure)
    {
        StartCoroutine(SigninCoroutine(signinData, success, failure));
    }

    public IEnumerator SigninCoroutine(SigninData signinData, Action<SigninResult> success, Action<int> failure)
    {
        string jsonString = JsonUtility.ToJson(signinData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/signin", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + www.error);
            }
            else
            {
                var cookie = www.GetResponseHeader("set-cookie");
                if (!string.IsNullOrEmpty(cookie))
                {
                    int lastIndex = cookie.LastIndexOf(";");
                    string sid = cookie.Substring(0, lastIndex);
                    PlayerPrefs.SetString("sid", sid); 
                }
                
                var result = www.downloadHandler.text;
                var signinResult = JsonUtility.FromJson<SigninResult>(result);

                if (signinResult.result == 0)
                {
                    Debug.Log("유저 이메일이 유효하지 않습니다.");
                    failure?.Invoke(0);
                    // 유저 이메일 유효하지 않음 팝업 표시
                    GameManager.Instance.panelManager.OpenConfirmPanel("이메일이 유효하지 않습니다.", () =>
                    {
                        failure?.Invoke(0);
                    });
                }
                else if (signinResult.result == 1)
                {
                    Debug.Log("패스워드가 유효하지 않습니다.");
                    failure?.Invoke(1);
                    // 패스워드가 유효하지 않음 팝업 표시
                    GameManager.Instance.panelManager.OpenConfirmPanel("패스워드가 유효하지 않습니다.", () =>
                    {
                        failure?.Invoke(1);
                    });
                }
                else if (signinResult.result == 2)
                {
                    Debug.Log("로그인에 성공하였습니다.");
                    success?.Invoke(signinResult);
                    
                    // 성공 팝업 표시
                    // GameManager.Instance.panelManager.OpenConfirmPanel("로그인에 성공하였습니다.", () =>
                    // {
                    //     success?.Invoke();
                    // });
                }
            }
        }
    }

    public void GetInfo(Action<UserInfoResult> success, Action failure)
    {
        StartCoroutine(GetInfoCoroutine(success, failure));
    }

    public IEnumerator GetInfoCoroutine(Action<UserInfoResult> success, Action failure)
    {
        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/get-info", UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            string sid = PlayerPrefs.GetString("sid", "");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }
            else
            {
                Debug.LogError("SID 값이 없습니다. 로그인 정보가 없습니다.");
                // GameManager.Instance.panelManager.OpenConfirmPanel("SID 값이 없습니다. 로그인 정보가 없습니다.", () =>
                // {
                // });
                failure?.Invoke();
                yield break; // 더 이상 진행하지 않고 종료
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                if (www.responseCode == 403)
                {
                    Debug.Log("로그인이 필요합니다.");
                    GameManager.Instance.panelManager.OpenConfirmPanel("로그인이 필요합니다.", () =>
                    {
                        failure?.Invoke();
                    });
                }
            }
            else
            {
                var result = www.downloadHandler.text;
                var userInfo = JsonUtility.FromJson<UserInfoResult>(result);
                
                success?.Invoke(userInfo);
            }
        }
    }
    
    public void SignOut(Action success, Action failure)
    {
        StartCoroutine(SignOutCoroutine(success, failure));
    }

    public IEnumerator SignOutCoroutine(Action success, Action failure)
    {
        string sid = PlayerPrefs.GetString("sid", "");
        if (string.IsNullOrEmpty(sid))
        {
            Debug.Log("로그인 정보가 없습니다.");
            GameManager.Instance.panelManager.OpenConfirmPanel("로그인이 필요합니다.", () =>
            {
                failure?.Invoke();
            });
            yield break; // 로그인이 되어 있지 않다면 로그아웃을 시도하지 않음
        }

        using (UnityWebRequest www = new UnityWebRequest(Constants.ServerURL + "/users/signout", UnityWebRequest.kHttpVerbPOST))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cookie", sid);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                if (www.responseCode == 403)
                {
                    Debug.Log("로그인이 필요합니다.");
                    GameManager.Instance.panelManager.OpenConfirmPanel("로그인이 필요합니다.", () => { });
                }
                failure?.Invoke();
            }
            else
            {
                var result = www.downloadHandler.text;
                // 로그아웃 후 sid를 삭제하여 세션을 클리어
                PlayerPrefs.SetString("sid", "");
                // 유저 정보도 삭제
                PlayerPrefs.SetString("UserInfo", "");
                UserManager.Instance.UserInfoInit();

                success?.Invoke();
            }
        }
    }

    public IEnumerator GetLeaderboard(Action<Scores> success, Action failure)
    {
        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/leaderboard", UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            
            string sid = PlayerPrefs.GetString("sid", "");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                if (www.responseCode == 403)
                {
                    Debug.Log("로그인이 필요합니다.");
                    GameManager.Instance.panelManager.OpenConfirmPanel("로그인이 필요합니다.", () => { });
                }
                
                failure?.Invoke();
            }
            else
            {
                var result = www.downloadHandler.text;
                var scores = JsonUtility.FromJson<Scores>(result);
                
                success?.Invoke(scores);
            }
        }
    }
    
    
    public void GetCoinsInfo(Action<CoinsInfoResult> success, Action failure)
    {
        StartCoroutine(GetCoinsInfoCoroutine(success, failure));
    }

    public IEnumerator GetCoinsInfoCoroutine(Action<CoinsInfoResult> success, Action failure)
    {
        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/coins", UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            string sid = PlayerPrefs.GetString("sid", "");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }
            else
            {
                Debug.LogError("SID 값이 없습니다. 로그인 정보가 없습니다.");
                GameManager.Instance.panelManager.OpenConfirmPanel("SID 값이 없습니다. 로그인 정보가 없습니다.", () =>
                {
                    failure?.Invoke();
                });
                yield break; // 더 이상 진행하지 않고 종료
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                if (www.responseCode == 403)
                {
                    Debug.Log("로그인이 필요합니다.");
                    GameManager.Instance.panelManager.OpenConfirmPanel("로그인이 필요합니다.", () => { });
                }
                
                failure?.Invoke();
            }
            else
            {
                var result = www.downloadHandler.text;
                var coinsInfo = JsonUtility.FromJson<CoinsInfoResult>(result);
                
                success?.Invoke(coinsInfo);
            }
        }
    }
    
    /// <summary>
    /// 광고 보상 함수
    /// </summary>
    /// <param name="success"></param>
    /// <param name="failure"></param>
    public void WatchAdForCoins(Action<int> success, Action failure)
    {
        StartCoroutine(WatchAdForCoinsCoroutine(success, failure));
    }

    private IEnumerator WatchAdForCoinsCoroutine(Action<int> success, Action failure)
    {
        string jsonString = "{\"adCompleted\": true}";  //광고가 끝난 후 함수가 호출되기 때문에 true 고정
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/coins/recharge/ad", UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            string sid = PlayerPrefs.GetString("sid", "");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }
            else
            {
                Debug.LogError("SID 값이 없습니다. 로그인 정보가 없습니다.");
                failure?.Invoke();
                yield break;
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("광고 시청 후 코인 충전 실패: " + www.error);
                failure?.Invoke();
            }
            else
            {
                var result = www.downloadHandler.text;
                var rechargeResult = JsonUtility.FromJson<CoinsAdResult>(result);

                if (rechargeResult.result == "SUCCESS")
                {
                    Debug.Log("광고 시청으로 코인 충전 완료: " + rechargeResult.recharged);
                    UserManager.Instance.SetCoinsInfo();
                    success?.Invoke(rechargeResult.recharged);
                }
                else
                {
                    Debug.Log("광고 시청 후 충전 실패: " + rechargeResult.result);
                    failure?.Invoke();
                }
            }
        }
    }
    
    
    /// <summary>
    /// 코인 구매 함수
    /// </summary>
    /// <param name="amount">충전양</param>
    /// <param name="paymentId">결제ID(??)</param>
    /// <param name="paymentType">결제타입(카드,구글페이)</param>
    /// <param name="success"></param>
    /// <param name="failure"></param>
    public void PurchaseCoins(int amount, string paymentId, string paymentType, Action<int> success, Action failure)
    {
        StartCoroutine(PurchaseCoinsCoroutine(amount, paymentId, paymentType, success, failure));
    }

    private IEnumerator PurchaseCoinsCoroutine(int amount, string paymentId, string paymentType, Action<int> success, Action failure)
    {
        string url = Constants.ServerURL + "/coins/purchase"; // 서버 엔드포인트
        PurchaseData purchaseData = new PurchaseData(amount, paymentId, paymentType);
        string jsonString = JsonUtility.ToJson(purchaseData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);
        
        using (UnityWebRequest www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            string sid = PlayerPrefs.GetString("sid", "");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }
            else
            {
                Debug.LogError("SID 값이 없습니다. 로그인 정보가 없습니다.");
                failure?.Invoke();
                yield break;
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("결제 요청 실패: " + www.error);
                failure?.Invoke();
            }
            else
            {
                var result = www.downloadHandler.text;
                var purchaseResult = JsonUtility.FromJson<CoinsPurchaseResult>(result);

                if (purchaseResult.result == "SUCCESS")
                {
                    Debug.Log($"결제 완료 {purchaseResult.purchased} 코인 충전됨, 현재 코인: {purchaseResult.currentCoins}");

                    // 유저 데이터 갱신
                    UserManager.Instance.SetCoinsInfo();

                    // 최신 코인 개수를 성공 콜백으로 전달
                    success?.Invoke(purchaseResult.purchased);
                }
                else
                {
                    Debug.LogError("결제 후 코인 충전 실패: " + purchaseResult.result);
                    failure?.Invoke();
                }
            }
        }
    }
    
    public void DeductCoins(Action<int> success, Action<string> failure)
    {
        StartCoroutine(DeductCoinsCoroutine(success, failure));
    }

    private IEnumerator DeductCoinsCoroutine(Action<int> success, Action<string> failure)
    {
        string DeductCoinsUrl = Constants.ServerURL + "/coins/deduct";
        
        using (UnityWebRequest www = new UnityWebRequest(DeductCoinsUrl, UnityWebRequest.kHttpVerbPOST))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            string sid = PlayerPrefs.GetString("sid", "");

            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }
            else
            {
                Debug.LogError("SID 값이 없습니다. 로그인 정보가 없습니다.");
                failure?.Invoke("LOGIN_REQUIRED");
                yield break;
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || 
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("코인 차감 실패: " + www.error);

                if (www.responseCode == 400)
                {
                    failure?.Invoke("INSUFFICIENT_COINS");
                }
                else
                {
                    failure?.Invoke("ERROR");
                }
            }
            else
            {
                var result = www.downloadHandler.text;
                var deductResult = JsonUtility.FromJson<DeductCoinsResult>(result);

                if (deductResult.result == "SUCCESS")
                {
                    Debug.Log("코인 차감 완료: " + deductResult.deducted);
                    UserManager.Instance.SetCoinsInfo();
                    success?.Invoke(deductResult.deducted);
                }
                else
                {
                    Debug.LogError("코인 차감 실패: " + deductResult.result);
                    failure?.Invoke(deductResult.result);
                }
            }
        }
    }
}
