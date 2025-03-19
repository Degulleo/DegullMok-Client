using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
    private RewardedInterstitialAd rewardedInterstitialAd;
    private string adUnitId = "ca-app-pub-3940256099942544/5354046379"; // 테스트 광고 ID

    void Start()
    {
        // Google Mobile Ads 초기화
        MobileAds.Initialize(initStatus => {  });

        // 광고 로드
        LoadRewardedInterstitialAd();
    }

    // 보상형 전면 광고 로드
    public void LoadRewardedInterstitialAd()
    {
        AdRequest request = new AdRequest();

        RewardedInterstitialAd.Load(adUnitId, request,
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                if (error != null)
                {
                    Debug.LogError("보상형 전면 광고 로드 실패: " + error);
                    return;
                }

                Debug.Log("보상형 전면 광고 로드 성공");
                rewardedInterstitialAd = ad;

                // 광고 종료 이벤트 설정
                rewardedInterstitialAd.OnAdFullScreenContentClosed += HandleAdClosed;
            });
    }

    // 보상형 전면 광고 실행
    public void ShowRewardedInterstitialAd()
    {
        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            rewardedInterstitialAd.Show((Reward reward) =>
            {
                // 코인 지급 로직
                GrantReward();
            });
        }
        else
        {
            Debug.Log("보상형 전면 광고가 아직 로드되지 않았습니다.");
        }
    }

    // 광고 닫힘 이벤트 처리
    private void HandleAdClosed()
    {
        Debug.Log("보상형 전면 광고 닫힘, 새로운 광고 로드.");
        LoadRewardedInterstitialAd(); // 광고가 닫힌 후 다시 로드
    }

    // 코인 지급 함수
    private void GrantReward()
    {
        NetworkManager.Instance.WatchAdForCoins((coinsAdded) =>
        {
            // UI 업데이트
            GameManager.Instance.UpdateCoinsPanelUI(coinsAdded);
        }, () =>
        {
            Debug.Log("광고 시청 후 코인 추가 실패!");
        });
    }
}