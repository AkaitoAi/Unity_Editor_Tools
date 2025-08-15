using UnityEngine;
using DG.Tweening;
using AkaitoAi;

public class MaterialOffsetAnimator : MonoBehaviour
{
    [GetComponent] [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Vector2 offsetPerLoop = new Vector2(1f, 0f);
    [SerializeField] private float duration = 1f;
    [SerializeField] private string propertyName = "_MainTex";

    private Material mat;
    private Vector2 currentOffset;

    private void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        mat = targetRenderer.material;
        currentOffset = mat.GetTextureOffset(propertyName);

        Animate();
    }

    private void Animate()
    {
        DOTween.To(() => currentOffset, x =>
        {
            currentOffset = x;
            mat.SetTextureOffset(propertyName, currentOffset);
        },
        currentOffset + offsetPerLoop, duration)
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Incremental);
    }
}
