using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LoadingScreenFader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image imageA;
    [SerializeField] private Image imageB;
    [SerializeField] private Sprite[] sprites;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private bool loop = true;

    private List<int> _availableIndices = new List<int>();
    private bool _usingImageA = true; // Track which image is active

    private void Start()
    {
        if (imageA == null || imageB == null)
        {
            Debug.LogError("Missing Image references!");
            return;
        }

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("No sprites assigned!");
            return;
        }

        InitializeIndices();
        imageA.sprite = GetNextSprite();
        imageA.color = Color.white;
        imageB.color = new Color(1, 1, 1, 0); // Fully transparent
        StartCoroutine(ImageCycle());
    }

    private void InitializeIndices()
    {
        _availableIndices = Enumerable.Range(0, sprites.Length).OrderBy(x => Random.value).ToList();
    }

    private IEnumerator ImageCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(displayDuration);

            if (_availableIndices.Count == 0 && !loop) yield break;

            Sprite nextSprite = GetNextSprite();
            if (nextSprite == null) yield break;

            yield return StartCoroutine(FadeTransition(nextSprite));
        }
    }

    private IEnumerator FadeTransition(Sprite nextSprite)
    {
        Image fadeOutImage = _usingImageA ? imageA : imageB;
        Image fadeInImage = _usingImageA ? imageB : imageA;

        fadeInImage.sprite = nextSprite;
        fadeInImage.color = new Color(1, 1, 1, 0); // Start transparent

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            fadeOutImage.color = new Color(1, 1, 1, 1 - t); // Fade out
            fadeInImage.color = new Color(1, 1, 1, t); // Fade in

            yield return null;
        }

        fadeOutImage.color = new Color(1, 1, 1, 0); // Ensure fade-out is complete
        fadeInImage.color = Color.white; // Ensure fade-in is complete

        _usingImageA = !_usingImageA; // Swap roles
    }

    private Sprite GetNextSprite()
    {
        if (_availableIndices.Count == 0)
        {
            if (loop) InitializeIndices();
            else return null;
        }

        int index = _availableIndices[0];
        _availableIndices.RemoveAt(0);
        return sprites[index];
    }
}
