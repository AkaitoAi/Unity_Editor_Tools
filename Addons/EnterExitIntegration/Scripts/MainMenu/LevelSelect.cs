using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    [SerializeField] private LevelDetails[] modeLevels;

    internal SetupSO setupScriptable;

    public static int totalModeLevels;


    //Bindings
    EventBinding<OnModeSelected> modeSelectedEventBinding;
    EventBinding<OnModeSkipLevelSelected> modeSkipLevelSelectedEventBinding;

    [System.Serializable]
    public struct LevelDetails
    {
        internal LevelSelectSO levelSelectSO;
        public Button[] buttons;
        
        public bool unlockAllLevels;

        public GameObject parentContainer;
        public RectTransform scrollViewRectTransform;
        public ScrollRect scrollRect;
        public RectTransform contentRectTransfrom;

        internal float oldPos;
    }

    private void ContentSetup()
    {
        if (modeLevels == null) return;

        LoadData();

        foreach (LevelDetails _mLevels in modeLevels)
        {
            if (_mLevels.contentRectTransfrom) _mLevels.contentRectTransfrom.gameObject.SetActive(false);
            if (_mLevels.scrollRect) _mLevels.scrollRect.gameObject.SetActive(false);
            if (_mLevels.parentContainer) _mLevels.parentContainer.SetActive(false);
        }

        if (modeLevels[setupScriptable.sModeIndex].buttons.Length > 0 ||
          modeLevels[setupScriptable.sModeIndex].buttons != null)
        {
            // Assign OnModeSelect to modeButtons with their respective indexes
            for (int i = 0; i < modeLevels[setupScriptable.sModeIndex].buttons.Length; i++)
            {
                // Create local copy of index for closure
                int index = i;
                modeLevels[setupScriptable.sModeIndex].buttons[i].onClick.AddListener(() => SelectLevel(index));
            }
        }

        modeLevels
            [setupScriptable.sModeIndex].
            contentRectTransfrom.gameObject.SetActive(true);
        modeLevels
            [setupScriptable.sModeIndex].
            scrollRect.gameObject.SetActive(true);
        
        if(modeLevels[setupScriptable.sModeIndex].parentContainer)
            modeLevels[setupScriptable.sModeIndex].parentContainer.SetActive(true);

        EnableModeLevelButtons(modeLevels[setupScriptable.sModeIndex].levelSelectSO.totalLevels);
        SetLeft(modeLevels[setupScriptable.sModeIndex].contentRectTransfrom, modeLevels[setupScriptable.sModeIndex].levelSelectSO.ContentLeft);
        SetRight(modeLevels[setupScriptable.sModeIndex].contentRectTransfrom, modeLevels[setupScriptable.sModeIndex].levelSelectSO.ContentRight);

        int length = modeLevels[setupScriptable.sModeIndex].buttons.Length;
        for (int i = 0; i < length; i++)
            LockLevel(i);

        void EnableModeLevelButtons(int _levels)
        {
            foreach (Button levelBtn in modeLevels[setupScriptable.sModeIndex].buttons)
                levelBtn.gameObject.SetActive(false);

            int length = _levels;
            for (int i = 0; i < length; i++)
                modeLevels[setupScriptable.sModeIndex].buttons[i].gameObject.SetActive(true);
        }

        void SetLeft(RectTransform rt, float left) =>
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);

        void SetRight(RectTransform rt, float right) =>
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);

        void LockLevel(int _level)
        {
            if (_level > PlayerPrefs.GetInt(modeLevels[setupScriptable.sModeIndex].levelSelectSO.prefName))
            {
                ToggleLevelLock(_level, false);
            }
            else
            {
                setupScriptable.sLevelIndex = _level;
                ToggleLevelLock(_level, true);
            }

            void ToggleLevelLock(int _index, bool _state)
            {
                if (modeLevels[setupScriptable.sModeIndex].levelSelectSO.hasLevelLock)
                    modeLevels[setupScriptable.sModeIndex].buttons[_index].
                        transform.GetChild(modeLevels[setupScriptable.sModeIndex].levelSelectSO.levelLockChildCount).gameObject.SetActive(!_state);

                if (modeLevels[setupScriptable.sModeIndex].levelSelectSO.hasSeparateLevelNumber)
                    modeLevels[setupScriptable.sModeIndex].buttons[_index].
                        transform.GetChild(modeLevels[setupScriptable.sModeIndex].levelSelectSO.levelNumberChildCount).gameObject.SetActive(_state);

                if (modeLevels[setupScriptable.sModeIndex].buttons[_index].TryGetComponent<Button>(out Button _btn1)) _btn1.interactable = _state;

                TogglePlayedStatusScale(_index);
                
                void TogglePlayedStatusScale(int _levelIndex)
                {
                    if (!modeLevels[setupScriptable.sModeIndex].levelSelectSO.useLevelStatus) return;

                    if(modeLevels[setupScriptable.sModeIndex].buttons[_index].TryGetComponent<Button>(out Button _btn))
                    {
                        if (!_btn.interactable) return;
                    }

                    modeLevels[setupScriptable.sModeIndex].levelSelectSO.levelForm = 
                        PlayerPrefs.GetInt(modeLevels[setupScriptable.sModeIndex].levelSelectSO.prefName);

                    if (_levelIndex == modeLevels[setupScriptable.sModeIndex].levelSelectSO.levelForm)
                    {
                        ToggleButtonShiny(true);
                        ToggleButtonAnimation(true);
                        TogglePlayStatus(true);
                    }

                    if (_levelIndex < modeLevels[setupScriptable.sModeIndex].levelSelectSO.levelForm)
                    {
                        ToggleButtonShiny(false);
                        ToggleButtonAnimation(false);
                        TogglePlayStatus(false);
                    }

                    void ToggleButtonShiny(bool _state)
                    {
                        //if (modeLevels[setupScriptable.sModeIndex].buttons[_levelIndex].TryGetComponent<UIShiny>(out UIShiny uiShiny)) uiShiny.enabled = _state;
                    }
                    void ToggleButtonAnimation(bool _state)
                    {
                        if (modeLevels[setupScriptable.sModeIndex].buttons[_levelIndex].TryGetComponent<Animation>(out Animation animation)) animation.enabled = _state;
                    }

                    void TogglePlayStatus(bool _state)
                    {
                        if (!modeLevels[setupScriptable.sModeIndex].levelSelectSO.hasLevelComplete && 
                            !modeLevels[setupScriptable.sModeIndex].levelSelectSO.hasCurrentLevel)
                            return;
                            
                        modeLevels[setupScriptable.sModeIndex].buttons[_levelIndex].
                            transform.GetChild(modeLevels[setupScriptable.sModeIndex].levelSelectSO.currentLevelChildCount).gameObject.SetActive(_state);
                            
                        modeLevels[setupScriptable.sModeIndex].buttons[_levelIndex].
                            transform.GetChild(modeLevels[setupScriptable.sModeIndex].levelSelectSO.levelCompleteChildCount).gameObject.SetActive(!_state);
                    }
                }
            }
        }
    }

    private void LoadData()
    {
        if (modeLevels == null) return;

        setupScriptable = 
            MenuManager.GetInstance().setupScriptable;

        int modeLevelsLength = modeLevels.Length;
        for (int i = 0; i < modeLevelsLength; i++)
            modeLevels[i].levelSelectSO =
                Resources.Load(
                    "ScriptableObjects/LevelSelect/LevelSelect_" + i
                    ) 
                as LevelSelectSO;

        totalModeLevels = modeLevels.Length;

        //! Unlocks All Levels
        if (modeLevels[setupScriptable.sModeIndex].unlockAllLevels)
            PlayerPrefs.SetInt(modeLevels[setupScriptable.sModeIndex].levelSelectSO.prefName,
                modeLevels[setupScriptable.sModeIndex].levelSelectSO.totalLevels);
    }

    public void SelectLevel(int _index) => setupScriptable.sLevelIndex = _index;

    public void ScrollRectSound()
    {
        if (modeLevels[setupScriptable.sModeIndex].scrollRect.horizontalNormalizedPosition > 
            (modeLevels[setupScriptable.sModeIndex].oldPos + modeLevels[setupScriptable.sModeIndex].levelSelectSO.scrollSoundDelay))
        {
            modeLevels[setupScriptable.sModeIndex].oldPos = modeLevels[setupScriptable.sModeIndex].scrollRect.horizontalNormalizedPosition;

            //TODO Sound Calling
            SoundManager.Instance.PlayScrollRectSound();
        }
        else if (modeLevels[setupScriptable.sModeIndex].scrollRect.horizontalNormalizedPosition < 
            (modeLevels[setupScriptable.sModeIndex].oldPos - modeLevels[setupScriptable.sModeIndex].levelSelectSO.scrollSoundDelay))
        {
            modeLevels[setupScriptable.sModeIndex].oldPos = modeLevels[setupScriptable.sModeIndex].scrollRect.horizontalNormalizedPosition;

            //TODO Sound Calling
            SoundManager.Instance.PlayScrollRectSound();
        }
    }

    private void OnEnable()
    {
        modeSelectedEventBinding = new EventBinding<OnModeSelected>(ContentSetup);
        EventBus<OnModeSelected>.Register(modeSelectedEventBinding);

        modeSkipLevelSelectedEventBinding = new EventBinding<OnModeSkipLevelSelected>(LoadData);
        EventBus<OnModeSkipLevelSelected>.Register(modeSkipLevelSelectedEventBinding);
    }

    private void OnDisable()
    {
        EventBus<OnModeSelected>.Deregister(modeSelectedEventBinding);
        EventBus<OnModeSkipLevelSelected>.Deregister(modeSkipLevelSelectedEventBinding);

    }
}
