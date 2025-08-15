using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System;
using RichTap;
using UnityEngine.Events;

public class CasinoWheel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Image wheelImage;

    [Header("Spin Settings")]
    [SerializeField] private float spinDuration = 3f;
    [SerializeField] private int minFullRotations = 3;
    [SerializeField] private int maxFullRotations = 6;
    [SerializeField] private int slotCount = 8;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip tickClip;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.3f;

    [Header("Visual Alignment")]
    [Tooltip("Extra degrees to align the pointer exactly to the slot's center. Measure in Unity.")]
    [SerializeField] private float centerOffset = 0f;

    private const string LastSlotKey = "Wheel_LastSlotIndex";
    private bool isSpinning = false;
    private Coroutine tickCoroutine;
    private float slotAngle;
    private float baseOffset;

    public UnityEvent OnSpinCompletedEvent;
    public static event Action<int> OnSlotSelectedAction;

    private void Start()
    {
        slotAngle = 360f / slotCount;
        baseOffset = wheelImage.rectTransform.eulerAngles.z;

        int lastSlot = PlayerPrefs.GetInt(LastSlotKey, 0);
        SetWheelToSlot(lastSlot);

        if (spinButton != null)
        {
            spinButton.interactable = true;
            spinButton.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayOnButtonSound();
                spinButton.interactable = false;
                SpinToRandom();
            });
        }
    }

    private void SetWheelToSlot(int slotIndex)
    {
        float angle = SlotIndexToAngle(slotIndex);
        wheelImage.rectTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private float SlotIndexToAngle(int slotIndex)
    {
        return baseOffset + (slotIndex * slotAngle) + centerOffset;
    }

    private int AngleToSlotIndex(float angle)
    {
        float adjusted = (angle - baseOffset - centerOffset + 360f) % 360f;
        return Mathf.FloorToInt(adjusted / slotAngle) % slotCount;
    }

    public void SpinToRandom()
    {
        int lastSlot = PlayerPrefs.GetInt(LastSlotKey, 0);
        int newSlot;
        do
        {
            newSlot = UnityEngine.Random.Range(0, slotCount);
        }
        while (newSlot == lastSlot);
        SpinToSlot(newSlot);
    }

    public void SpinToSlot(int slotIndex)
    {
        if (isSpinning) return;
        isSpinning = true;

        float startAngle = wheelImage.rectTransform.eulerAngles.z;
        float targetAngle = SlotIndexToAngle(slotIndex);

        int fullRotations = UnityEngine.Random.Range(minFullRotations, maxFullRotations + 1);
        float finalAngle = targetAngle + (fullRotations * 360f);

        if (tickCoroutine != null) StopCoroutine(tickCoroutine);
        tickCoroutine = StartCoroutine(PlayTickSound(slotAngle));

        wheelImage.rectTransform
            .DORotate(new Vector3(0, 0, finalAngle), spinDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                if (tickCoroutine != null) StopCoroutine(tickCoroutine);
                isSpinning = false;

                wheelImage.rectTransform.rotation = Quaternion.Euler(0, 0, targetAngle);

                PlayerPrefs.SetInt(LastSlotKey, slotIndex);
                PlayerPrefs.Save();

                OnSlotSelectedAction?.Invoke(slotIndex);
                if (spinButton != null) spinButton.interactable = true;

                OnSpinCompletedEvent?.Invoke();

                Debug.Log($"Landed on slot {slotIndex}");
            });
    }

    private IEnumerator PlayTickSound(float slotAngle)
    {
        float lastAngle = wheelImage.rectTransform.eulerAngles.z;
        int lastSlotIndex = Mathf.FloorToInt(lastAngle / slotAngle);
        float lastTime = Time.time;

        while (isSpinning)
        {
            float currentAngle = wheelImage.rectTransform.eulerAngles.z;
            int currentSlotIndex = Mathf.FloorToInt(currentAngle / slotAngle);

            if (currentSlotIndex != lastSlotIndex)
            {
                int diff = Mathf.Abs(currentSlotIndex - lastSlotIndex);
                if (diff > slotCount / 2) diff = slotCount - diff;

                for (int i = 0; i < diff; i++)
                {
                    float now = Time.time;
                    float timeDiff = now - lastTime;
                    lastTime = now;

                    float rotationSpeed = 1f / Mathf.Max(timeDiff, 0.0001f);
                    float t = Mathf.Clamp01(rotationSpeed / 15f);
                    float pitch = Mathf.Lerp(minPitch, maxPitch, t);

                    if (audioSource != null && tickClip != null)
                    {
                        audioSource.pitch = pitch;
                        audioSource.PlayOneShot(tickClip);
                        RichtapEffectSource.Instance?.Play(RichTap.Common.RichtapPreset.RT_SOFT_CLICK);
                    }
                }
                lastSlotIndex = currentSlotIndex;
            }
            yield return null;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Test Spin All Slots")]
    private void TestSpinAllSlots()
    {
        StartCoroutine(TestSpinRoutine());
    }

    private IEnumerator TestSpinRoutine()
    {
        for (int i = 0; i < slotCount; i++)
        {
            SetWheelToSlot(i);
            yield return new WaitForSeconds(0.5f);
        }
    }
#endif
}
