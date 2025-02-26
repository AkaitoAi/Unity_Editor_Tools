using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    #region Singleton
    private static SoundManager instance;
    public static SoundManager Instance { get { if (instance == null) instance = GameObject.FindObjectOfType<SoundManager>(); return instance; } }
    #endregion

    [Header ("Audio Sources")]
    public AudioSource bgAudioSource;
    public AudioSource sfxAudioSource;

    [Header("Audio Clips")]
    [HideInInspector] public bool inMenu; // true in menu and false in gameplay
    [SerializeField] private AudioClip[] bgAudioClips;
    [SerializeField] private AudioClip[] hornAudioClips;
    [SerializeField] private AudioClip[] winAudioClips;
    [SerializeField] private AudioClip[] failedAudioClips;
    [SerializeField] private AudioClip[] wooshAudioClips;
    [SerializeField] private AudioClip[] radioTuningAudioClips;
    
    [SerializeField] private AudioClip menuAudioClip;
    [SerializeField] private AudioClip gamePlayAudioClip;
    [SerializeField] private AudioClip onButtonAudioClip;
    [SerializeField] private AudioClip parkingAudioClip;
    [SerializeField] private AudioClip levelWinAudioClip;
    [SerializeField] private AudioClip coinPickupAudioClip;
    [SerializeField] private AudioClip scrollRectAudioClip;
    [SerializeField] private AudioClip purchaseAudioClip;
    [SerializeField] private AudioClip purchaseFaliedClip;
    [SerializeField] private AudioClip engineStartClip;
    [SerializeField] private AudioClip thunderClapAudioClip;
    [SerializeField] private AudioClip checkpointAudioClip;
    [SerializeField] private AudioClip starSoundAudioClip;
    [SerializeField] private AudioClip engineAudioClip;
    [SerializeField] private AudioClip flashAudioClip;
    [SerializeField] private AudioClip gearSwitchAudioClip;
    [SerializeField] private AudioClip typingAudioClip;
    [SerializeField] private AudioClip winAudioClip;



    private float masterVolumePref;
    private float bgVolume;
    private float bgVolumePref;
    private float sfxVolume;
    private float sfxVolumePref;
    private int musicMute;
    private int musicMutePref;
    private int sfxMute;
    private int sfxMutePref;
    private int muteAudioPref;
    private int lastBGAudio = -1;
    private int lastSFXAudio = -1;

    private SetupSO setupScriptable;

    internal int loopIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);

        setupScriptable = Resources.Load("ScriptableObjects/Setup/Setup") as SetupSO;

        masterVolumePref = PlayerPrefs.GetFloat(setupScriptable.masterVolumePref);
        bgVolumePref = PlayerPrefs.GetFloat(setupScriptable.bGVolumePref);
        sfxVolumePref = PlayerPrefs.GetFloat(setupScriptable.sFXVolumePref);
        musicMutePref = PlayerPrefs.GetInt(setupScriptable.musicMutePref);
        sfxMutePref = PlayerPrefs.GetInt(setupScriptable.sFXMutePref);
        muteAudioPref = PlayerPrefs.GetInt(setupScriptable.muteAudioPref);

        bgVolume = bgAudioSource.volume;
        sfxVolume = sfxAudioSource.volume;

        if (bgAudioSource)
            bgVolume = bgVolumePref;
        if (sfxAudioSource)
            sfxVolume = sfxVolumePref;
    }

    public void PlayBGSound() => PlayRandomClip(bgAudioClips);
    public void PlayBGSoundFromArray(int _clipIndex) => PlayClipFromArray(bgAudioClips, _clipIndex);

    public void PlayMenuBG() => PlayBGClip(menuAudioClip);
    public void PlayGameplayBG() => PlayBGClip(gamePlayAudioClip);
    public void PlayOnButtonSound() => PlayOneShotClip(onButtonAudioClip); 
    public void PlayLevelWinSound() => PlayNormalClip(levelWinAudioClip);
    public void PlayWinSound() => PlayNormalClip(winAudioClip);
    public void PlayParkingSound() => PlayNormalClip(parkingAudioClip);
    public void PlayCoinPickupSound() => PlayOneShotClip(coinPickupAudioClip); 
    public void PlayPurchaseSound() => PlayNormalClip(purchaseAudioClip);
    public void PlayPurchaseFailedSound() => PlayNormalClip(purchaseFaliedClip);
    public void PlayScrollRectSound() => PlayOneShotClip(scrollRectAudioClip);
    public void PlayLevelWinRewardSound() => PlayNormalClip(scrollRectAudioClip);
    public void PlayEngineStartSound() => PlayNormalClip(engineStartClip);
    public void PlayWarningSound() => PlayOneShotClip(purchaseFaliedClip);
    public void PlayHornSound() => PlayNormalRandomClip(hornAudioClips);
    public void PlayThunderClapSound() => PlayNormalClip(thunderClapAudioClip);
    public void PlayCheckpointSound() => PlayOneShotClip(checkpointAudioClip);
    public void PlayStarSound() => PlayOneShotClip(starSoundAudioClip);
    public void PlayEngineSound() => PlayOneShotClip(engineAudioClip);
    public void PlayFlashSound() => PlayNormalClip(flashAudioClip);
    public void PlayGearSwitchSound() => PlayNormalClip(gearSwitchAudioClip);
    public void PlayNormalRandomWinSound() => PlayNormalRandomClip(winAudioClips);
    public void PlayNormalRandomFailedSound() => PlayNormalRandomClip(failedAudioClips);
    public void PlayNormalRandomWooshSound() => PlayNormalRandomClip(wooshAudioClips);
    public void PlayTypeSound() => PlayOneShotClip(typingAudioClip);
    public void PlayNextBGSong() => RadioLoop(bgAudioClips, radioTuningAudioClips, bgAudioSource);

    public void ChangeVolume(float _musicVolume, float _sFXVolume)
    {
        bgAudioSource.volume = _musicVolume;
        sfxAudioSource.volume = _sFXVolume;
    }

    private void PlayRandomClip(AudioClip[] _audioClips)
    {
        if (_audioClips.Length == 0) return;

        int randomClip = Random.Range(0, _audioClips.Length);

        while(randomClip == lastBGAudio)
            randomClip = Random.Range(0, _audioClips.Length);

        lastBGAudio = randomClip;

        bgAudioSource.clip = _audioClips[randomClip];
        bgAudioSource.Play();
    }

    private void PlayBGClip(AudioClip _audioClip)
    {
        if (_audioClip == null) return;

        bgAudioSource.clip = _audioClip;
        bgAudioSource.Play();
    }

    private void PlayClipFromArray(AudioClip[] _audioClips, int _clipIndex)
    {
        if (_audioClips.Length == 0) return;

        bgAudioSource.clip = _audioClips[_clipIndex];
        bgAudioSource.Play();
    }

    private void PlayNormalRandomClip(AudioClip[] _audioClips)
    {
        if (_audioClips.Length == 0) return;

        int randomClip = Random.Range(0, _audioClips.Length);

        while (randomClip == lastSFXAudio)
            randomClip = Random.Range(0, _audioClips.Length);

        lastSFXAudio = randomClip;

        sfxAudioSource.clip = _audioClips[randomClip];
        sfxAudioSource.Play();
    }

    public void PlayOneShotClip(AudioClip _audioClip)
    {
        if (_audioClip == null) return;

        sfxAudioSource.clip = _audioClip;
        sfxAudioSource.PlayOneShot(_audioClip);
    }

    private void PlayNormalClip(AudioClip _audioClip)
    {
        if (_audioClip == null) return;

        sfxAudioSource.clip = _audioClip;
        sfxAudioSource.Play();
    }

    public void PlayNormalClipFromArray(AudioClip[] _audioClips, int _clipIndex)
    {
        if (_audioClips.Length == 0) return;

        sfxAudioSource.clip = _audioClips[_clipIndex];
        sfxAudioSource.Play();
    }
    
    public void PlayOneShotClipFromArray(AudioClip[] _audioClips, int _clipIndex)
    {
        if (_audioClips.Length == 0) return;

        sfxAudioSource.clip = _audioClips[_clipIndex];
        sfxAudioSource.PlayOneShot(_audioClips[_clipIndex]);
    }

    public AudioClip[] GetAudioClips(AudioClip[] _audioClips)
    {
        return _audioClips;
    }

    private void RadioLoop(AudioClip[] _audioClips, AudioClip[] _tuningAudioClip, AudioSource _audioSource)
    {
        if (_audioClips.Length == 0) return;
        if (_tuningAudioClip.Length == 0) return;

        Debug.Log("Music Index Pre: " + loopIndex);

        loopIndex++;
        if (loopIndex >= _audioClips.Length) loopIndex = 0;
        Debug.Log("Music Index Post: " + loopIndex);
        
        int rand = Random.Range(0, _tuningAudioClip.Length);

        StartCoroutine(PlayNextSong());

        IEnumerator PlayNextSong()
        {
            _audioSource.clip = _tuningAudioClip[rand];
            _audioSource.Play();

            //! Wait for AudioClip Length
            //yield return new WaitForSeconds(_tuningAudioClip[rand].length);
            yield return new WaitForSeconds(2.5f);

            Debug.Log("Music Index Selected: " + loopIndex);

            _audioSource.clip = _audioClips[loopIndex];

            _audioSource.Play();
        }
    }

    //private void PlayScrollRectSound(ScrollRect _scroll, float _scrollSoundDely, float _oldPos)
    //{
    //    if (sfxVolumePref > 0.0f)
    //    {
    //        if (_scroll.horizontalNormalizedPosition > (_oldPos + _scrollSoundDely))
    //        {
    //            _oldPos = _scroll.horizontalNormalizedPosition;
    //            PlayNormalClip(scrollRectAudioClip);
    //        }
    //        else if (_scroll.horizontalNormalizedPosition < (_oldPos - _scrollSoundDely))
    //        {
    //            _oldPos = _scroll.horizontalNormalizedPosition;
    //            PlayNormalClip(scrollRectAudioClip);
    //        }
    //    }
    //}
}
