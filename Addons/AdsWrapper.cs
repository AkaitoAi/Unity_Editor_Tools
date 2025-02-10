using AkaitoAi.Singleton;
using System;
using UnityEngine;
using System.Collections;

namespace AkaitoAi.Advertisement
{
    public class AdsWrapper : SingletonPresistent<AdsWrapper>
    {
        [SerializeField] private GameObject adsLoadingPanel;

        #region Ads Calling

        //Show Ads Calling
        public void ShowSmallBannerTop()
        {
            //AdsController.Instance.ShowBannerAd_Admob(0);
        }
        public void ShowSmallBannerTopLeft()
        {
            //AdsController.Instance.ShowBannerAd_Admob(1);
        }
        public void ShowSmallBannerTopRight()
        {
            //AdsController.Instance.ShowBannerAd_Admob(2);
        }
        public void ShowMediumBannerBottomLeft()
        {
            //AdsController.Instance.ShowBannerAd_Admob(3);
        }
        public void ShowMediumBannerBottomRight()
        {
            //AdsController.Instance.ShowBannerAd_Admob(4);
        }
        public void ShowAdaptiveBannerTop()
        {
            //AdsController.Instance.ShowBannerAd_Admob(5);
        }
        public void ShowCustomAdaptiveBannerTopLeft()
        {
            //AdsController.Instance.ShowBannerAd_Admob(6);
        }
        public void ShowCustomAdaptiveBannerTopRight()
        {
            //AdsController.Instance.ShowBannerAd_Admob(7);
        }

        public void ShowInterstitial()
        {
            //AdsController.Instance.ShowInterstitialAd_Admob();
        }

        public void ShowInterstitialWithLoading(Action Behaviour = null)
        {
            InternetReachability(()=> StartCoroutine(LoadInterstital()), () => Behaviour?.Invoke());

            IEnumerator LoadInterstital()
            {
                adsLoadingPanel.SetActive(true);

                yield return new WaitForSecondsRealtime(1.5f);

                //AdsController.Instance.ShowInterstitialAd_Admob();

                yield return new WaitForSecondsRealtime(.5f);

                adsLoadingPanel.SetActive(false);

                Behaviour?.Invoke();
            }
        }

        public void HideSmallBannerTop()
        {
            //AdsController.Instance.HideBannerAd_Admob(0);
        }
        public void HideSmallBannerTopLeft()
        {
            //AdsController.Instance.HideBannerAd_Admob(1);
        }
        public void HideSmallBannerTopRight()
        {
            //AdsController.Instance.HideBannerAd_Admob(2);
        }
        public void HideMediumBannerBottomLeft()
        {
            //AdsController.Instance.HideBannerAd_Admob(3);
        }
        public void HideMediumBannerBottomRight()
        {
            //AdsController.Instance.HideBannerAd_Admob(4);
        }
        public void HideAdaptiveBannerTop()
        {
            //AdsController.Instance.HideBannerAd_Admob(5);
        }
        public void HideCustomAdaptiveBannerTopLeft()
        {
            //AdsController.Instance.HideBannerAd_Admob(6);
        }
        public void HideCustomAdaptiveBannerTopRight()
        {
            //AdsController.Instance.HideBannerAd_Admob(7);
        }
        public void HideAllBanners()
        {
            //AdsController.Instance.HideAllBanners_Admob();
        }
        #endregion

        #region Internet Reachability

        public void InternetReachability(Action withInternet)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
                withInternet?.Invoke();
        }

        public void InternetReachability(Action withInternet, Action withoutInternet)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
                withInternet?.Invoke();
            else withoutInternet?.Invoke();
        }

        public void InternetReachability(IEnumerator withInternet, Action withoutInternet)
        {
             if (Application.internetReachability != NetworkReachability.NotReachable)
                StartCoroutine(withInternet);
            else withoutInternet?.Invoke();
        }

        public void InternetReachability(Action withInternet, IEnumerator withoutInternet)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
                withInternet?.Invoke();
            else StartCoroutine(withoutInternet);
        }

        public void InternetReachability(IEnumerator withInternet, IEnumerator withoutInternet)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
                StartCoroutine(withInternet);
            else StartCoroutine(withoutInternet);
        }

        #endregion

        #region Rewarded Ads
        public void ShowRewardedAdsLoading(Action reward,
            Action noInternet, Action adNotAvailable)
        {
#if UNITY_EDITOR

            reward?.Invoke();

#else
            if (Application.internetReachability
                == NetworkReachability.NotReachable) noInternet?.Invoke();
            //else AdsController.Instance.ShowRewardedAd_Loading_Admob(reward, adNotAvailable);
#endif
        }
        public void ShowRewardedAds(Action reward,
            Action noInternet, Action adNotAvailable)
        {
#if UNITY_EDITOR

            reward?.Invoke();

#else
            if (Application.internetReachability
                == NetworkReachability.NotReachable) noInternet?.Invoke();
            //else AdsController.Instance.ShowRewardedAd_Admob(reward, adNotAvailable);
#endif
        }

        public void ShowRewardedAds(Action reward, Action adNotAvailable)
        {
#if UNITY_EDITOR

            reward?.Invoke();

#else
            //AdsController.Instance.ShowRewardedAd_Admob(reward, adNotAvailable);
#endif
        }

        public void ShowRewardedAdsLoading(Action reward, Action adNotAvailable)
        {
#if UNITY_EDITOR

            reward?.Invoke();

#else
            //AdsController.Instance.ShowRewardedAd_Loading_Admob(reward, adNotAvailable);
#endif
        }

        #endregion

        #region Firebase
        public void FirebaseLog(string message)
        {
            InternetReachability(() =>
            {
                //FirebaseAnalyticsHandler.Instance.LogFirebaseEvent(message);
            });
        }
        public void FirebaseLog(string message1, string message2, string message3)
        {
            InternetReachability(() =>
            {
                //FirebaseAnalyticsHandler.Instance.LogFirebaseEvent(message1, message2, message3);
            });
        }
        public void FirebaseLogGroup(string eventName, int totalParameters, string[] parameterName, string[] parameterValue)
        {
            InternetReachability(() =>
            {
                //FirebaseAnalyticsHandler.Instance.LogFirebaseEvent_Group(eventName, totalParameters, parameterName, parameterValue);
            });
        }
        #endregion
    }

    public enum ADBannerSize
    {
        Banner = 0,
        MediumRectangle = 1,
        IABBanner = 2,
        Leaderboard = 3,
        AdaptiveSize = 4,
        AdaptiveCustomSize_500 = 5,
        Collapsible = 6
    }

    public enum AdPosition
    {
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }
}