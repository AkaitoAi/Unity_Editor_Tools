using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private Image fillImage;  // Reference to the Image component

    [SerializeField]
    private RectTransform playBtn, privacyBtn, rateusBtn, gamesBtn, settingBtn, shopBtn, rewardBtn, adBtn;

    private List<Tweener> tweeners = new List<Tweener>();
    private Stack<Tweener> reverseStack = new Stack<Tweener>();

    private Vector2 privacyBtnStart, rateusBtnStart, gamesBtnStart, settingBtnStart;
    private Vector2 rewardBtnStart, adBtnStart, playBtnStart, shopBtnStart;

    private int activeForwardTweens = 0;
    private int activeReverseTweens = 0;

    public delegate void TweenCompleteHandler();
    public static event TweenCompleteHandler OnAllTweensComplete;
    public static event TweenCompleteHandler OnAllReverseTweensComplete;

    private void Awake()
    {
        // Store initial positions using anchoredPosition
        privacyBtnStart = privacyBtn.anchoredPosition;
        rateusBtnStart = rateusBtn.anchoredPosition;
        gamesBtnStart = gamesBtn.anchoredPosition;
        settingBtnStart = settingBtn.anchoredPosition;

        rewardBtnStart = rewardBtn.anchoredPosition;
        adBtnStart = adBtn.anchoredPosition;

        playBtnStart = playBtn.anchoredPosition;
        shopBtnStart = shopBtn.anchoredPosition;
    }

    private void Start()
    {
        OpenMenu();
    }

    private void OnEnable()
    {
        OpenMenu(); // Restart tweens when re-enabling the menu
    }

    public void OpenMenu()
    {
        menu.SetActive(true);

        MenuManager.GetInstance()?.eventSystem.SetActive(false);

        // Set the fill image to 0 when entering the panel and then animate to 1
        fillImage.fillAmount = 0f;
        DOTween.To(() => fillImage.fillAmount, x => fillImage.fillAmount = x, 1f, 1f).SetEase(Ease.OutBack);  // Fill animation from 0 to 1

        tweeners.Clear();
        reverseStack.Clear();
        activeForwardTweens = 0;

        ResetButtonPositions(); // Reset positions before starting tweens

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        AddTween(privacyBtn, new Vector2(screenWidth * 0.3f, 0), 0f);
        AddTween(rateusBtn, new Vector2(screenWidth * 0.3f, 0), 0.1f);
        AddTween(gamesBtn, new Vector2(screenWidth * 0.3f, 0), 0.2f);
        AddTween(settingBtn, new Vector2(screenWidth * 0.3f, 0), 0.3f);

        AddTween(rewardBtn, new Vector2(-screenWidth * 0.3f, 0), 0.1f);
        AddTween(adBtn, new Vector2(-screenWidth * 0.3f, 0), 0.2f);

        AddTween(playBtn, new Vector2(0, screenHeight * 0.3f), 0.4f);
        AddTween(shopBtn, new Vector2(0, screenHeight * 0.3f), 0.4f);

        PlayTweens();
    }

    private void ResetButtonPositions()
    {
        privacyBtn.anchoredPosition = privacyBtnStart;
        rateusBtn.anchoredPosition = rateusBtnStart;
        gamesBtn.anchoredPosition = gamesBtnStart;
        settingBtn.anchoredPosition = settingBtnStart;

        rewardBtn.anchoredPosition = rewardBtnStart;
        adBtn.anchoredPosition = adBtnStart;

        playBtn.anchoredPosition = playBtnStart;
        shopBtn.anchoredPosition = shopBtnStart;
    }

    private void AddTween(RectTransform element, Vector2 offset, float delay)
    {
        Tweener tweener = element.DOAnchorPos(element.anchoredPosition - offset, 0.5f)
            .SetDelay(delay)
            .From()
            .SetEase(Ease.OutBack)
            .SetAutoKill(false)
            .Pause()
            .OnStart(() => activeForwardTweens++)
            .OnComplete(CheckIfAllForwardTweensComplete);

        tweeners.Add(tweener);
        reverseStack.Push(tweener);
    }

    public void PlayTweens()
    {
        activeReverseTweens = 0;
        foreach (var tweener in tweeners)
        {
            tweener.Restart();
        }
    }

    public void CloseMenu(GameObject panelToActivate)
    {
        //TODO Sounds Calling
        SoundManager.Instance?.PlayOnButtonSound();

        MenuManager.GetInstance()?.eventSystem.SetActive(false);

        activeReverseTweens = reverseStack.Count;
        StartCoroutine(PlayTweensInReverse(panelToActivate));
    }

    private IEnumerator PlayTweensInReverse(GameObject panelToActivate)
    {
        activeReverseTweens = reverseStack.Count;

        // Rewind the fill image from 1 to 0 as the menu closes
        DOTween.To(() => fillImage.fillAmount, x => fillImage.fillAmount = x, 0f, 1f).SetEase(Ease.OutBack);  // Fill animation from 1 to 0

        while (reverseStack.Count > 0)
        {
            Tweener tweener = reverseStack.Pop();

            tweener.OnRewind(() => CheckIfAllReverseTweensComplete(panelToActivate));
            tweener.PlayBackwards();

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void CheckIfAllForwardTweensComplete()
    {
        activeForwardTweens--;
        if (activeForwardTweens <= 0)
        {
            OnAllTweensComplete?.Invoke();

            MenuManager.GetInstance()?.eventSystem.SetActive(true);
        }
    }

    private void CheckIfAllReverseTweensComplete(GameObject panelToActivate)
    {
        activeReverseTweens--;
        if (activeReverseTweens <= 0)
        {
            menu.SetActive(false);

            panelToActivate.SetActive(true);

            tweeners.Clear();
            reverseStack.Clear();

            OnAllReverseTweensComplete?.Invoke();
        }
    }
}
