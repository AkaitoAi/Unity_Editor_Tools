using UnityEngine;

[CreateAssetMenu(fileName = "Setup", menuName = "ScriptableObjects/Setup", order = 1)]
public class SetupSO : ScriptableObject
{
    public int sVehicleIndex;
    public int sModeIndex;
    public int sLevelIndex;

    public string masterVolumePref;
    public string sFXVolumePref;
    public string bGVolumePref;
    public string muteAudioPref;
    public string musicMutePref;
    public string sFXMutePref;
    
    public string controlPref;
    
    public string levelCoinsPref;
    public string totalCoinsPref;
    
    public string vehiclesPref;
    
    public string qualitySettingPref;
    public string shadowSettingPref;
    public string cameraFarPref;

    public string mapPref;
    public string speedMeterPref;
    public string vibrationPref;
    public string steeringSensitivityPref;

    
    public string setupPref;
}
