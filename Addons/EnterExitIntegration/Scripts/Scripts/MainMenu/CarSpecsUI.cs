using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CarSpecsUI : MonoBehaviour
{
    [Header("Vehicle Specs Filler")]
    [SerializeField] bool useSpecsFiller = false;
    [SerializeField] private bool useFillerLerp = false;
    [SerializeField] private Image[] specsFillers;

    [Header("Vehicle Specs Text")]
    [SerializeField] bool useSpecsText = false;
    [SerializeField] private bool useTextLerp = false;
    [SerializeField] private Text[] specsTexts;

    [Header("Vehicle Stats")]
    [SerializeField] private float lerpDuration = 3f;

    public void UpdateVehicleSpecs(VehicleSpecsSO _vehcileStats)
    {
        if (useSpecsFiller) UpdateVehicleStatsFiller(_vehcileStats);
        if (useSpecsText) UpdateVehicleStatsText(_vehcileStats);
        if (useFillerLerp) StartCoroutine(VehicleStatsFillerLerp(_vehcileStats));
        if (useTextLerp) StartCoroutine(VehicleStatsTextLerp(_vehcileStats));
    }

    private void UpdateVehicleStatsFiller(VehicleSpecsSO _vehcileStats)
    {
        for (int i = 0; i < specsFillers.Length; i++)
            specsFillers[i].fillAmount = (float)_vehcileStats.specs[i] / 100f;
    }
    
    private void UpdateVehicleStatsText(VehicleSpecsSO _vehcileStats)
    {
        for (int i = 0; i < specsTexts.Length; i++)
            specsTexts[i].text = _vehcileStats.specs[i].ToString();
    }

    IEnumerator VehicleStatsFillerLerp(VehicleSpecsSO _vehicleStats)
    {
        float timeElapsed = 0;
        while (timeElapsed < lerpDuration)
        {
            for (int i = 0; i < specsFillers.Length; i++)
            {
                specsFillers[i].fillAmount = Mathf.LerpUnclamped((float)specsFillers[i].fillAmount * 100f
                    , _vehicleStats.specs[i], timeElapsed / lerpDuration) / 100f;
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        for (int i = 0; i < specsFillers.Length; i++)
            specsFillers[i].fillAmount = (float)_vehicleStats.specs[i] / 100f;
    }


    IEnumerator VehicleStatsTextLerp(VehicleSpecsSO _vehicleStats)
    {
        float timeElapsed = 0;
        while (timeElapsed < lerpDuration)
        {
            for (int i = 0; i < specsTexts.Length; i++)
            {
                specsTexts[i].text = 
                    Mathf.FloorToInt(Mathf.LerpUnclamped(
                        0f
                        , _vehicleStats.specs[i]
                        , timeElapsed / lerpDuration)).ToString();
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < specsTexts.Length; i++)
            specsTexts[i].text = _vehicleStats.specs[i].ToString();
    }
}
