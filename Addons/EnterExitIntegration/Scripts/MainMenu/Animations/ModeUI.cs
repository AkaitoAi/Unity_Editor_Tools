using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class ModeUI : MonoBehaviour
{
    [SerializeField] private GameObject mode;

    [SerializeField] private RectTransform openWorldMode, drivingChallenge, gangsterChallenge;

    private List<Tweener> tweeners = new List<Tweener>();
    private Stack<Tweener> reverseStack = new Stack<Tweener>();

    private Vector2 openWorldModeStart, drivingChallengeStart, gangsterChallengeStart;

    private int activeForwardTweens = 0;
    private int activeReverseTweens = 0;

    public delegate void TweenCompleteHandler();
    public static event TweenCompleteHandler OnAllTweensComplete;
    public static event TweenCompleteHandler OnAllReverseTweensComplete;

    private void Awake()
    {
        // Store initial anchored positions
        openWorldModeStart = openWorldMode.anchoredPosition;
        drivingChallengeStart = drivingChallenge.anchoredPosition;
        gangsterChallengeStart = gangsterChallenge.anchoredPosition;
    }

    private void Start()
    {
        OpenMode();
    }

    private void OnEnable()
    {
        OpenMode(); // Restart tweens when re-enabling the menu
    }

    public void OpenMode()
    {
        mode.SetActive(true);

        MenuManager.GetInstance()?.eventSystem.SetActive(false);

        tweeners.Clear();
        reverseStack.Clear();
        activeForwardTweens = 0;

        ResetButtonPositions(); // Reset positions before starting tweens

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        AddTween(openWorldMode, new Vector2(screenWidth * 0.3f, 0), 0f);
        AddTween(drivingChallenge, new Vector2(0, screenHeight * 1f), 0.3f);
        AddTween(gangsterChallenge, new Vector2(0, screenHeight * 1f), 0.4f);

        PlayTweens();
    }

    private void ResetButtonPositions()
    {
        openWorldMode.anchoredPosition = openWorldModeStart;
        drivingChallenge.anchoredPosition = drivingChallengeStart;
        gangsterChallenge.anchoredPosition = gangsterChallengeStart;
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

    public void CloseMode(GameObject panelToActivate)
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
            mode.SetActive(false);

            panelToActivate.SetActive(true);

            tweeners.Clear();
            reverseStack.Clear();

            OnAllReverseTweensComplete?.Invoke();
        }
    }
}
