using Driveable.Core;
using System;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public DriverIK bodyIK;
    public DriverIK drivingIK;

    [SerializeField] private FootStepSound footStepSound;

    public static event Action<string> OnAnimationEventTriggered;
    [Serializable] public struct FootStepSound
    {
        public AudioClip[] audioClips;
        public AudioSource audioSource;

        public void PlayAudioClip()
        {
            if (audioSource == null) return;

            if (audioClips == null || 
                audioClips.Length <= 0) return;

            AudioClip audioClip = audioClips[UnityEngine.Random.Range(0, audioClips.Length)];
            audioSource.pitch += UnityEngine.Random.Range(-.05f, .05f);

            audioSource.PlayOneShot(audioClip);
        }
    }
    public void OnFootStepSound()
    { 
        footStepSound.PlayAudioClip();
    }

    public void OnAnimationEvent(string eventName)
    {
        OnAnimationEventTriggered?.Invoke(eventName);
    }

    public void OnLeftHandFoot()
    {
        EventBus<OnProvideIK>.Raise(new OnProvideIK
        {
            instanceID = transform.GetInstanceID(),
            lookAt = null,
            leftHand = null,
            rightHand = drivingIK.rightHandIK,
            leftFoot = null,
            rightFoot = drivingIK.rightFootIK
        });
    }
    
    public void OnLerpToDrop()
    {
        EventBus<OnProvideIK>.Raise(new OnProvideIK
        {
            instanceID = transform.GetInstanceID(),
            lookAt = null,
            leftHand = null,
            rightHand = null,
            leftFoot = null,
            rightFoot = null
        });
    }
}
