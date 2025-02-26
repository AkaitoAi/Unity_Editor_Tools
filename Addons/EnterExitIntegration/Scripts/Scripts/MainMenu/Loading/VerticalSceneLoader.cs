using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VerticalSceneLoader : MonoBehaviour
{
    [SerializeField] private bool vertical = true;
    [SerializeField] private float loadingTime = 5.0f;
    [SerializeField] private int sceneIndex;
    private IEnumerator Start()
    {
        Time.timeScale = 1.0f;

        if(vertical) Screen.orientation = ScreenOrientation.Portrait;
        else Screen.orientation = ScreenOrientation.LandscapeLeft;

        yield return new WaitForSeconds(loadingTime);

        SceneManager.LoadScene(sceneIndex);
    }
}
