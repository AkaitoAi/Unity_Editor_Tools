using UnityEngine;
using UnityEngine.UI;
using System;

namespace AkaitoAi.Advertisement
{
    public class DailyReward7 : MonoBehaviour
    {
        public int secondsToWait = 86400; // 24hrs
        public bool canGetReward = true;
        [Space]
        public Reward[] rewardButtons = new Reward[7];
        public Text displayText;

        private ulong lastRewarded;
        private ulong difference;
        private ulong milisec;
        private float milisecToWait;
        private float secondsLeft;

        [SerializeField] private GameObject dailyRewardPanel, rewardedAdBtn, claimNowBtn;
        [SerializeField] private GameObject rewardGrantedPrefab;
        [SerializeField] private string coinPref = "TotalCoins";
        bool flag;

        [System.Serializable]
        public struct Reward
        {
            public Button button;
            public int reward;
        }

        private void OnEnable()
        {
            CheckOnStart();
        }
        private void Start()
        {
            flag = true;

            string lastSavedTime = PlayerPrefs.GetString("LastRewarded", "0");
            lastRewarded = ulong.Parse(lastSavedTime);

            //Match the variables.
            if (!CanGetReward())
            {
                canGetReward = false;
                rewardedAdBtn.SetActive(true);
            }
            else if (CanGetReward())
            {
                canGetReward = true;
                dailyRewardPanel.SetActive(true);
            }

            if (claimNowBtn.TryGetComponent<Button>(out Button claimBtn))
                claimBtn.onClick.AddListener(OnGetRewardButton);

            if (rewardedAdBtn.TryGetComponent<Button>(out Button adBtn))
                adBtn.onClick.AddListener(OnRewardAdButton);

        }

        private void Update()
        {
            //Matching the different variables.
            canGetReward = CanGetReward();

            //Our reward is ready.
            if (canGetReward)
            {
                if (displayText)
                    displayText.text = "Claim Today's Reward!";

                if (rewardButtons[PlayerPrefs.GetInt("Day")].button)
                {
                    rewardButtons[PlayerPrefs.GetInt("Day")].button.interactable = true;

                    //TODO Effects for Active Reward Button
                    claimNowBtn.SetActive(true);

                }

                if (flag)
                {
                    flag = false;
                }
            }

            //We cannot get reward at the moment.
            else if (!canGetReward)
            {
                if (rewardButtons[PlayerPrefs.GetInt("Day")].button)
                    rewardButtons[PlayerPrefs.GetInt("Day")].button.interactable = false;

                string timerText = "";

                timerText += ((int)secondsLeft / 3600).ToString("00") + "h ";
                secondsLeft -= ((int)secondsLeft / 3600) * 3600;

                timerText += ((int)secondsLeft / 60).ToString("00") + "m ";

                timerText += (secondsLeft % 60).ToString("00") + "s";

                if (displayText)
                    displayText.text = "Reward in \"" + timerText + "\"";
            }
        }


        void DisableButtonEffects()
        {
            for (int i = 0; i <= PlayerPrefs.GetInt("Day"); i++)
            {
                rewardButtons[i].button.interactable = false;

                //TODO Disable Effect of other Reward Buttons
            }
        }

        public void ShowRewrdMsg(int day, int reward, string rewardType)
        {
            //TODO Show Message On Reward Granted

            //RewardPrefabs.transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<Text>().text = reward.ToString() + rewardType;
            //RewardPrefabs.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Text>().text = "YOU GOT YOUR DAY " + PlayerPrefs.GetInt("Day").ToString() + "  REWARD";

            //rewardParticlePrefab.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>().text = "YOU GOT REWARD " + reward.ToString() + "  COINS";

            if (rewardGrantedPrefab.transform.GetChild(0).GetChild(0).TryGetComponent<Text>(out Text textComp))
                textComp.text = "YOU GOT REWARD " + reward.ToString() + "  COINS";

            //rewardParticlePrefab.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>().text = "YOU GOT REWARD " + reward.ToString() + "  COINS";

            Instantiate(rewardGrantedPrefab);
        }

        public void OnGetRewardButton()
        {
            DisableButtonEffects();
            rewardButtons[PlayerPrefs.GetInt("Day")].button.enabled = false;

            if (canGetReward)
            {
               ShowRewrdMsg(PlayerPrefs.GetInt("Day"), rewardButtons[PlayerPrefs.GetInt("Day")].reward, "Coins");
                //..Reward....................
               PlayerPrefs.SetInt(coinPref, PlayerPrefs.GetInt(coinPref) + rewardButtons[PlayerPrefs.GetInt("Day")].reward);

               if (PlayerPrefs.GetInt("Day") == 6)
               {
                    PlayerPrefs.SetInt("Day", 0);
               }

                rewardedAdBtn.SetActive(true);
                PlayerPrefs.SetInt("Day", PlayerPrefs.GetInt("Day") + 1);

                rewardButtons[PlayerPrefs.GetInt("Day")].button.interactable = true;

                PlayerPrefs.SetInt("ClaimDay" + PlayerPrefs.GetInt("Day"), 1);

                lastRewarded = (ulong)DateTime.Now.Ticks;
                PlayerPrefs.SetString("LastRewarded", lastRewarded.ToString());

                canGetReward = false;
            }
        }
        public void SetInteractableFalse()
        {
            for (int i = 0; i < rewardButtons.Length; i++)
            {
                rewardButtons[i].button.interactable = false;
            }
        }
        public void CheckOnStart()
        {
            for (int i = 0; i < rewardButtons.Length; i++)
                rewardButtons[i].button.interactable = false;

            rewardButtons[PlayerPrefs.GetInt("ClaimDay") + PlayerPrefs.GetInt("Day")].button.onClick.AddListener(OnGetRewardButton);
            rewardButtons[PlayerPrefs.GetInt("ClaimDay") + PlayerPrefs.GetInt("Day")].button.interactable = true;
        }
        public void ResetAll()
        {
            for (int i = 0; i < rewardButtons.Length; i++)
            {
                PlayerPrefs.SetInt("ClaimDay" + i, 0);
            }
        }
        public void CheckRewardPanelEnableDisable()
        {
            if (CanGetReward())
            {
                canGetReward = true;
                dailyRewardPanel.SetActive(true);
            }
        }

        private bool CanGetReward()
        {
            //Getting the difference between the current time and the <LastRewarded> time.
            difference = ((ulong)DateTime.Now.Ticks - lastRewarded);
            milisec = difference / TimeSpan.TicksPerMillisecond;

            //Since the input wait time is in seconds, we have to multiply by 1000 to get it in miliseconds.
            milisecToWait = secondsToWait * 1000;
            secondsLeft = (float)(milisecToWait - milisec) / 1000f;

            //Check if we can get the reward.
            if (secondsLeft < 0)
            {
                rewardedAdBtn.SetActive(false);
                //PlayerPrefs.SetInt("Day", 0);

                if (PlayerPrefs.GetInt("Day") == 0)
                {
                    SetInteractableFalse();
                    ResetAll();
                }

                return true;
            }
            else return false;
        }

        public void OnRewardAdButton()
        {
            AdsWrapper.GetInstance()
                .ShowRewardedAds(Reward
                , () => MenuManager.Instance.noInternetScreen.SetActive(true)
                , () => MenuManager.Instance.noAdScreen.SetActive(true));

            void Reward()
            {
                rewardButtons[PlayerPrefs.GetInt("ClaimDay") + PlayerPrefs.GetInt("Day")].button.onClick.AddListener(OnGetRewardButton);
                rewardButtons[PlayerPrefs.GetInt("ClaimDay") + PlayerPrefs.GetInt("Day")].button.interactable = true;
                PlayerPrefs.SetString("LastRewarded", "0");
                string lastSavedTime = PlayerPrefs.GetString("LastRewarded");
                lastRewarded = ulong.Parse(lastSavedTime);
            }
        }
    }
}
