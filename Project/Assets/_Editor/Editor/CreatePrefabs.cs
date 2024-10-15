using AkaitoAi.Advertisement;
using UnityEditor;
using UnityEngine;

namespace AkaitoAi
{
    public class CreatePrefabs : Editor
    {
        [MenuItem("AkaitoAi/Setup/Tweaks")]
        public static void CreateTweaks()
        {
            // Check if already exists in the scene
            if (GameObject.FindObjectOfType<Tweaks>())
            {
                EditorUtility.DisplayDialog("Scene has Tweaks already!", "Scene has Tweaks already!", "Close");
                Selection.activeGameObject = GameObject.FindObjectOfType<Tweaks>().gameObject;
            }
            else
            {
                // Ensure the path includes ".prefab"
                string prefabPath = "Assets/_Editor/Prefabs/Tweaks.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                // Check if the prefab is found
                if (prefab != null)
                {
                    // Instantiate the prefab in the scene
                    GameObject tweaks = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    tweaks.transform.position = Vector3.zero;
                    tweaks.transform.rotation = Quaternion.identity;
                    tweaks.name = "Tweaks";
                    Selection.activeGameObject = tweaks;
                }
                else
                {
                    // Log an error if the prefab is not found
                    Debug.LogError("Prefab not found at path: " + prefabPath);
                }
            }
        }

        [MenuItem("AkaitoAi/Setup/Profiler")]
        public static void CreateProfiler()
        {
            // Check if already exists in the scene
            if (GameObject.FindObjectOfType<ProfileReader>())
            {
                EditorUtility.DisplayDialog("Scene has Profiler already!", "Scene has Profiler already!", "Close");
                Selection.activeGameObject = GameObject.FindObjectOfType<ProfileReader>().gameObject;
            }
            else
            {
                // Ensure the path includes ".prefab"
                string prefabPath = "Assets/_Editor/Prefabs/Profiler.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                // Check if the prefab is found
                if (prefab != null)
                {
                    // Instantiate the prefab in the scene
                    GameObject profiler = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    profiler.transform.position = Vector3.zero;
                    profiler.transform.rotation = Quaternion.identity;
                    profiler.name = "Profiler";
                    Selection.activeGameObject = profiler;
                }
                else
                {
                    // Log an error if the prefab is not found
                    Debug.LogError("Prefab not found at path: " + prefabPath);
                }
            }
        }

        [MenuItem("AkaitoAi/Misc/UpdateManager")]
        public static void CreateUpdateManager()
        {
            // Check if already exists in the scene
            if (GameObject.FindObjectOfType<UpdateManager>())
            {
                EditorUtility.DisplayDialog("Scene has UpdateManager already!", "Scene has UpdateManager already!", "Close");
                Selection.activeGameObject = GameObject.FindObjectOfType<UpdateManager>().gameObject;
            }
            else
            {
                // Ensure the path includes ".prefab"
                string prefabPath = "Assets/_Editor/Prefabs/UpdateManager.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                // Check if the prefab is found
                if (prefab != null)
                {
                    // Instantiate the prefab in the scene
                    GameObject updateManager = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    updateManager.transform.position = Vector3.zero;
                    updateManager.transform.rotation = Quaternion.identity;
                    updateManager.name = "UpdateManager";
                    Selection.activeGameObject = updateManager;
                }
                else
                {
                    // Log an error if the prefab is not found
                    Debug.LogError("Prefab not found at path: " + prefabPath);
                }
            }
        }

        [MenuItem("AkaitoAi/Misc/TimeOfDay")]
        public static void CreateTimeOfDay()
        {
            // Check if already exists in the scene
            if (GameObject.FindObjectOfType<TimeOfDay>())
            {
                EditorUtility.DisplayDialog("Scene has TimeOfDay already!", "Scene has TimeOfDay already!", "Close");
                Selection.activeGameObject = GameObject.FindObjectOfType<TimeOfDay>().gameObject;
            }
            else
            {
                // Ensure the path includes ".prefab"
                string prefabPath = "Assets/_Editor/Prefabs/TimeOfDay.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                // Check if the prefab is found
                if (prefab != null)
                {
                    // Instantiate the prefab in the scene
                    GameObject timeOfDay = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    timeOfDay.transform.position = Vector3.zero;
                    timeOfDay.transform.rotation = Quaternion.identity;
                    timeOfDay.name = "AdsWrapper";
                    Selection.activeGameObject = timeOfDay;
                }
                else
                {
                    // Log an error if the prefab is not found
                    Debug.LogError("Prefab not found at path: " + prefabPath);
                }
            }
        }

        [MenuItem("AkaitoAi/Setup/AdsWrapper")]
        public static void CreateAdsWrapper()
        {
            // Check if already exists in the scene
            if (GameObject.FindObjectOfType<AdsWrapper>())
            {
                EditorUtility.DisplayDialog("Scene has AdsWrapper already!", "Scene has AdsWrapper already!", "Close");
                Selection.activeGameObject = GameObject.FindObjectOfType<AdsWrapper>().gameObject;
            }
            else
            {
                // Ensure the path includes ".prefab"
                string prefabPath = "Assets/_Editor/Prefabs/AdsWrapper.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                // Check if the prefab is found
                if (prefab != null)
                {
                    // Instantiate the prefab in the scene
                    GameObject adsWrapper = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    adsWrapper.transform.position = Vector3.zero;
                    adsWrapper.transform.rotation = Quaternion.identity;
                    adsWrapper.name = "AdsWrapper";
                    Selection.activeGameObject = adsWrapper;
                }
                else
                {
                    // Log an error if the prefab is not found
                    Debug.LogError("Prefab not found at path: " + prefabPath);
                }
            }
        }

        [MenuItem("AkaitoAi/Misc/InEditorGridGenetator")]
        public static void CreateInEditorGridGenetator()
        {
            // Ensure the path includes ".prefab"
            string prefabPath = "Assets/_Editor/Prefabs/GridGeneratorEditor.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            // Check if the prefab is found
            if (prefab != null)
            {
                // Instantiate the prefab in the scene
                GameObject gridGenerator = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                gridGenerator.transform.position = Vector3.zero;
                gridGenerator.transform.rotation = Quaternion.identity;
                gridGenerator.name = "GridGeneratorEditor";
                Selection.activeGameObject = gridGenerator;
            }
            else
            {
                // Log an error if the prefab is not found
                Debug.LogError("Prefab not found at path: " + prefabPath);
            }
        }

        [MenuItem("AkaitoAi/Misc/RuntimeGridGenetator")]
        public static void CreateRuntimeGridGenetator()
        {
            // Ensure the path includes ".prefab"
            string prefabPath = "Assets/_Editor/Prefabs/GridGenerator.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            // Check if the prefab is found
            if (prefab != null)
            {
                // Instantiate the prefab in the scene
                GameObject gridGenerator = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                gridGenerator.transform.position = Vector3.zero;
                gridGenerator.transform.rotation = Quaternion.identity;
                gridGenerator.name = "GridGenerator";
                Selection.activeGameObject = gridGenerator;
            }
            else
            {
                // Log an error if the prefab is not found
                Debug.LogError("Prefab not found at path: " + prefabPath);
            }
        }

        [MenuItem("AkaitoAi/Misc/TimelineCutScene")]
        public static void CreateTimelineCutScene()
        {
            // Ensure the path includes ".prefab"
            string prefabPath = "Assets/_Editor/Prefabs/TimelineCutscene.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            // Check if the prefab is found
            if (prefab != null)
            {
                // Instantiate the prefab in the scene
                GameObject timelineCutscene = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                timelineCutscene.transform.position = Vector3.zero;
                timelineCutscene.transform.rotation = Quaternion.identity;
                timelineCutscene.name = "TimelineCutscene";
                Selection.activeGameObject = timelineCutscene;
            }
            else
            {
                // Log an error if the prefab is not found
                Debug.LogError("Prefab not found at path: " + prefabPath);
            }
        }

        [MenuItem("AkaitoAi/Misc/RuntimeMeshCombine")]
        public static void CreateRuntimeMeshCombine()
        {
            // Ensure the path includes ".prefab"
            string prefabPath = "Assets/_Editor/Prefabs/RuntimeMeshCombine.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            // Check if the prefab is found
            if (prefab != null)
            {
                // Instantiate the prefab in the scene
                GameObject runtimeMeshCombine = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                runtimeMeshCombine.transform.position = Vector3.zero;
                runtimeMeshCombine.transform.rotation = Quaternion.identity;
                runtimeMeshCombine.name = "RuntimeMeshCombine";
                Selection.activeGameObject = runtimeMeshCombine;
            }
            else
            {
                // Log an error if the prefab is not found
                Debug.LogError("Prefab not found at path: " + prefabPath);
            }
        }

        [MenuItem("AkaitoAi/Misc/LineRenderer Route System")]
        public static void CreateLineRendererRouteSystem()
        {
            // Ensure the path includes ".prefab"
            string prefabPath = "Assets/_Editor/Prefabs/LineRendererRoute.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            // Check if the prefab is found
            if (prefab != null)
            {
                // Instantiate the prefab in the scene
                GameObject lineRendererRoute = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                lineRendererRoute.transform.position = Vector3.zero;
                lineRendererRoute.transform.rotation = Quaternion.identity;
                lineRendererRoute.name = "LineRendererRoute";
                Selection.activeGameObject = lineRendererRoute;
            }
            else
            {
                // Log an error if the prefab is not found
                Debug.LogError("Prefab not found at path: " + prefabPath);
            }
        }
    }
}
