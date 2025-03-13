using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

public class AdmobAdsManager : Singleton<AdmobAdsManager>
{
#if UNITY_ANDROID
    private string _bannerAdUnitId = "ca-app-pub-3245096053779548~5309609957";          // Test ID
    private string _interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";    // Test ID
    private string _rewardAdUnitId = "ca-app-pub-3940256099942544/5224354917";          // Test ID
#elif UNITY_IOS
    // IOS ID,  IOS 빌드 추가할 경우 수정
    private string _bannerAdUnitId = "ca-app-pub-3940256099942544/2934735716";          // Test ID
    private string _interstitialAdUnitId = "ca-app-pub-3940256099942544/4411468910";    // Test ID
    private string _rewardAdUnitId = "ca-app-pub-3940256099942544/1712485313";          // Test ID
#endif

    // TODO: 위처럼 전처리기 이용해 빌드에 따라 변경되어야함
    private string _rewardAdUnitId = "ca-app-pub-3940256099942544/5224354917";          // Test ID
    // ---------------------------------------------------------------------

    // private BannerView _bannerView;
    // private InterstitialAd _interstitialAd;
    private RewardedAd _rewardedAd;
    
    private void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            // 리워드 광고 로드
            LoadRewardedAd();
        });
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }

    #region Rewarded Ads

    /// <summary>
    /// Loads the rewarded ad.
    /// </summary>
    public void LoadRewardedAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        RewardedAd.Load(_rewardAdUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " + "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());

                _rewardedAd = ad;
                RegisterRewardedAdEventHandlers(_rewardedAd);
            });
    }
    
    public void ShowRewardedAd()
    {
        const string rewardMsg =
            "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                // TODO: Reward the user.
                // 여기서 유저에게 보상 제공.
                // 실제 보상은 admob 대시보드에서 설정한 후 지급하지만
                // 편의를 위해 여기서 임의로 보상 내용 작성
                // GameManager.Instance.AddHeartCount(3);
                Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
            });
        }
    }
    
    private void RegisterRewardedAdEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            LoadRewardedAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " + "with error : " + error);
            LoadRewardedAd();
        };
    }
    
    #endregion
    
}
