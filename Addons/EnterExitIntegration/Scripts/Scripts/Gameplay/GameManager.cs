using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using AkaitoAi.Singleton;

public enum GameplayScreens
{
    None,
    Resume,
    Pause,
    Win,
    Fail,
    CutScene,
    AdsLoading,
    Loading
};

public class GameManager : Singleton<GameManager>
{
    [Header("Gameplay States")]
    public GameplayScreens state = new GameplayScreens();
    [SerializeField] private GameObject inGameScreen, pauseScreen, failScreen, winScreen, adsLoadingScreen, loadingScreen, cutSceneScreen;

    [Header("Canvases")]
    public GameObject dialogueCanvas;

    [Header("Panel Animators")]
    public Animator cutSceneBarsAnimator;
    public Animator canvasesAnimator;

    [Header("Timer Controller")]
    internal TimerController timeController;
    private bool useTimer = false;
    private GameObject timerContainer;
    internal bool timeRunning = false;

    [Header("Game Setup")]
    [SerializeField] private ModeLevelSetup[] modeLevelSetup;
    private GameObject environmentObj;
    private GameObject levelsParentObj;
    private GameObject vehiclesParentObj;
    private GameObject selectedEnvironment;
    private GameObject selectedLevel;
    internal SetupSO setupScriptable;

    [Header("Level Setup")]
    private int levelWinReward = 1000;
    private int reward;
    private int totalReward;
    internal int levelNumber;
    internal int selectedMode;
    internal int vehicleNumber;
    internal int controlIndex;
    internal int muteSound;
    internal int musicMuteSound;
    internal int sfxMuteSound;
    internal int levelCoins;
    internal float sfxVolume;
    internal float musicVolume;
    internal LevelInfo levelInfo;
    internal LevelSelectSO currentLevelSelectSO;
    private bool showAds = false;
    private bool addCoins = false;
    private bool winPanelStateEnabled = false;
    private bool showWinPanel = false;

    [Header("RCC Setup")]
    [SerializeField] private Camera rccMainCamera;
    internal GameObject selectedVehicle;
    //[HideInInspector] public RCC_CarControllerV3 selectedVehicleRCC;
    internal Transform selectedVehicleLastTransform;
    internal float selectedVehicleIdleVolume, selectedVehicleMinVolume, selectedVehicleMaxVolume;
    internal AudioListener rccAudioListener;
    internal bool hasEntered;
    //private RCC_SceneManager sceneManager;

    [Header("Engine Start/Stop")]
    [SerializeField] private GameObject controls;
    [SerializeField] private GameObject enginestartButton, engineStopButton;

    [Header("Black Fade Screen")]
    [SerializeField] private CanvasGroup blackScreenEffectCG;
    private bool isFaded = false;
    
    [Header("Vintage Splash")]
    public Image vintageImage;
    public Sprite blueSplash, greenSplash, redSplash;
    
    [Header("Objective")]
    [SerializeField] private GameObject objectiveContainer;
    [SerializeField] private Text objectiveText;

    [Header("Lerp Setup")]
    [SerializeField] private float counterDuration = .5f;
    [SerializeField] private Text totalWinRewardText;
    [SerializeField] private Text winTimeText;
    [SerializeField] private Text coinText;

    public GameObject Vehicle => selectedVehicle;

    internal bool inResume = false;
    internal bool inCutScene = false;

    [System.Serializable]
    public struct ModeLevelSetup
    {
        public GameObject environmentObj;
        public GameObject levelsParentObj;
        public GameObject vehiclesParentObj;
        public TimerController timeController;
        public int levelWinReward;
    }

    private void Awake()
    {
        Time.timeScale = 1f;

        //! Loads Setup which are configured in menu scene and assigning relevant variables
        setupScriptable = Resources.Load("ScriptableObjects/Setup/Setup") as SetupSO;
        levelNumber = setupScriptable.sLevelIndex;
        selectedMode = setupScriptable.sModeIndex;
        vehicleNumber = setupScriptable.sVehicleIndex;
        controlIndex = PlayerPrefs.GetInt(setupScriptable.controlPref);
        sfxVolume = PlayerPrefs.GetFloat(setupScriptable.sFXVolumePref);
        musicVolume = PlayerPrefs.GetFloat(setupScriptable.bGVolumePref);
        muteSound = PlayerPrefs.GetInt(setupScriptable.muteAudioPref);
        musicMuteSound = PlayerPrefs.GetInt(setupScriptable.musicMutePref);
        sfxMuteSound = PlayerPrefs.GetInt(setupScriptable.sFXMutePref);

        //! Assigning ModeLevelSetup struct values according to mode index
        environmentObj = modeLevelSetup[selectedMode].environmentObj;
        levelsParentObj = modeLevelSetup[selectedMode].levelsParentObj;
        vehiclesParentObj = modeLevelSetup[selectedMode].vehiclesParentObj;
        timeController = modeLevelSetup[selectedMode].timeController;
        levelWinReward = modeLevelSetup[selectedMode].levelWinReward;

        //! Loads LevelDetails 
        currentLevelSelectSO = 
            Resources.Load("ScriptableObjects/LevelSelect/LevelSelect_" + selectedMode) as LevelSelectSO;

        if (controls) controls.SetActive(false);
        //sceneManager = RCC_SceneManager.Instance;

        SpawnLevel();
        SpawnRCCV3();

        state = GameplayScreens.Resume;
        UpdateGameplayState();

        //TODO Sounds Calling
        SoundManager.Instance?.PlayGameplayBG();

        //TODO Ads Calling
        //AdsManager.instance.hideAllAdmobBanners();
        //AdsManager.instance.showAdMobBannerTopLeft();
    }

#if UNITY_EDITOR
    private void Update()
    {
        UpdateGameplayState();
    }
#endif

    #region Gameplay States
    public void LevelPaused() // ! Level paused functionality
    {
        //TODO Sound Calling
        SoundManager.Instance?.PlayOnButtonSound();
        //SoundManager.Instance.ChangeVolume(0f, sfxVolume);
        RCCVolume(0f,0f,0f);

        if (Application.internetReachability != NetworkReachability.NotReachable)
            StartCoroutine(PauseScreen());
        else
        {
            state = GameplayScreens.Pause;
            UpdateGameplayState();
        }

        IEnumerator PauseScreen()
        {
            state = GameplayScreens.AdsLoading;
            UpdateGameplayState();

            //TODO Ads Calling
            //AdsManager.instance.hideAllAdmobBanners();

            yield return new WaitForSecondsRealtime(1f);

            //TODO Ads Calling
            //AdsManager.instance.showAdmobInterstitial();

            yield return new WaitForSecondsRealtime(.5f);

            state = GameplayScreens.Pause;
            UpdateGameplayState();

            //TODO Firebase Calling
            //FirebaseManager.ModeLevelPause(selectedMode.ToString(), levelNumber.ToString());

            //TODO Ads Calling
            //AdsManager.instance.showAdmobAdpativeBannerTop();
        }
    }

    public void LevelResume() // ! Level resumed functionality
    {
        //TODO Sound Calling
        SoundManager.Instance?.PlayOnButtonSound();
        //SoundManager.Instance.ChangeVolume(musicVolume, sfxVolume);
        RCCVolume(selectedVehicleIdleVolume, selectedVehicleMinVolume, selectedVehicleMaxVolume);

        state = GameplayScreens.Resume;
        UpdateGameplayState();
    }
    
    public void LevelFailed() // ! Level failed functionality
    {
        //TODO Sound Calling
        //SoundManager.Instance.ChangeVolume(0f, sfxVolume);
        //SoundManager.Instance.PlayNormalRandomFailedSound();
        RCCVolume(0f, 0f, 0f);

        StartCoroutine(FailedState());

        IEnumerator FailedState()
        {
            if (!showAds)
            {
                showAds = true;

                yield return new WaitForSeconds(2f);

                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    state = GameplayScreens.AdsLoading;
                    UpdateGameplayState();

                    //TODO Ads Calling
                    //AdsManager.instance.hideAllAdmobBanners();

                    yield return new WaitForSecondsRealtime(1f);

                    //TODO Ads Calling
                    //AdsManager.instance.showAdmobInterstitial();

                    yield return new WaitForSecondsRealtime(.5f);

                    state = GameplayScreens.Fail;
                    UpdateGameplayState();

                    selectedVehicle.SetActive(false);
                    selectedLevel.SetActive(false);
                    selectedEnvironment.SetActive(false);

                    //TODO Firebase Calling
                    //FirebaseManager.ModeLevelFail(selectedMode.ToString(), levelNumber.ToString());

                    //TODO Ads Calling
                    //AdsManager.instance.showAdmobAdpativeBannerTop();
                }
                else
                {
                    state = GameplayScreens.Fail;
                    UpdateGameplayState();
                    selectedVehicle.SetActive(false);
                    selectedLevel.SetActive(false);
                    selectedEnvironment.SetActive(false);
                }
            }
        }
    }

    public void LevelWin() // ! Level Win/ Pass/ Passed/ Success/ Successfull functionality before delay
    {
        //TODO Sound Calling
        SoundManager.Instance?.ChangeVolume(0f, sfxVolume);
        RCCVolume(0f, 0f, 0f);

        if (!addCoins)
        {
            //TODO Sound Calling
            //SoundManager.Instance.PlayNormalRandomWinSound();
            SoundManager.Instance?.PlayLevelWinSound();

            PlayerPrefs.SetInt(setupScriptable.levelCoinsPref, levelCoins);
            reward = PlayerPrefs.GetInt(setupScriptable.levelCoinsPref) + levelWinReward;
            PlayerPrefs.SetInt(setupScriptable.totalCoinsPref, 
                PlayerPrefs.GetInt(setupScriptable.totalCoinsPref) + 
                PlayerPrefs.GetInt(setupScriptable.levelCoinsPref) + levelWinReward);
            totalReward = PlayerPrefs.GetInt(setupScriptable.totalCoinsPref);

            //rccCamera.ChangeCamera(RCC_Camera.CameraMode.TPS);
            //rccCamera.lookBackAtWon = true;

            rccMainCamera.transform.GetChild(0).gameObject.SetActive(true);

            addCoins = true;
        }

        int _level = levelNumber;

        UnlockLevel(_level, currentLevelSelectSO.levelForm, currentLevelSelectSO.totalLevels, currentLevelSelectSO.prefName);

        StartCoroutine(LevelWinStateDelay(levelInfo.winDelay));

        IEnumerator LevelWinStateDelay(float _delay) // !Level Win/ Pass/ Passed/ Success/ Successfull functionality after delay
        {
            state = GameplayScreens.None;
            UpdateGameplayState();

            if (!winPanelStateEnabled) winPanelStateEnabled = true;

            yield return new WaitForSeconds(_delay);

            if (winPanelStateEnabled && !showWinPanel)
            {
                TimeToggle(false, 1f);
                if (Application.internetReachability != NetworkReachability.NotReachable)
                    StartCoroutine(WinState());
                else
                {
                    state = GameplayScreens.Win;
                    UpdateGameplayState();
                    
                    //StartCoroutine(RewardCounter());
                    
                    selectedVehicle.SetActive(false);
                    selectedLevel.SetActive(false);
                    selectedEnvironment.SetActive(false);
                }

                //TODO Sound Calling
                SoundManager.Instance?.ChangeVolume(musicVolume, sfxVolume);
            }

            IEnumerator WinState()
            {
                if (!showAds)
                {
                    showAds = true;
                    showWinPanel = true;

                    state = GameplayScreens.AdsLoading;
                    UpdateGameplayState();

                    //TODO Ads Calling
                    //AdsManager.instance.hideAllAdmobBanners();

                    yield return new WaitForSecondsRealtime(1f);

                    //TODO Ads Calling
                    //AdsManager.instance.showAdmobInterstitial();

                    yield return new WaitForSecondsRealtime(.5f);

                    state = GameplayScreens.Win;
                    UpdateGameplayState();

                    //StartCoroutine(RewardCounter());

                    selectedVehicle.SetActive(false);
                    selectedLevel.SetActive(false);
                    selectedEnvironment.SetActive(false);

                    //TODO Firebase Calling
                    //FirebaseManager.ModeLevelComplete(selectedMode.ToString(), levelNumber.ToString());

                    //TODO Ads Calling
                    //AdsManager.instance.showAdmobAdpativeBannerTop();
                }
            }
            
            IEnumerator RewardCounter()
            {
                float timeElapsed = 0;

                while (timeElapsed < counterDuration)
                {
                    float amountCount = Mathf.Lerp(0f, reward, timeElapsed / counterDuration);
                    coinText.text = Mathf.FloorToInt(amountCount).ToString();

                    if (timerContainer)
                    {
                        float timeCount = Mathf.Lerp(0, timeController.time, timeElapsed / counterDuration);
                        winTimeText.text = Mathf.FloorToInt(timeCount).ToString();
                    }

                    timeElapsed += Time.unscaledDeltaTime;

                    yield return null;
                }

                coinText.text = reward.ToString();

                if (timerContainer) winTimeText.text = timeController.timerText.text;

                StartCoroutine(TotalRewardCounter());
            }

            IEnumerator TotalRewardCounter()
            {
                float timeElapsed = 0;

                while (timeElapsed < counterDuration)
                {
                    float amountCount = Mathf.Lerp(reward, totalReward, timeElapsed / counterDuration);
                    totalWinRewardText.text = Mathf.FloorToInt(amountCount).ToString();
                    timeElapsed += Time.unscaledDeltaTime;

                    yield return null;
                }

                totalWinRewardText.text = totalReward.ToString();
            }
        }
        void UnlockLevel(int _level, int _levelForm, int _levelsLength, string _levelFormPref) //! Unlock's next level
        {
            _levelForm = PlayerPrefs.GetInt(_levelFormPref);

            if (_levelForm <= _level)
            {
                if (_level < _levelsLength)
                {
                    _levelForm++;
                    PlayerPrefs.SetInt(_levelFormPref, _levelForm);

                    // Rateus during after Level Win
                    //if (levelNumber % 2 == 0)
                    //{
                    //    if (PlayerPrefs.GetInt("RateUs") == 0)
                    //        RateUs();
                    //}
                }
            }
        }
    }

    public void Loading()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
            StartCoroutine(LoadingScreen());
        else
        {
            state = GameplayScreens.Loading;
            UpdateGameplayState();
        }

        IEnumerator LoadingScreen()
        {
            state = GameplayScreens.AdsLoading;
            UpdateGameplayState();

            //TODO Ads Calling
            //AdsManager.instance.hideAllAdmobBanners();

            yield return new WaitForSecondsRealtime(1f);

            //TODO Ads Calling
            //AdsManager.instance.showAdmobInterstitial();

            yield return new WaitForSecondsRealtime(.5f);

            state = GameplayScreens.Loading;
            UpdateGameplayState();

            //TODO Ads Calling
            //AdsManager.instance.showAdmobAdpativeBannerTop();
        }
    }

    public void UpdateGameplayState()
    {
        switch (state)
        {
            case GameplayScreens.None:
                {
                    TimeToggle(true, 1f);

                    if (dialogueCanvas.activeInHierarchy) dialogueCanvas.SetActive(false);

                    failScreen.SetActive(false);
                    winScreen.SetActive(false);
                    adsLoadingScreen.SetActive(false);
                    loadingScreen.SetActive(false);
                    pauseScreen.SetActive(false);
                    inGameScreen.SetActive(false);
                    cutSceneScreen.SetActive(false);

                    break;
                }

            case GameplayScreens.Resume:
                {
                    TimeToggle(true, 1f);

                    if (!dialogueCanvas.activeInHierarchy) dialogueCanvas.SetActive(true);

                    failScreen.SetActive(false);
                    winScreen.SetActive(false);
                    adsLoadingScreen.SetActive(false);
                    loadingScreen.SetActive(false);
                    pauseScreen.SetActive(false);
                    cutSceneScreen.SetActive(false);

                    inGameScreen.SetActive(true);

                    if (inResume) break;

                    inResume = true;
                    inCutScene = false;

                    //TODO Ads Calling
                    //AdsManager.instance.hideAdmobTopBanner();
                    //AdsManager.instance.hideAdmobTopLeftBanner();
                    //AdsManager.instance.showAdMobBannerTopLeft();

                    break;
                }

            case GameplayScreens.Pause:
                {
                    TimeToggle(false, 0f);

                    if (!dialogueCanvas.activeInHierarchy) dialogueCanvas.SetActive(true);

                    failScreen.SetActive(false);
                    winScreen.SetActive(false);
                    inGameScreen.SetActive(false);
                    adsLoadingScreen.SetActive(false);
                    loadingScreen.SetActive(false);
                    cutSceneScreen.SetActive (false);

                    pauseScreen.SetActive(true);

                    break;
                }

            case GameplayScreens.Win:
                {
                    TimeToggle(false, 0f);

                    if (!dialogueCanvas.activeInHierarchy) dialogueCanvas.SetActive(true);

                    failScreen.SetActive(false);
                    inGameScreen.SetActive(false);
                    adsLoadingScreen.SetActive(false);
                    loadingScreen.SetActive(false);
                    pauseScreen.SetActive(false);
                    cutSceneScreen.SetActive(false);

                    winScreen.SetActive(true);

                    break;
                }

            case GameplayScreens.Fail:
                {
                    TimeToggle(false, 0f);

                    if (!dialogueCanvas.activeInHierarchy) dialogueCanvas.SetActive(true);

                    inGameScreen.SetActive(false);
                    adsLoadingScreen.SetActive(false);
                    loadingScreen.SetActive(false);
                    pauseScreen.SetActive(false);
                    winScreen.SetActive(false);
                    cutSceneScreen.SetActive(false);

                    failScreen.SetActive(true);

                    break;
                }

            case GameplayScreens.AdsLoading:
                {
                    TimeToggle(false, 0f);

                    if (!dialogueCanvas.activeInHierarchy) dialogueCanvas.SetActive(true);

                    inGameScreen.SetActive(false);
                    loadingScreen.SetActive(false);
                    pauseScreen.SetActive(false);
                    winScreen.SetActive(false);
                    failScreen.SetActive(false);
                    cutSceneScreen.SetActive(false);

                    adsLoadingScreen.SetActive(true);

                    break;
                }

            case GameplayScreens.Loading:
                {
                    TimeToggle(false, 1f);

                    if (!dialogueCanvas.activeInHierarchy) dialogueCanvas.SetActive(true);

                    inGameScreen.SetActive(false);
                    pauseScreen.SetActive(false);
                    winScreen.SetActive(false);
                    failScreen.SetActive(false);
                    adsLoadingScreen.SetActive(false);
                    cutSceneScreen.SetActive(false);

                    loadingScreen.SetActive(true);

                    break;
                }
            
            case GameplayScreens.CutScene:
                {
                    TimeToggle(false, 1f);

                    if (!dialogueCanvas.activeInHierarchy) dialogueCanvas.SetActive(true);

                    inGameScreen.SetActive(false);
                    pauseScreen.SetActive(false);
                    winScreen.SetActive(false);
                    failScreen.SetActive(false);
                    adsLoadingScreen.SetActive(false);
                    loadingScreen.SetActive(false);
                    
                    cutSceneScreen.SetActive(true);

                    if (inCutScene) break;

                    inCutScene = true;
                    inResume = false;

                    //TODO Ads Calling
                    //AdsManager.instance.hideAdmobTopBanner();
                    //AdsManager.instance.hideAdmobTopLeftBanner();
                    //AdsManager.instance.showAdmobAdpativeBannerTop();

                    break;
                }
            
            default: break;
        }
    }

    public void TimeToggle(bool _timeState, float _timeScale) //! Control's game time scale and level time
    {
        if (useTimer)
            timeRunning = _timeState;

        Time.timeScale = _timeScale;
    }
    #endregion

    #region Screen Effect

    public void FadeScreen(float _fadeTime)
    {
        isFaded = !isFaded;

        if (isFaded)
        {
            blackScreenEffectCG.DOFade(1, _fadeTime);
            blackScreenEffectCG.blocksRaycasts = true;
            blackScreenEffectCG.interactable = true;
            if (blackScreenEffectCG.TryGetComponent<Image>(out Image img))
                img.raycastTarget = true;

            TimeToggle(false, 1f);
        }
        else
        {
            blackScreenEffectCG.DOFade(0, _fadeTime);
            blackScreenEffectCG.blocksRaycasts = false;
            blackScreenEffectCG.interactable = false;
            if (blackScreenEffectCG.TryGetComponent<Image>(out Image img))
                img.raycastTarget = false;

            TimeToggle(true, 1f);
        }
    }

    public void DisplayVintage(Sprite _color)
    {
        if (vintageImage.gameObject.activeInHierarchy) return;

        //TODO Sounds Calling
        SoundManager.Instance?.PlayCoinPickupSound();

        vintageImage.sprite = _color;
        vintageImage.gameObject.SetActive(true);

        if (!vintageImage.gameObject.activeInHierarchy) return;
        StartCoroutine(DisableVintageScreen());

        IEnumerator DisableVintageScreen()
        {
            yield return new WaitForSeconds(.25f);

            vintageImage.gameObject.SetActive(false);
        }
    }

    public void DisplayDialogue(string dialogue)
    {
        if (objectiveContainer.activeInHierarchy) OnObjectiveOKButton();

        StartCoroutine(Dialogue());

        IEnumerator Dialogue()
        {
            yield return new WaitForSeconds(0f);

            objectiveText.text = dialogue;
            objectiveContainer.SetActive(true);

            if (!dialogueCanvas.activeInHierarchy) dialogueCanvas.SetActive(true);

            inGameScreen.SetActive(false);

            //TODO Ads Calling
            //AdsManager.instance.showAdMobRectangleBannerBottomLeft();
        }
    }

    public void OnObjectiveOKButton()
    {
        //TODO Sound Calling
        SoundManager.Instance?.PlayOnButtonSound();

        //TODO Sound Calling
        //SoundManager.Instance.ChangeVolume(musicMuteSound, sfxMuteSound);

        if (!objectiveContainer.activeInHierarchy) return;

        objectiveContainer.SetActive(false);

        inGameScreen.SetActive(true);

        //FadeScreen(1.5f);

        //TODO Ads Calling
        //AdsManager.instance.hideAdmobBottomLeftBanner();
        //AdsManager.instance.showAdMobBannerTopLeft();
    }

    #endregion

    #region Spawner

    //! Evironment_Level
    private void SpawnLevel()
    {
        EnableEnvironment();

        EnableLevel(levelsParentObj);

        void EnableEnvironment()
        {
            selectedEnvironment = environmentObj;
            selectedEnvironment.SetActive(true);
        }
        void EnableLevel(GameObject parentObj) // ! Instantiate level from resources
        {
            if(!parentObj.activeInHierarchy) parentObj.SetActive(true);

            selectedLevel = parentObj.transform.GetChild(levelNumber).gameObject;
            selectedLevel.SetActive(true);

            LevelSetup();

            void LevelSetup() // ! Setup spawned level
            {
                levelInfo = selectedLevel.GetComponent<LevelInfo>();

                useTimer = levelInfo.useTimer;

                if (!levelInfo.useTimer && timerContainer) timerContainer.SetActive(false);
                //if (levelInfo.useEngineStartButton) engineStartPanel = engineStartState;
            }
        }
    }

    //! RCC
    private void SpawnRCCV3() // ! Spawn player from resouces, also setups player after instentiate
    {
        if (!levelInfo.vehicleSpawnPosition) return;

        if (!vehiclesParentObj.activeInHierarchy) vehiclesParentObj.SetActive(true);
        
        selectedVehicle = vehiclesParentObj.transform.GetChild(vehicleNumber).gameObject;

        selectedVehicle.transform.position = levelInfo.vehicleSpawnPosition.position;
        selectedVehicle.transform.rotation = levelInfo.vehicleSpawnPosition.rotation;
        selectedVehicle.SetActive(true);

        //selectedVehicleRCC = selectedVehicle.GetComponent<RCC_CarControllerV3>();
        rccAudioListener = rccMainCamera.GetComponent<AudioListener>();

        //selectedVehicleIdleVolume = selectedVehicleRCC.idleEngineSoundVolume; // ! Store sound values of spawned vehicle
        //selectedVehicleMinVolume = selectedVehicleRCC.minEngineSoundVolume; // ! Store sound values of spawned vehicle
        //selectedVehicleMaxVolume = selectedVehicleRCC.maxEngineSoundVolume; // ! Store sound values of spawned vehicle

        //selectedVehicleRCC.Rigid.Sleep(); // ! sleeps the physcis 

        selectedVehicleLastTransform = selectedVehicle.transform; // ! Set's last transform to player spawned player transform

        //selectedVehicleRCC.engineRunning = EngineRunning();

        bool EngineRunning()
        {
            if (enginestartButton && enginestartButton.activeInHierarchy)
                return true;

            return false;
        }
    }

    public void SetVehicleTransform(Transform _transform) // ! Reset's player position and rotation to desired position and rotation
    {
        selectedVehicle.transform.position = new Vector3(_transform.position.x,
            selectedVehicle.transform.position.y, _transform.position.z); // ! Reset's position

        selectedVehicle.transform.rotation = Quaternion.Euler(_transform.eulerAngles.x,
            _transform.eulerAngles.y, _transform.eulerAngles.z);  // ! Resets rotation

        //selectedVehicleRCC.Rigid.velocity = Vector3.zero; // ! Set's physics velocity to 0
        //selectedVehicleRCC.Rigid.angularVelocity = Vector3.zero; // ! Set's angular drag to 0
        //selectedVehicleRCC.Rigid.Sleep(); // ! Sleep's the physics
    }

    public void RCCVolume(float _idle, float _min, float _max) // ! Set's rcc vehicle engine volume
    {
        //selectedVehicleRCC.idleEngineSoundVolume = _idle;
        //selectedVehicleRCC.minEngineSoundVolume = _min;
        //selectedVehicleRCC.maxEngineSoundVolume = _max;
    }

    #endregion

    #region Button Events

    public void OnPauseButton() => LevelPaused();
    public void OnResumeButton() => LevelResume();
    public void OnHomeButton() => Loading();
    public void OnRestartButton()
    {
        //TODO Sounds Calling
        SoundManager.Instance?.PlayOnButtonSound();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        //TODO Ads Calling
        //AdsManager.instance.hideAllAdmobBanners();
    }
    public void OnNextButton()
    {
        //TODO Sound Calling
        SoundManager.Instance?.PlayOnButtonSound();

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            StartCoroutine(ShowInterstitial());
        }
        else
        {
            RCCVolume(selectedVehicleIdleVolume, selectedVehicleMinVolume, selectedVehicleMaxVolume);
            TimeToggle(false, 1f);

            levelNumber++;

           if (levelNumber < currentLevelSelectSO.totalLevels)
           {
               PlayerPrefs.SetInt(currentLevelSelectSO.prefName, PlayerPrefs.GetInt(currentLevelSelectSO.prefName));
               setupScriptable.sLevelIndex = levelNumber;
               SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
           }    
           else
           {
               setupScriptable.sModeIndex = setupScriptable.sModeIndex++;

                if (setupScriptable.sModeIndex >= LevelSelect.totalModeLevels) setupScriptable.sModeIndex = 0;

                setupScriptable.sLevelIndex = 0;
               SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
           }
        }

        IEnumerator ShowInterstitial()
        {
            RCCVolume(selectedVehicleIdleVolume, selectedVehicleMinVolume, selectedVehicleMaxVolume);
            TimeToggle(false, 1f);

            state = GameplayScreens.AdsLoading;
            UpdateGameplayState();

            //TODO Ads Calling
            //AdsManager.instance.hideAllAdmobBanners();

            yield return new WaitForSecondsRealtime(1f);

            //TODO Ads Calling
            //AdsManager.instance.showAdmobInterstitial();

            yield return new WaitForSecondsRealtime(.5f);

            levelNumber++;

           if (levelNumber < currentLevelSelectSO.totalLevels)
           {
               PlayerPrefs.SetInt(currentLevelSelectSO.prefName, PlayerPrefs.GetInt(currentLevelSelectSO.prefName));
               setupScriptable.sLevelIndex = levelNumber;
               SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
           }
           else
           {
               setupScriptable.sModeIndex = 0;
               setupScriptable.sLevelIndex = 0;
               SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
           }

            // ! Garbage Collection
            //if (Tweaks.Instance != null)
            //    Tweaks.Instance.CollectGarbage();
        }
    }
    public void OnEngineStartButton()
    {
        // TODO Sound Calling
        SoundManager.Instance?.PlayOnButtonSound();

        StartCoroutine(StartVehicleEngine());

        IEnumerator StartVehicleEngine() // ! Start engine with a delay
        {
            state = GameplayScreens.None;
            UpdateGameplayState();

            enginestartButton.SetActive(false);

            //if (sceneManager.activeMainCamera.TryGetComponent<CameraShaker>(out CameraShaker camShaker))
            //    camShaker.ShakeOnce(2.5f, 5f, .5f, .5f);

            //TODO Sound Calling
            SoundManager.Instance?.PlayEngineStartSound();

            yield return new WaitForSeconds(1.5f);

            ToggleVehicleEngine(true);

            engineStopButton.SetActive(true);

            //selectedVehicleRCC.engineRunning = true;

            canvasesAnimator.Play("CanvasFadeAnimation");

            state = GameplayScreens.Resume;
            UpdateGameplayState();
            controls.SetActive(true);

            //selectedVehicleRCC.Rigid.constraints = RigidbodyConstraints.None;


            RCCVolume(selectedVehicleIdleVolume, selectedVehicleMinVolume, selectedVehicleMaxVolume);

            //TODO Sound Calling
            //SoundManager.Instance.ChangeVolume(musicMuteSound, sfxMuteSound);
        }
    }
    public void OnEngineStopButton()  // ! Stop engine funtionality
    {
        //selectedVehicleRCC.Rigid.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        
        //selectedVehicleRCC.engineRunning = true;

        controls.SetActive(false);
        state = GameplayScreens.Resume;
        UpdateGameplayState();

        ToggleVehicleEngine(false);
    }
    private void ToggleVehicleEngine(bool _state) // ! Toggle vehicle engine
    {
        if (_state)
        {
            enginestartButton.SetActive(false);
            engineStopButton.SetActive(true);
            
            //selectedVehicleRCC.StartEngine(true);

            RCCVolume(selectedVehicleIdleVolume, selectedVehicleMinVolume, selectedVehicleMaxVolume);
        }
        else
        {
            enginestartButton.SetActive(true);
            engineStopButton.SetActive(false);

            //selectedVehicleRCC.KillEngine();

            RCCVolume(0f, 0f, 0f);
        }
    }

    #endregion
}
