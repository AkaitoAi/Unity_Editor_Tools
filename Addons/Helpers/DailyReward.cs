using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DailyReward : MonoBehaviour
{
    [SerializeField] private Text timeToWaitForNextReward;
    [SerializeField] private float msToWait = 86400000f; // 24 hours in milliseconds
    [SerializeField] private Button rewardButton, rewardAdButton;
    [SerializeField] private Image rewardIconImage;
    [SerializeField] private Text rewardAmountText;
    
    private ulong lastTimeClicked;

    [Space(10)]
    [SerializeField] private Reward[] rewards;

    [Space(10)]
    public UnityEvent OnRewardReadyEvent, OnRewardNotReadyEvent;

    private void Start()
    {
        if (PlayerPrefs.GetInt("FirstTime") == 0)
        {
            lastTimeClicked = (ulong)DateTime.Now.Ticks - (ulong)(msToWait * TimeSpan.TicksPerMillisecond);
            PlayerPrefs.SetString("LastTimeClicked", lastTimeClicked.ToString());
            PlayerPrefs.SetInt("FirstTime", 1);
            PlayerPrefs.Save();
        }
        else
        {
            lastTimeClicked = ulong.Parse(
                PlayerPrefs.GetString("LastTimeClicked", DateTime.Now.Ticks.ToString())
            );
        }

        GrantRewardByID(PlayerPrefs.GetInt("CasinoWheelGrantedRewardID"));

        //if (rewardButton != null)
        //{
        //    rewardButton.onClick.AddListener(() =>
        //    {
        //        SoundManager.Instance?.PlayOnButtonSound();
        //        OnRewardButton();
        //    });
        //}

        if (rewardAdButton != null)
        {
            rewardAdButton.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayOnButtonSound();
                OnUnlockRewardButton();
            });
        }
    }

    private void Update()
    {
        if (Ready())
        {
            timeToWaitForNextReward.text = "00:00:00";

            if (rewardButton)
            {
                rewardButton.interactable = true;
                rewardAdButton.gameObject.SetActive(true);
            }

            if (rewardAdButton)
            {
                rewardAdButton.interactable = false;
                rewardAdButton.gameObject.SetActive(false);
            }

            OnRewardReadyEvent?.Invoke();

            return;
        }

        ulong diff = ((ulong)DateTime.Now.Ticks - lastTimeClicked);
        ulong m = diff / TimeSpan.TicksPerMillisecond;
        float secondsLeft = (float)(msToWait - m) / 1000.0f;

        int hours = (int)secondsLeft / 3600;
        int minutes = ((int)secondsLeft % 3600) / 60;
        int seconds = (int)secondsLeft % 60;

        OnRewardNotReadyEvent?.Invoke();

        timeToWaitForNextReward.text = $"{hours:00}:{minutes:00}:{seconds:00}";

        if (rewardButton)
        {
            rewardButton.interactable = false;
            rewardAdButton.gameObject.SetActive(false);
        }

        if (rewardAdButton)
        {
            rewardAdButton.interactable = true;
            rewardAdButton.gameObject.SetActive(true);
        }
    }

    public void OnRewardButton()
    {
        lastTimeClicked = (ulong)DateTime.Now.Ticks;
        PlayerPrefs.SetString("LastTimeClicked", lastTimeClicked.ToString());
        PlayerPrefs.Save();
    }

    public void OnUnlockRewardButton()
    {
        lastTimeClicked = (ulong)DateTime.Now.Ticks - (ulong)(msToWait * TimeSpan.TicksPerMillisecond);
        PlayerPrefs.SetString("LastTimeClicked", lastTimeClicked.ToString());
        PlayerPrefs.Save();

        rewardButton.interactable = true;
        timeToWaitForNextReward.text = "00:00:00";
    }


    public bool Ready()
    {
        ulong diff = ((ulong)DateTime.Now.Ticks - lastTimeClicked);
        ulong m = diff / TimeSpan.TicksPerMillisecond;
        float secondsLeft = (float)(msToWait - m) / 1000.0f;
        return secondsLeft <= 0;
    }

    private void OnEnable()
    {
        CasinoWheel.OnSlotSelectedAction += (id) => { rewards[id].GrantReward();  GrantRewardByID(id); OnRewardButton(); };
    }

    private void OnDisable()
    {
        CasinoWheel.OnSlotSelectedAction -= (id) => { rewards[id].GrantReward(); GrantRewardByID(id); OnRewardButton(); };
    }


    private void GrantRewardByID(int id)
    {
        if (rewardIconImage != null)
        {
            rewardIconImage.sprite = rewards[id].icon;
            rewardIconImage.SetNativeSize();
            rewardIconImage.transform.localScale = Vector3.zero;
            rewardIconImage.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
        }

        if (rewardAmountText)
        {
            rewardAmountText.text = rewards[id].amount.ToString() + " COINS";
        }

        PlayerPrefs.SetInt("CasinoWheelGrantedRewardID", id);
    }

    [Serializable]
    public struct Reward
    {
        public int id;
        public int amount;
        public Sprite icon;

        public void GrantReward()
        {
            PlayerPrefs.SetInt(MenuManager.GetInstance()?.setupScriptable.totalCoinsPref,
                PlayerPrefs.GetInt(MenuManager.GetInstance()?.setupScriptable.totalCoinsPref) + amount);
            
            EventBus<OnCoinsCharged>.Raise(new OnCoinsCharged {});
        }
    }
}
