using System;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] private FootStepSound footStepSound;

    [Serializable]
    public struct FootStepSound
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
}
