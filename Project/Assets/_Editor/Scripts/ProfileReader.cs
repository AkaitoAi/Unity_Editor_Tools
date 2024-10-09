using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_5
using UnityEngine.Profiling;
#endif

namespace AkaitoAi
{
    //-----------------------------------------------------------------------------------------------------
    public class ProfileReader : MonoBehaviour
    {
        StringBuilder SB;
        public Text txt;
        float updateInterval = 1.0f;
        float lastInterval;                     // Last interval end time
        float frames = 0;                       // Frames over current interval
        float framesavtick = 0;
        float framesav = 0.0f;
        public static ProfileReader profileReader;

        // Use this for initialization
        void Start()
        {
            if (profileReader == null)
            {
                profileReader = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }

            lastInterval = Time.realtimeSinceStartup;
            frames = 0;
            framesav = 0;
            SB = new StringBuilder();
            SB.Capacity = 200;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

        }

        void OnDisable()
        {
            if (txt)
                DestroyImmediate(txt.gameObject);
        }

        // Update is called once per frame
        void Update()
        {
            ++frames;
            var timeNow = Time.realtimeSinceStartup;

            if (timeNow > lastInterval + updateInterval)
            {
                if (!txt)
                {
                    GameObject go = new GameObject("FPS Display", typeof(Text));
                    go.hideFlags = HideFlags.HideAndDontSave;
                    go.transform.position = new Vector3(0, 0, 0);
                    txt = go.GetComponent<Text>();
                }

                float fps = frames / (timeNow - lastInterval);
                float ms = 1000.0f / Mathf.Max(fps, 0.00001f);
                ++framesavtick;
                framesav += fps;
                float fpsav = framesav / framesavtick;
                SB.Length = 0;

                SB.AppendFormat("Time : {0} ms \nCurrent FPS: {1}     AvgFPS: {2}\n\nCPU : {3} \nCPU Count : {4}\nCPU Freq : {5}mhz\nQuality : {6}\nOS : {7}\n", ms, fps, fpsav,
                SystemInfo.processorType,
                SystemInfo.processorCount,
                SystemInfo.processorFrequency,
                QualitySettings.GetQualityLevel(),
                SystemInfo.operatingSystem)
                .AppendFormat("\nTotal Memory : {0}mb\nTotalAllocatedMemory : {1}mb\nTotalReservedMemory : {2}mb\nTotalUnusedReservedMemory : {3}mb\n\nGPU : {4}\nTotal GPU Memory : {5}mb",
                SystemInfo.systemMemorySize,
                UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1048576,
                UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / 1048576,
                UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong() / 1048576,
                SystemInfo.graphicsDeviceName,
                SystemInfo.graphicsMemorySize
                );
#if UNITY_EDITOR
                SB.AppendFormat("\nDrawCalls : {0}\nUsed Texture Memory : {1}\nRenderedTextureCount : {2}", UnityStats.drawCalls, UnityStats.usedTextureMemorySize / 1048576, UnityStats.usedTextureCount);
#endif

                txt.text = SB.ToString();
                frames = 0;
                lastInterval = timeNow;
            }
        }
    }
}