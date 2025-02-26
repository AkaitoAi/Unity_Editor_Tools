using UnityEngine;

public class ScaredOfHorse : MonoBehaviour, IAnimationEventable
{
    [SerializeField] private Animator scaredAnimator;

    public void OnAnimatonEvent(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight < 0.5f) return;

        scaredAnimator.Play("Scared");

        if (!TryGetComponent<AudioSource>(out AudioSource _as)) return;

        _as.Play();
    }
}
