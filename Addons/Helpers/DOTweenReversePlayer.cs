using UnityEngine;
using DG.Tweening;

public class DOTweenReversePlayer : MonoBehaviour
{
    [Tooltip("List of objects that have DOTweenAnimation components")]
    [SerializeField] private GameObject[] targetObjects;

    [SerializeField] private bool playOnStart = false;

    private void Start()
    {
        if (playOnStart)
        {
            PlayReverse();
        }
    }

    public void PlayReverse()
    {
        foreach (var obj in targetObjects)
        {
            if (obj == null) continue;

            var animations = obj.GetComponents<DOTweenAnimation>();

            foreach (var anim in animations)
            {
                if (anim != null)
                {
                    // Make sure tween is created
                    if (anim.tween == null || !anim.tween.active)
                    {
                        anim.CreateTween();
                        anim.tween.Pause();
                    }

                    anim.tween.PlayBackwards();
                }
            }
        }
    }
}
