using UnityEngine;

public class SetAudioVolume : MonoBehaviour
{
    private void Start()
    {
        if(!TryGetComponent<AudioSource>(out AudioSource _as)) return;

        _as.volume = SoundManager.Instance.bgAudioSource.volume;
    }
}
