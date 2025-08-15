using UnityEngine;
using AkaitoAi.Singleton;
using Lean.Pool;
using AkaitoAi.Extensions;
using System;

public class EmojiPooler : Singleton<EmojiPooler>
{
    [SerializeField] private GameObject[] emojiPrefabs;
    [SerializeField] private float duration = 2.5f;

    public void RandomEmoji(Vector3 position = default, Quaternion rotation = default, Action onDespawnAction = null)
    {
        GameObject pool = LeanPool.Spawn(emojiPrefabs[UnityEngine.Random.Range(0, emojiPrefabs.Length)], position, rotation, transform);

        pool.SetActive(true);

        StartCoroutine(AkaitoAiExtensions.SimpleDelay(duration, () => 
        { 
            LeanPool.Despawn(pool); 

            onDespawnAction?.Invoke();
        }));
    }
}
