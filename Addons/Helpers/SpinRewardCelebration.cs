using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SpinRewardCelebration : MonoBehaviour
{
    [SerializeField] private Image icon, title, backgroundShine;

    [Space(10)]
    [SerializeField] private Reward[] rewards;

    private void OnEnable()
    {
        CasinoWheel.OnSlotSelectedAction += OnRewardByID;

        if (icon != null)
        {
            icon.SetNativeSize();
            icon.transform.localScale = Vector3.zero;
            icon.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                backgroundShine.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);

                if (title != null)
                {
                    title.SetNativeSize();
                    title.transform.localScale = Vector3.zero;
                    title.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
                }
            });
        }
    }

    private void OnDisable()
    {
        CasinoWheel.OnSlotSelectedAction -= OnRewardByID;
    }

    private void OnRewardByID(int id)
    {
        if (icon != null)
        {
            icon.sprite = rewards[id].icon;  
        }

        if (title != null)
        {
            title.sprite = rewards[id].title;
        }
    }

    [Serializable]
    public struct Reward
    {
        public Sprite icon;
        public Sprite title;
    }
}
