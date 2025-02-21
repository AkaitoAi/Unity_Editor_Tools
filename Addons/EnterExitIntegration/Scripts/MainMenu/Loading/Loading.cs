using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneLoadingType
{
    Static,
    Dynamic
}
public enum StaticLoadingProgression
{
    Speed,
    Duration
}
public class Loading : MonoBehaviour
{
    [Header("Select Loading Type")]
    [SerializeField] private SceneLoadingType loadingType;

    [Header ("Loading Progress")]
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillerImage;
    [SerializeField] private Text progressText;
    [SerializeField] private int sceneIndex;
    private string loadingPercentPostfix = "%";

    [Header("Game Tips")]
    [SerializeField] private string tipPrefix = "Tip: ";
    [SerializeField] private Text tipsText;
    [SerializeField] private string prefsName = "TipShown";
    [SerializeField] private string[] tips;
    internal int switchIndex = 0;
    internal int index;

    [Header("Loading")]
    [SerializeField] private Text loadingText;
    [SerializeField] private string[] loadingString = { "Fetching Files", "Fetching Environment", "Fetching Models", "Fetching Level", "Fetching Vehicles" };

    [Header("Static Loading")]
    [SerializeField] private StaticLoadingProgression staticLoadingProgression;
    [SerializeField] private float simpleLoadingDuration = 5f;
    private float loadingSpeed;
    private float loadingSlowSpeed;
    private float counterTime = 0f;
    private float counterProgress = 0f;
    private float counterStartProgress = 0f;
    private float counterEndProgress = 1f;

    [Header("Dynamic Loading")]
    AsyncOperation asyncOperation;

    private void Awake()
    {
        //Sound Calling
        //SoundManager.Instance.ChangeVolume(1f, 1f);

        TipsSetup();


        //if (Tweaks.Instance != null)
        //    Tweaks.Instance.CollectGarbage();
    }

    private void Start()
    {
        Time.timeScale = 1f;

        loadingSpeed = simpleLoadingDuration;
        //loadingSlowSpeed = simpleLoadingDuration / 2f;

        InvokeRepeating("ShowLoadingText", 1f, 1f);

        switch (loadingType)
        {
            case SceneLoadingType.Static:

                StartCoroutine(StaticSceneLoader()); // Static Loading
                
                break;

            case SceneLoadingType.Dynamic:

                StartCoroutine(DynamicAsyncSceneLoader()); // Dynamic Loading

                break;
        }

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

                if (counterTime >= counterEndProgress) SceneManager.LoadScene(sceneIndex);

                yield return null;
            }
        }

        IEnumerator DynamicAsyncSceneLoader()
        {
            //Begin to load the Scene you specify
            asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);

            //Don't let the Scene activate until you allow it to
            asyncOperation.allowSceneActivation = false;

            while (!asyncOperation.isDone)
            {
                //progressText.text = (asyncOperation.progress * 100) + loadingPercentPrefix;

                float progressValue = Mathf.Clamp01(asyncOperation.progress / 0.9f);

                if (progressText != null) progressText.text = Mathf.Round(progressValue * 100).ToString() + loadingPercentPostfix; // Converts 0 to 1 into 0 to 100

                if (slider != null) slider.value += asyncOperation.progress; // 0 to 1
                if (fillerImage != null) fillerImage.fillAmount += asyncOperation.progress; // 0 to 1
                                                                                            //if (progressText != null) progressText.text = Mathf.Round(asyncOperation.progress * 100).ToString() + loadingPercentPrefix;

                // Check if the load has finished
                if (asyncOperation.progress >= 0.9f) asyncOperation.allowSceneActivation = true;

                yield return null;
            }
        }
    }
   

    private void ShowLoadingText()
    {
        if (loadingText == null)
        {
            CancelInvoke("ShowLoadingText");

            return;
        }

        ShowRepeatingText(loadingString, loadingText);
    }

    private void TipsSetup()
    {
        if (tipsText == null) return;
        
        //! Shows's next tip, everytime it is called 
        ShowRepeatingText(tips, tipsText);
    }

    private void ShowRepeatingText(string[] _string, Text _text)
    {
        if(tipsText != null) switchIndex = PlayerPrefs.GetInt(prefsName, switchIndex);

        for (index = 0; index < _string.Length; index++)
            if (index == switchIndex)
                _text.text = _text == tipsText ? tipPrefix + _string[index].ToString() : _string[index].ToString();

        if (switchIndex < _string.Length - 1) switchIndex++;
        else switchIndex = 0;

        if (tipsText != null) PlayerPrefs.SetInt(prefsName, switchIndex);
    }
}
