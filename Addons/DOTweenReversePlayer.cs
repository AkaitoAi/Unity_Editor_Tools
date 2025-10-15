using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;

public class DOTweenReversePlayer : MonoBehaviour
{
    [Tooltip("List of objects that have DOTweenAnimation components")]
    [SerializeField] private GameObject[] targetObjects;

    [SerializeField] private bool playOnStart = false;

    private DOTweenAnimation[] animations;

    [SerializeField] private UnityEvent OnPlayEvent, OnCompleteEvent;

    private void Awake()
    {
        animations = GetComponentsInChildren<DOTweenAnimation>(true);

        foreach (var anim in animations)
        {
            if (anim.tween == null || !anim.tween.active)
                anim.CreateTween();
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayReverse();
        }
    }

    public void PlayReverse()
    {
        OnPlayEvent?.Invoke();

        foreach (var obj in targetObjects)
        {
            if (obj == null) continue;

            var animations = obj.GetComponents<DOTweenAnimation>();

            foreach (var anim in animations)
            {
                if (anim != null)
                {
                    if (anim.tween == null || !anim.tween.active)
                    {
                        anim.CreateTween();
                        anim.tween.Pause();
                    }

                    anim.tween.PlayBackwards();
                }
            }

            DOVirtual.DelayedCall(GetMaxDuration(), () => OnCompleteEvent?.Invoke());
        }
    }

    public void PlayReverseRewind()
    {
        OnPlayEvent?.Invoke();

        foreach (var obj in targetObjects)
        {
            if (obj == null) continue;

            var animations = obj.GetComponents<DOTweenAnimation>();

            foreach (var anim in animations)
            {
                if (anim != null)
                {
                    if (anim.tween == null || !anim.tween.active)
                    {
                        anim.CreateTween();
                        anim.tween.Pause();
                    }

                    anim.tween.OnRewind(() => { OnCompleteEvent?.Invoke(); });

                    anim.tween.PlayBackwards();
                }
            }
        }
    }


    public void PlayForward(Action onComplete = null)
    {
        foreach (var anim in animations)
        {
            if (anim == null) continue;

            anim.DORestart(); 
        }

        if (onComplete != null)
            DOVirtual.DelayedCall(GetMaxDuration(), () => onComplete());
    }

    public void PlayReverse(Action onComplete = null)
    {
        foreach (var anim in animations)
        {
            if (anim == null) continue;

            anim.DORestart();      
            anim.DOPlayBackwards();
        }

        if (onComplete != null)
            DOVirtual.DelayedCall(GetMaxDuration(), () => onComplete());
    }

    private float GetMaxDuration()
    {
        float max = 0f;
        foreach (var anim in animations)
        {
            if (anim == null || anim.tween == null) continue;
            max = Mathf.Max(max, anim.tween.Duration(includeLoops: true));
        }
        return max;
    }
}

//public void PlayReverse()
//{
//    foreach (var obj in targetObjects)
//    {
//        if (obj == null) continue;

//        var animations = obj.GetComponents<DOTweenAnimation>();

//        foreach (var anim in animations)
//        {
//            if (anim != null)
//            {
//                if (anim.tween == null || !anim.tween.active)
//                {
//                    anim.CreateTween();
//                    anim.tween.Pause();
//                }

//                anim.tween.PlayBackwards();
//            }
//        }
//    }
//}
