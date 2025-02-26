using System;
using UnityEngine;

public class MovePathUpdateManager : MonoBehaviour
{
    [Range(1, 10)][SerializeField] private int interval = 3;

    public static Action OnUpdate;

    private void Update()
    {
        if (Time.frameCount % interval != 0) return;

        OnUpdate?.Invoke();
    }
}
