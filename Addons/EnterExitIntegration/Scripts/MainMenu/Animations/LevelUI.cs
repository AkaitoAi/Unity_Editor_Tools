using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private GameObject level;
    [SerializeField] private Image fillImage;  // Reference to the Image component
    [SerializeField] private HorizontalLayoutGroup horizontalLayout;

    [SerializeField] private Transform level1, level2,
        level3, level4, level5;

    private List<Tweener> tweeners = new List<Tweener>();
    private Stack<Tweener> reverseStack = new Stack<Tweener>();

    private Vector3 level1Start, level2Start, level3Start, level4Start, level5Start;

    private int activeForwardTweens = 0;
    private int activeReverseTweens = 0;

    public delegate void TweenCompleteHandler();
    public static event TweenCompleteHandler OnAllTweensComplete;
    public static event TweenCompleteHandler OnAllReverseTweensComplete;

    private void Awake()
    {
        // Store initial positions
        level1Start = level1.localPosition;
        level2Start = level2.localPosition;
        level3Start = level3.localPosition;
        level4Start = level4.localPosition;
        level5Start = level5.localPosition;
    }

    private void Start()
    {
        OpenPanel();
    }

    private void OnEnable()
    {
        OpenPanel(); // Restart tweens when re-enabling the menu
    }

    public void OpenPanel()
    {
        level.SetActive(true);

        MenuManager.GetInstance()?.eventSystem.SetActive(false);

        horizontalLayout.enabled = false;

        // Set the fill image to 0 when entering the panel and then animate to 1
        fillImage.fillAmount = 0f;
        DOTween.To(() => fillImage.fillAmount, x => fillImage.fillAmount = x, 1f, .5f).SetEase(Ease.OutBack);  // Fill animation from 0 to 1

        tweeners.Clear();
        reverseStack.Clear();
        activeForwardTweens = 0;

        ResetButtonPositions(); // Reset positions before starting tweens

        AddTween(level1, -320, 0.0f);
        AddTween(level2, -320, 0.1f);
        AddTween(level3, -320, 0.2f);
        AddTween(level4, -320, 0.3f);
        AddTween(level5, -320, 0.4f);

        AddTween(level1, 1000, 0.0f, isVertical: true);
        AddTween(level2, 1000, 0.1f, isVertical: true);
        AddTween(level3, 1000, 0.2f, isVertical: true);
        AddTween(level4, 1000, 0.3f, isVertical: true);
        AddTween(level5, 1000, 0.4f, isVertical: true);

        PlayTweens();
    }

    private void ResetButtonPositions()
    {
        level1.localPosition = level1Start;
        level2.localPosition = level2Start;
        level3.localPosition = level3Start;
        level4.localPosition = level4Start;
        level5.localPosition = level5Start;
    }

    private void AddTween(Transform element, float offset, float delay, bool isVertical = false)
    {
        Tweener tweener = isVertical
            ? CreateTweenY(element, offset, delay, 0.5f, Ease.OutBack)
            : CreateTweenX(element, offset, delay, 0.5f, Ease.OutBack);

        tweeners.Add(tweener);
        reverseStack.Push(tweener);
    }

    private Tweener CreateTweenX(Transform element, float offsetX, float delay, float duration, Ease ease)
    {
        return element.DOMoveX(element.position.x - offsetX, duration)
            .SetDelay(delay)
            .From()
            .SetEase(ease)
            .SetAutoKill(false)
            .Pause()
            .OnStart(() => activeForwardTweens++)
            .OnComplete(CheckIfAllForwardTweensComplete);
    }

    private Tweener CreateTweenY(Transform element, float offsetY, float delay, float duration, Ease ease)
    {
        return element.DOMoveY(element.position.y - offsetY, duration)
            .SetDelay(delay)
            .From()
            .SetEase(ease)
            .SetAutoKill(false)
            .Pause()
            .OnStart(() => activeForwardTweens++)
            .OnComplete(CheckIfAllForwardTweensComplete);
    }

    public void PlayTweens()
    {
        activeReverseTweens = 0;
        foreach (var tweener in tweeners)
        {
            tweener.Restart();
        }
    }

    public void ClosePanel(GameObject panelToActivate)
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

        horizontalLayout.enabled = false;

        // Rewind the fill image from 1 to 0 as the menu closes
        DOTween.To(() => fillImage.fillAmount, x => fillImage.fillAmount = x, 0f, .5f).SetEase(Ease.OutBack);  // Fill animation from 1 to 0

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

            horizontalLayout.enabled = false;

            MenuManager.GetInstance()?.eventSystem.SetActive(true);
        }
    }

    private void CheckIfAllReverseTweensComplete(GameObject panelToActivate)
    {
        activeReverseTweens--;
        if (activeReverseTweens <= 0)
        {
            level.SetActive(false);

            panelToActivate.SetActive(true);

            //if (panelToActivate == setting)
            //    settingFader.FadeIN();

            tweeners.Clear();
            reverseStack.Clear();

            OnAllReverseTweensComplete?.Invoke();
        }
    }
}
