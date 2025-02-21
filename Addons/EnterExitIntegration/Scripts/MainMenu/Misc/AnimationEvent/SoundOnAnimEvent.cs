using UnityEngine;

public class SoundOnAnimEvent : MonoBehaviour, IAnimationEventable
{
    public void OnAnimatonEvent(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight < 0.5f) return;

        if (!TryGetComponent<AudioSource>(out AudioSource _as)) return;

        if (!_as.enabled) return;

        _as.Play();
    }
}