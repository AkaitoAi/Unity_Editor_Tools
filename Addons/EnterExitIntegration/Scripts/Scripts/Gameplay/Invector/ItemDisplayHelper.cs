using Invector.vItemManager;
using UnityEngine;
using UnityEngine.Events;

public class ItemDisplayHelper : MonoBehaviour
{
    [SerializeField] private vEquipmentDisplay vEquipmentDisplay;

    public UnityEvent OnArmedEvent, OnUnArmedEvent;

    private void OnEnable()
    {
        UpdateUnArmedIcon();
    }
    public void UpdateUnArmedIcon()
    {
        if (vEquipmentDisplay == null) return;

        if (vEquipmentDisplay.hasItem)
        {
            OnArmedEvent?.Invoke();

            return;
        }

        OnUnArmedEvent?.Invoke();
    }
}
