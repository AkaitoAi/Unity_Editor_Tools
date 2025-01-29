using System;
using UnityEngine;

public class EnergyManager : Singleton<EnergyManager>
{
    private int maxEnergy = 100;

    private string currency = "EnergyCurrency";
    private string intialSetup = "FirstTimeEnergyCurrencySetup";

    public Action<int> OnEnergyTransition;

    private void Start()
    {
        CurrentEnergy();

        if (PlayerPrefs.GetInt(intialSetup) == 1) return;
        PlayerPrefs.SetInt(intialSetup, 1);

        PurchaseEnergy(maxEnergy);
    }

    public bool HasEnergy(int amount)
    { 
        return PlayerPrefs.GetInt(currency) >= amount? true : false;
    }

    private void CurrentEnergy()
    {
        PlayerPrefs.GetInt(currency);

        OnEnergyTransition?.Invoke(PlayerPrefs.GetInt(currency));
    }

    public void UseEnergy(int amount)
    {
        if (!HasEnergy(amount)) return;

        PlayerPrefs.SetInt(currency, PlayerPrefs.GetInt(currency) - amount);

        OnEnergyTransition?.Invoke(PlayerPrefs.GetInt(currency));
    }

    public void PurchaseEnergy(int amount)
    {
        if (HasEnergy(maxEnergy)) return;

        PlayerPrefs.SetInt(currency, PlayerPrefs.GetInt(currency) + amount);

        OnEnergyTransition?.Invoke(PlayerPrefs.GetInt(currency));
    }
}
