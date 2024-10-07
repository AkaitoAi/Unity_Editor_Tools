using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TimeOfDay : MonoBehaviour
{
    [SerializeField] private LightSettingSO lightSetting;
    [SerializeField] private Transform[] switchObjects;
    [SerializeField] private Button[] switchButtons;
    internal int switchIndex = 0;

    [SerializeField] private string prefsName = "TimeOfDay";
    internal int index;

    public static event Action<int> OnTimeOfDayAction;
    public UnityEvent OnStartEvent;

    private void Start()
    {
        OnStartEvent?.Invoke();
    }

    public void OnWeatherSwitch(int index)
    {
        foreach (Transform switchObject in switchObjects)
            switchObject.gameObject.SetActive(false);

        foreach (Button btn in switchButtons)
            btn.interactable = true;

        switchObjects[index].gameObject.SetActive(true);
        switchButtons[index].interactable = false;
        lightSetting.SceneLightSetup(lightSetting.lightSettings[index]);

        PlayerPrefs.SetInt(prefsName, index);

        OnTimeOfDayAction?.Invoke(index);
    }

    public void SequenceWeatherSwitch()
    {
        switchIndex = PlayerPrefs.GetInt(prefsName, switchIndex);

        for (index = 0; index < switchObjects.Length; index++)
        {
            if (index == switchIndex)
            {
                switchObjects[index].gameObject.SetActive(true);
                lightSetting.SceneLightSetup(lightSetting.lightSettings[index]);
            }
            else switchObjects[index].gameObject.SetActive(false);
        }

        if (switchIndex < switchObjects.Length - 1) switchIndex++;
        else switchIndex = 0;

        PlayerPrefs.SetInt(prefsName, switchIndex);

        OnTimeOfDayAction?.Invoke(switchIndex);
    }
}
