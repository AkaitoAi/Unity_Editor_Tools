using UnityEngine;
using UnityEngine.SceneManagement;

public class Tweaks : MonoBehaviour
{
    #region Singleton
    private static Tweaks instance;
    public static Tweaks Instance { get { if (instance == null) instance = GameObject.FindObjectOfType<Tweaks>(); return instance; } }

    public int SystemMemorySize { get => systemMemorySize; private set => systemMemorySize = value; }
    #endregion

    [SerializeField] private int lowMemoryDevice = 3072;

    private int systemMemorySize;

    private void Awake()
    {
        systemMemorySize = SystemInfo.systemMemorySize;

        if (Instance != null && Instance != this) { Destroy(this.gameObject); return; }
        else instance = this;
        
        DontDestroyOnLoad(this.gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    // Called everytime scene loads.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetFrameRate();
        DisableLogging();
        DisableVSync();
        DisableScreenTimeout();

        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);

    } // Calls the functions everytime scene loads
    private void DisableVSync()
    {
        if (!Application.isMobilePlatform) return;
        
        QualitySettings.vSyncCount = 0;
    } // Disables VSync on mobile devices
    private void DisableLogging()
    {
        //Disable Logging on the Device...
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
    } // Disables debug calls in build
    private void DisableScreenTimeout() => Screen.sleepTimeout = SleepTimeout.NeverSleep; // Keep device screen awake
    private void SetFrameRate()
    {
        if (systemMemorySize <= lowMemoryDevice)
        {
            Application.targetFrameRate = -1;

            return;
        }
        
        if (systemMemorySize > lowMemoryDevice)
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;

            return;
        }
    } // Sets max fps with regards to device ram
    public void CollectGarbage() => System.GC.Collect(); // Use this to manually collect garage if incremental GC is disabled
    public void SetResolution(float percentage)
    {
        // Get the current screen resolution
        int width = Screen.currentResolution.width;
        int height = Screen.currentResolution.height;

        // Calculate the new resolution based on the percentage
        int newWidth = Mathf.RoundToInt(width * percentage);
        int newHeight = Mathf.RoundToInt(height * percentage);

        // Set the new screen resolution
        Screen.SetResolution(newWidth, newHeight, Screen.fullScreen);

        Debug.Log($"Resolution set to: {newWidth}x{newHeight} ({percentage * 100}%)");
    }
    public void RunTimeStaticBatching(GameObject _parent) => StaticBatchingUtility.Combine(_parent); //Sets static batching on runtime

    //? Add in funtion to know from where the function us being called
    //print((new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name); 

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
