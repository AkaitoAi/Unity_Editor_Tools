using UnityEngine;

[CreateAssetMenu(fileName = "LevelSelect", menuName = "ScriptableObjects/LevelSelect_", order = 1)]
public class LevelSelectSO : ScriptableObject
{
    public int totalLevels;
    public bool useLevelStatus;
    
    public bool hasSeparateLevelNumber;
    public int levelNumberChildCount;
    
    public bool hasLevelLock;
    public int levelLockChildCount;
    
    public bool hasCurrentLevel;
    public int currentLevelChildCount;
    
    public bool hasLevelComplete;
    public int levelCompleteChildCount;
    
    public float scrollSoundDelay;
    public float ContentLeft;
    public float ContentRight;

    public string prefName;
    public int levelForm;
}
