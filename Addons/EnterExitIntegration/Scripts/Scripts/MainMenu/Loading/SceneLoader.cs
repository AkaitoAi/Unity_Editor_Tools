using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private int sceneIndex;
    [SerializeField] private bool unloadAssets = false;

    private void OnLoad(AnimationEvent animationEvent)
    {
        if (animationEvent.animationState.weight > 0.5f)
            SceneManager.LoadScene(sceneIndex);

        if (!unloadAssets) return;

        Resources.UnloadUnusedAssets();
    }
}
