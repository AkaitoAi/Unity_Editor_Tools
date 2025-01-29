using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class EnergyText : MonoBehaviour
{
	[SerializeField] private TMP_Text text;

    private void Start()
    {
        EnergyManager instance = Singleton<EnergyManager>.Instance;
        instance.OnEnergyTransition = (Action<int>)Delegate.Combine(instance.OnEnergyTransition, new Action<int>(UpdateText));
    }

    private void UpdateText(int amount)
	{ 
		text.text = $"{ amount}";
        text.rectTransform.DOKill();
        text.rectTransform.localScale = Vector2.one;
        text.rectTransform.DOPunchScale(Vector2.one * 0.1f, 0.4f, 0);

    }

    private void OnDestroy()
    {
        if (Singleton<EnergyManager>.Instance != null)
        {
            EnergyManager instance = Singleton<EnergyManager>.Instance;
            instance.OnEnergyTransition = (Action<int>)Delegate.Remove(instance.OnEnergyTransition, new Action<int>(UpdateText));
        }
    }
}
