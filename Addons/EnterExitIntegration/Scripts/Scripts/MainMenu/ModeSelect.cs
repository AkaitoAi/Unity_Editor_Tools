using UnityEngine;
using UnityEngine.UI;

public class ModeSelect : MonoBehaviour
{
    [SerializeField] private Button[] modeButtons;
    [SerializeField] private Button[] skipLevelSelectionButtons;

    private void Start()
    {
        if(modeButtons == null || modeButtons.Length <= 0) return;
        
        // Assign OnModeSelect to modeButtons with their respective indexes
        for (int i = 0; i < modeButtons.Length; i++)
        {
            // Create local copy of index for closure
            int index = i;
            modeButtons[i].onClick.AddListener(() => OnModeSelect(index));
        }

        if (skipLevelSelectionButtons == null || skipLevelSelectionButtons.Length <= 0) return;

        // Assign OnModeSkipLevel to skipLevelSelectionButtons with their respective indexes
        for (int i = 0; i < skipLevelSelectionButtons.Length; i++)
        {
            // Create local copy of index for closure
            int index = i;
            skipLevelSelectionButtons[i].onClick.AddListener(() => OnModeSkipLevel(index));
        }
    }

    //! Loads data n setup level selection UI
    public void OnModeSelect(int _index)
    {
        MenuManager.GetInstance().setupScriptable.sModeIndex = _index;

        EventBus<OnModeSelected>.Raise(new OnModeSelected { });
    }
    
    //! Loads data only
    public void OnModeSkipLevel(int _index)
    {
        MenuManager.GetInstance().setupScriptable.sModeIndex = _index;

        EventBus<OnModeSkipLevelSelected>.Raise(new OnModeSkipLevelSelected { });
    }
}
