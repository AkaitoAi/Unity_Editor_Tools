using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum DemoLoadingProgression
{
    Speed,
    Duration
}
public class DemoLoading : MonoBehaviour
{
    [SerializeField] private bool loadNextScene = false;

    [Header("Loading Progress")]
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillerImage;
    [SerializeField] private Text progressText;
    private string loadingPercentPostfix = "%";

    [Header("Static Loading")]
    [SerializeField] private StaticLoadingProgression staticLoadingProgression;
    [SerializeField] private float simpleLoadingDuration = 5f;
    private float loadingSpeed;
    private float counterTime = 0f;
    private float counterProgress = 0f;
    private float counterStartProgress = 0f;
    private float counterEndProgress = 1f;

    private void Start()
    {
        Time.timeScale = 1f;

        loadingSpeed = simpleLoadingDuration;
        //loadingSlowSpeed = simpleLoadingDuration / 2f;

        StartCoroutine(StaticSceneLoader());

        IEnumerator StaticSceneLoader()
        {
            while (counterTime <= counterEndProgress)
            {
                switch (staticLoadingProgression)
                {
                    case StaticLoadingProgression.Duration:
                        counterTime += Time.deltaTime / simpleLoadingDuration;
                        counterProgress = Mathf.Lerp(counterStartProgress, counterEndProgress, Mathf.SmoothStep(0f, 1f, counterTime));
                        break;

                    case StaticLoadingProgression.Speed:
                        counterTime += Time.deltaTime * simpleLoadingDuration;
                        counterProgress = Mathf.Lerp(counterStartProgress, counterEndProgress, counterTime);

                        if ((counterTime <= .1f || counterTime > .9f) && simpleLoadingDuration > .1f) simpleLoadingDuration = simpleLoadingDuration / 2f;
                        else if (counterTime > .1f && counterTime <= .9f) simpleLoadingDuration = loadingSpeed;

                        break;
                }

                if (progressText != null) progressText.text = Mathf.RoundToInt(counterProgress * 100).ToString() + loadingPercentPostfix;
                if (slider != null) slider.value = counterTime;
                if (fillerImage != null) fillerImage.fillAmount = counterTime;

                yield return null;

                if (loadNextScene && (counterTime >= counterEndProgress)) 
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }
}
