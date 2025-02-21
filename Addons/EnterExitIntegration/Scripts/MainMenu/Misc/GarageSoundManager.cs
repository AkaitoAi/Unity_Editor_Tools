using UnityEngine;

public class GarageSoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource[] audioSrcs;

    public void GarageAudioActive(bool _status)
    {
        foreach (AudioSource src in audioSrcs)
        {
            src.enabled = _status;

            //TODO Sounds Calling
            src.volume = SoundManager.Instance.bgAudioSource.volume;
        }

        //TODO Sounds Calling
        SoundManager.Instance.bgAudioSource.enabled = !_status;
    }
}
