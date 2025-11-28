using Gadsme;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GadsmeInit : MonoBehaviour
{
    public Camera mainCamera;

    [Header("Assign all Gadsme ad objects")]
    public List<GameObject> gadsmeAds = new List<GameObject>();

    [HideInInspector] public bool assignInactivePlacements = false;

    private const int DEFAULT_LAYER = 0;
    private const int IGNORE_LAYER = 2;

    private readonly List<int> originalLayers = new List<int>();
    private readonly List<int> currentLayers = new List<int>();

    private static readonly List<RaycastResult> uiHits = new List<RaycastResult>(10);

    void Awake()
    {
        if (gadsmeAds.Count == 0) return;

        int count = gadsmeAds.Count;
        originalLayers.Capacity = count;
        currentLayers.Capacity = count;

        for (int i = 0; i < count; i++)
        {
            GameObject ad = gadsmeAds[i];
            int layer = ad != null ? ad.layer : DEFAULT_LAYER;

            originalLayers.Add(layer);
            currentLayers.Add(layer);
        }
    }

    void Start()
    {
        GadsmeSDK.SetUserAge(21);
        GadsmeSDK.SetUserGender(Gender.MALE);
        GadsmeSDK.SetMainCamera(mainCamera);
        GadsmeSDK.Init();
    }

    void OnGamePhaseChange(Camera newCamera)
    {
        GadsmeSDK.SetMainCamera(newCamera);
    }

    void Update()
    {
        if (gadsmeAds.Count == 0) return;

        if (!(Input.GetMouseButtonDown(0) ||
             (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
            return;

        if (PointerOverNonDefaultUI())
            SetAdsToIgnore();
        else
            RestoreAds();
    }

    [System.NonSerialized] private bool isIgnored = false;

    void SetAdsToIgnore()
    {
        if (isIgnored) return;
        isIgnored = true;

        for (int i = 0; i < gadsmeAds.Count; i++)
        {
            GameObject ad = gadsmeAds[i];
            if (ad == null) continue;
            ad.layer = IGNORE_LAYER;
            currentLayers[i] = IGNORE_LAYER;
        }
    }

    void RestoreAds()
    {
        if (!isIgnored) return;
        isIgnored = false;

        for (int i = 0; i < gadsmeAds.Count; i++)
        {
            GameObject ad = gadsmeAds[i];
            if (ad == null) continue;
            int orig = originalLayers[i];
            ad.layer = orig;
            currentLayers[i] = orig;
        }
    }

    bool PointerOverNonDefaultUI()
    {
        if (EventSystem.current == null) return false;

        Vector2 pos = Input.mousePosition;

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
            pos = Input.GetTouch(0).position;
#endif

        var ped = new PointerEventData(EventSystem.current) { position = pos };

        uiHits.Clear();
        EventSystem.current.RaycastAll(ped, uiHits);

        int count = uiHits.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject ui = uiHits[i].gameObject;

            if (!ui.activeInHierarchy) continue;

            Graphic g = ui.GetComponent<Graphic>();
            if (g == null || !g.raycastTarget) continue;

            CanvasGroup cg = ui.GetComponentInParent<CanvasGroup>();
            if (cg != null && (!cg.blocksRaycasts || !cg.interactable))
                continue;

            if (ui.layer == DEFAULT_LAYER)
                return false;

            return true;
        }
        return false;
    }

    public struct OnGadsmeLayer : IEvent { public bool ignore; }

#if UNITY_EDITOR
    // =======================================================
    //                    EDITOR SECTION
    // =======================================================
    [CustomEditor(typeof(GadsmeInit))]
    public class GadsmeInitEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GadsmeInit script = (GadsmeInit)target;

            EditorGUILayout.Space(10);

            script.assignInactivePlacements = EditorGUILayout.Toggle(
                "Assign Inactive Placements",
                script.assignInactivePlacements
            );

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Assign Gadsme"))
                AssignGadsme(script);

            if (script.gadsmeAds.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No Gadsme Placement Banner objects found in the scene!",
                    MessageType.Warning
                );
            }
        }

        private static void AssignGadsme(GadsmeInit script, bool auto = false)
        {
            if (script == null) return;

            script.gadsmeAds.Clear();

            var all = Resources.FindObjectsOfTypeAll<GadsmePlacementBanner>();

            foreach (var placement in all)
            {
                if (placement == null) continue;

                bool active = placement.gameObject.activeInHierarchy;
                bool enabled = placement.enabled;

                if (!script.assignInactivePlacements)
                {
                    if (!active || !enabled)
                        continue;
                }

                script.gadsmeAds.Add(placement.gameObject);
            }

            script.gadsmeAds.Sort((a, b) => a.name.CompareTo(b.name));

            EditorUtility.SetDirty(script);

            if (!auto)
                Debug.Log($"Assigned {script.gadsmeAds.Count} Gadsme placements.");
        }
    }
#endif
}
