using System;
using System.Collections;
using UnityEngine;

public enum ParkingType
{ 
    None,
    SimpleWin,
    WinAfterCutscene
}
public class ParkingCollectibles : MonoBehaviour, ICollectible
{
    public ParkingType parkingType;
    public Transform alignTransform;

    public static event Action<Transform, ParkingType> OnParked;

    internal WaitForSeconds delay = new WaitForSeconds(.25f);

    public void OnCollect()
    {
        StartCoroutine(ParkingDelay());

        IEnumerator ParkingDelay()
        {
            yield return delay;

            OnParked?.Invoke(alignTransform, parkingType);
        }
    }
}
