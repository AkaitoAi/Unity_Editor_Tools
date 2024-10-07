using AkaitoAi.Singleton;
using System;
using UnityEngine;
using System.Collections;

namespace AkaitoAi.Advertisement
{
    public class AdsWrapper : SingletonPresistent<AdsWrapper>
    {
        #region Ads Calling
        public void ShowSmallBannerTopLeft()
        {
            //AdsController.Instance.ShowBannerAd_Admob(0);
        }
        public void ShowSmallBannerTopRight()
        {
            //AdsController.Instance.ShowBannerAd_Admob(1);
        }
        public void ShowMediumBannerBottomLeft()
        {
            //AdsController.Instance.ShowBannerAd_Admob(2);
        }
        public void ShowMediumBannerTopRight()
        {
            //AdsController.Instance.ShowBannerAd_Admob(3);
        }
        public void ShowAdaptiveBannerTop()
        {
            //AdsController.Instance.ShowBannerAd_Admob(4);
        }
        public void ShowAdaptiveBannerTopLeft()
        {
            //AdsController.Instance.ShowBannerAd_Admob(5);
        }
        public void ShowAdaptiveBannerTopRight()
        {
            //AdsController.Instance.ShowBannerAd_Admob(6);
        }
        public void ShowInterstitial()
        {
            //AdsController.Instance.ShowInterstitialAd_Admob();
        }

        //TODO Hide Ads Calling
        public void HideSmallBannerTopLeft()
        {
            //AdsController.Instance.HideBannerAd_Admob(0);
        }
        public void HideSmallBannerTopRight()
        {
            //AdsController.Instance.HideBannerAd_Admob(1);
        }
        public void HideMediumBannerBottomLeft()
        {
            //AdsController.Instance.HideBannerAd_Admob(2);
        }
        public void HideMediumBannerTopRight()
        {
            //AdsController.Instance.HideBannerAd_Admob(3);
        }
        public void HideAdaptiveBannerTop()
        {
            //AdsController.Instance.HideBannerAd_Admob(4);
        }
        public void HideAdaptiveBannerTopLeft()
        {
            //AdsController.Instance.HideBannerAd_Admob(5);
        }
        public void HideAdaptiveBannerTopRight()
        {
            //AdsController.Instance.HideBannerAd_Admob(6);
        }

        public void HideAll()
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
#if UNITY_EDITOR

            withoutInternet?.Invoke();
#else
            if (Application.internetReachability != NetworkReachability.NotReachable)
                withInternet?.Invoke();
            else withoutInternet?.Invoke();
#endif
        }

        public void InternetReachability(IEnumerator withInternet, Action withoutInternet)
        {
#if UNITY_EDITOR

            withoutInternet?.Invoke();
#else
             if (Application.internetReachability != NetworkReachability.NotReachable)
                StartCoroutine(withInternet);
            else withoutInternet?.Invoke();
#endif
        }

        public void InternetReachability(Action withInternet, IEnumerator withoutInternet)
        {
#if UNITY_EDITOR
            StartCoroutine(withoutInternet);
#else
            if (Application.internetReachability != NetworkReachability.NotReachable)
                withInternet?.Invoke();
            else StartCoroutine(withoutInternet);              
#endif
        }

        public void InternetReachability(IEnumerator withInternet, IEnumerator withoutInternet)
        {
#if UNITY_EDITOR

            StartCoroutine(withoutInternet);

#else
            if (Application.internetReachability != NetworkReachability.NotReachable)
                StartCoroutine(withInternet);
            else StartCoroutine(withoutInternet);
#endif
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
    }

    #endregion
}