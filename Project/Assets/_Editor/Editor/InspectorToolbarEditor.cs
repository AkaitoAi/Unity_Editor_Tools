using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.IO;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

using static System.Environment;
using static System.IO.Path;
using static UnityEditor.AssetDatabase;
using static System.IO.Directory;
using static UnityEngine.Application;
public class InspectorToolbarEditor : Editor
{
    [InitializeOnLoad]
    public class RecentSceneToolbarGUI
    {
        static RecentSceneToolbarGUI()
        {
            EditorSceneManager.activeSceneChangedInEditMode += EditorSceneManager_activeSceneChangedInEditMode;
            ToolbarExtender.RightToolbarGUI.Add(CopyBtn);
            ToolbarExtender.RightToolbarGUI.Add(PasteBtn);
            ToolbarExtender.RightToolbarGUI.Add(ShortcutBtn);
        }

        private static void EditorSceneManager_activeSceneChangedInEditMode(Scene current, Scene next) { /*Handle scene changes if necessary*/  }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnReloadScripts() { /* Handle script reload if necessary*/ }

        internal static void ShortcutBtn()
        {
            GUILayout.Space(0.5f);

            string title = " Inspector Shortcuts ▼ ";
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.alignment = TextAnchor.MiddleCenter;
            style.stretchWidth = false;

            if (GUILayout.Button(new GUIContent(title, "Essentials shortCuts"), style))
            {
                ShortcutList();
            }
        }
        private static void ShortcutList()
        {
            GenericMenu menuShortCut = new GenericMenu();

            menuShortCut.AddItem(new GUIContent("Create Model Folder"), false, () => CreateModelFolders());
            //menuShortCut.AddItem(new GUIContent("Package Manager"), false, () => EditorApplication.ExecuteMenuItem("Window/Package Manager"));
            menuShortCut.AddItem(new GUIContent("Create Project Setup Folders"), false, () => CreateProjectSetupFolders());
            menuShortCut.AddItem(new GUIContent("Open C# Project"), false, () => EditorApplication.ExecuteMenuItem("Assets/Open C# Project"));
            menuShortCut.AddItem(new GUIContent("Set All Materials GUPInst True"), false, () => GetAndSetAllMaterialsGUPInstTrue());
            menuShortCut.AddItem(new GUIContent("Set All Materials GUPInst False"), false, () => GetAndSetAllMaterialsGUPInstFalse());

            menuShortCut.ShowAsContext();
        }

        #region Copy Paste Transform
        internal static void CopyBtn()
        {
            GUILayout.Space(0.5f);

            string title = " Copy Transform";
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.alignment = TextAnchor.MiddleCenter;
            style.stretchWidth = false;

            if (GUILayout.Button(new GUIContent(title, "Copy Transform Of Selected Object"), style))
            {
                CopyTransform();
            }
        }
        internal static void PasteBtn()
        {
            GUILayout.Space(0.5f);

            string title = " Paste Transform";
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.alignment = TextAnchor.MiddleCenter;
            style.stretchWidth = false;

            if (GUILayout.Button(new GUIContent(title, "Paste Transform to Selected Object"), style))
            {
                PasteTransform();
            }
        }

        private static Transform copiedTransform;
        private static bool useLocalPosition;

        [MenuItem("Nauman/Tools/Copy Paste/Copy Transform")]
        public static void CopyTransform()
        {
            if (Selection.activeGameObject != null)
            {
                copiedTransform = Selection.activeGameObject.transform;
                useLocalPosition = Tools.pivotRotation == PivotRotation.Local;
                Debug.Log($"Transform copied from: {copiedTransform.name} (Using {(useLocalPosition ? "Local" : "Global")} position)");
            }
            else
            {
                Debug.LogWarning("No GameObject selected to copy transform from.");
            }
        }

        [MenuItem("Nauman/Tools/Copy Paste/Paste Transform")]
        public static void PasteTransform()
        {
            if (copiedTransform == null)
            {
                Debug.LogWarning("No transform copied to paste.");
                return;
            }

            if (Selection.gameObjects.Length > 0)
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    Transform targetTransform = go.transform;
                    Undo.RecordObject(targetTransform, "Paste Transform");

                    if (useLocalPosition)
                    {
                        targetTransform.localPosition = copiedTransform.localPosition;
                        targetTransform.localRotation = copiedTransform.localRotation;
                        targetTransform.localScale = copiedTransform.localScale;
                    }
                    else
                    {
                        targetTransform.position = copiedTransform.position;
                        targetTransform.rotation = copiedTransform.rotation;
                        targetTransform.localScale = copiedTransform.localScale;
                    }

                    Debug.Log($"Transform pasted to: {targetTransform.name} (Using {(useLocalPosition ? "Local" : "Global")} position)");
                }
            }
            else
            {
                Debug.LogWarning("No GameObjects selected to paste transform to.");
            }
        }
        #endregion

        #region Enable/ Disable GPU Instancing


        //[MenuItem("Nauman/Tools/Materials/Get all Metrials GPU Instancing True")]// % mean command Key and # mean shift Key....
        static void GetAndSetAllMaterialsGUPInstTrue()
        {
            string[] paths = AssetDatabase.FindAssets("t:Material");
            Debug.Log(paths.Length);
            Material[] mat = new Material[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                mat[i] = (Material)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(paths[i]), typeof(Material));
                mat[i].enableInstancing = true;
            }
            Debug.Log(mat[mat.Length - 1].name);
            Debug.Log(mat[0].name);
        }

        //[MenuItem("Nauman/Tools/Materials/Get all Metrials GPU Instancing False")]// % mean command Key and # mean shift Key....
        static void GetAndSetAllMaterialsGUPInstFalse()
        {
            string[] paths = AssetDatabase.FindAssets("t:Material");
            Debug.Log(paths.Length);
            Material[] mat = new Material[paths.Length];

            for (int i = 0; i < paths.Length; i++)
            {
                mat[i] = (Material)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(paths[i]), typeof(Material));
                mat[i].enableInstancing = false;
            }
            Debug.Log(mat[mat.Length - 1].name);
            Debug.Log(mat[0].name);
        }
        #endregion

        #region Project Setup
        public static string rootPath => $"_Project({Application.productName})";

        public static void CreateProjectSetupFolders()
        {
            Folders.Create($"{rootPath}", "Animations/Clip", "Animations/Controller", "Packages", "Materials", "Audios/Music", "Audios/SFX", "Cutscene/Playable", "Cutscene/Signals", "3D Assets", "Prefabs", "Particles", "UI", "Scriptables", "Shaders", "Textures 2D", "Scripts", "Scripts/Editor");
            Refresh();
            Folders.Move($"{rootPath}", "Scenes");
            Folders.Move($"{rootPath}", "Settings");
            Refresh();
        }
        public static void CreateModelFolders()
        {
            Folders.Create($"{rootPath}", "3D Assets/Model Folder/FBX", "3D Assets/Model Folder/Materials", "3D Assets/Model Folder/Textures");
            Refresh();
        }

        //[MenuItem("Nauman/Project Setup/Import/ Essential Assets", priority = -1)]
        //public static void ImportEssentials()
        //{
        //    Assets.ImportAsset("Odin Inspector and Serializer.unitypackage", "Sirenix/Editor ExtensionsSystem");
        //    Assets.ImportAsset("Odin Validator.unitypackage", "Sirenix/Editor ExtensionsUtilities");
        //    Assets.ImportAsset("Editor Console Pro.unitypackage", "FlyingWorm/Editor ExtensionsSystem");
        //}

        //[MenuItem("Nauman/Project Setup/Import/Install Essential Packages", priority = -1)]
        //public static void InstallPackages()
        //{
        //    Packages.InstallPackages(new[] {
        //    "com.unity.2d.animation",
        //    "git+https://github.com/adammyhre/Unity-Utils.git",
        //    "git+https://github.com/adammyhre/Unity-Improved-Timers.git",
        //    "git+https://github.com/KyleBanks/scene-ref-attribute.git"
        //});
        //}

        static class Folders
        {
            public static void Create(string root, params string[] folders)
            {
                var fullpath = Combine(Application.dataPath, root);
                if (!Directory.Exists(fullpath))
                {
                    Directory.CreateDirectory(fullpath);
                }

                foreach (var folder in folders)
                {
                    CreateSubFolders(fullpath, folder);
                }
            }

            static void CreateSubFolders(string rootPath, string folderHierarchy)
            {
                var folders = folderHierarchy.Split('/');
                var currentPath = rootPath;

                foreach (var folder in folders)
                {
                    currentPath = Combine(currentPath, folder);
                    if (!Directory.Exists(currentPath))
                    {
                        Directory.CreateDirectory(currentPath);
                    }
                }
            }

            public static void Move(string newParent, string folderName)
            {
                var sourcePath = $"Assets/{folderName}";
                if (IsValidFolder(sourcePath))
                {
                    var destinationPath = $"Assets/{newParent}/{folderName}";
                    var error = MoveAsset(sourcePath, destinationPath);

                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogError($"Failed to move {folderName}: {error}");
                    }
                }
            }

            public static void Delete(string folderName)
            {
                var pathToDelete = $"Assets/{folderName}";

                if (IsValidFolder(pathToDelete))
                {
                    DeleteAsset(pathToDelete);
                }
            }
        }
        static class Assets
        {
            public static void ImportAsset(string asset, string folder)
            {
                string basePath;
                if (OSVersion.Platform is PlatformID.MacOSX or PlatformID.Unix)
                {
                    string homeDirectory = GetFolderPath(SpecialFolder.Personal);
                    basePath = Combine(homeDirectory, "Library/Unity/Asset Store-5.x");
                }
                else
                {
                    string defaultPath = Combine(GetFolderPath(SpecialFolder.ApplicationData), "Unity");
                    basePath = Combine(EditorPrefs.GetString("AssetStoreCacheRootPath", defaultPath), "Asset Store-5.x");
                }

                asset = asset.EndsWith(".unitypackage") ? asset : asset + ".unitypackage";

                string fullPath = Combine(basePath, folder, asset);

                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"The asset package was not found at the path: {fullPath}");
                }

                ImportPackage(fullPath, false);
            }
        }
        static class Packages
        {
            static AddRequest request;
            static Queue<string> packagesToInstall = new Queue<string>();

            public static void InstallPackages(string[] packages)
            {
                foreach (var package in packages)
                {
                    packagesToInstall.Enqueue(package);
                }

                if (packagesToInstall.Count > 0)
                {
                    StartNextPackageInstallation();
                }
            }

            static async void StartNextPackageInstallation()
            {
                request = Client.Add(packagesToInstall.Dequeue());

                while (!request.IsCompleted) await Task.Delay(10);

                if (request.Status == StatusCode.Success) Debug.Log("Installed: " + request.Result.packageId);
                else if (request.Status >= StatusCode.Failure) Debug.LogError(request.Error.message);

                if (packagesToInstall.Count > 0)
                {
                    await Task.Delay(1000);
                    StartNextPackageInstallation();
                }
            }
        }

        #endregion
    }


    #region UnityToolBarExtender

    public static class ToolbarCallback
    {
        static Type m_toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        static Type m_guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
#if UNITY_2020_1_OR_NEWER
        static Type m_iWindowBackendType = typeof(Editor).Assembly.GetType("UnityEditor.IWindowBackend");
        static PropertyInfo m_windowBackend = m_guiViewType.GetProperty("windowBackend",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static PropertyInfo m_viewVisualTree = m_iWindowBackendType.GetProperty("visualTree",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#else
        static PropertyInfo m_viewVisualTree = m_guiViewType.GetProperty("visualTree",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#endif
        static FieldInfo m_imguiContainerOnGui = typeof(IMGUIContainer).GetField("m_OnGUIHandler",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static ScriptableObject m_currentToolbar;

        /// <summary>
        /// Callback for toolbar OnGUI method.
        /// </summary>
        public static Action OnToolbarGUI;
        public static Action OnToolbarGUILeft;
        public static Action OnToolbarGUIRight;

        static ToolbarCallback()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate()
        {
            // Relying on the fact that toolbar is ScriptableObject and gets deleted when layout changes
            if (m_currentToolbar == null)
            {
                // Find toolbar
                var toolbars = Resources.FindObjectsOfTypeAll(m_toolbarType);
                m_currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                if (m_currentToolbar != null)
                {
#if UNITY_2021_1_OR_NEWER
                    var root = m_currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                    var rawRoot = root.GetValue(m_currentToolbar);
                    var mRoot = rawRoot as VisualElement;
                    RegisterCallback("ToolbarZoneLeftAlign", OnToolbarGUILeft);
                    RegisterCallback("ToolbarZoneRightAlign", OnToolbarGUIRight);

                    void RegisterCallback(string root, Action cb)
                    {
                        var toolbarZone = mRoot.Q(root);

                        var parent = new VisualElement()
                        {
                            style = {
                                flexGrow = 1,
                                flexDirection = FlexDirection.Row,
                            }
                        };
                        var container = new IMGUIContainer();
                        container.style.flexGrow = 1;
                        container.onGUIHandler += () =>
                        {
                            cb?.Invoke();
                        };
                        parent.Add(container);
                        toolbarZone.Add(parent);
                    }
#else
#if UNITY_2020_1_OR_NEWER
                    var windowBackend = m_windowBackend.GetValue(m_currentToolbar);

                    // Get it's visual tree
                    var visualTree = (VisualElement)m_viewVisualTree.GetValue(windowBackend, null);
#else
                    // Get it's visual tree
                    var visualTree = (VisualElement)m_viewVisualTree.GetValue(m_currentToolbar, null);
#endif

                    // Get first child which 'happens' to be toolbar IMGUIContainer
                    var container = (IMGUIContainer)visualTree[0];

                    // (Re)attach handler
                    var handler = (Action)m_imguiContainerOnGui.GetValue(container);
                    handler -= OnGUI;
                    handler += OnGUI;
                    m_imguiContainerOnGui.SetValue(container, handler);

#endif
                }
            }
        }

        static void OnGUI()
        {
            var handler = OnToolbarGUI;
            if (handler != null) handler();
        }
    }

    [InitializeOnLoad]
    public static class ToolbarExtender
    {
        static int m_toolCount;
        static GUIStyle m_commandStyle = null;

        public static readonly List<Action> LeftToolbarGUI = new List<Action>();
        public static readonly List<Action> RightToolbarGUI = new List<Action>();

        static ToolbarExtender()
        {
            Type toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");

#if UNITY_2019_1_OR_NEWER
            string fieldName = "k_ToolCount";
#else
            string fieldName = "s_ShownToolIcons";
#endif

            FieldInfo toolIcons = toolbarType.GetField(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

#if UNITY_2019_3_OR_NEWER
            m_toolCount = toolIcons != null ? ((int)toolIcons.GetValue(null)) : 8;
#elif UNITY_2019_1_OR_NEWER
			m_toolCount = toolIcons != null ? ((int) toolIcons.GetValue(null)) : 7;
#elif UNITY_2018_1_OR_NEWER
			m_toolCount = toolIcons != null ? ((Array) toolIcons.GetValue(null)).Length : 6;
#else
            m_toolCount = toolIcons != null ? ((Array)toolIcons.GetValue(null)).Length : 5;
#endif

            ToolbarCallback.OnToolbarGUI = OnGUI;
            ToolbarCallback.OnToolbarGUILeft = GUILeft;
            ToolbarCallback.OnToolbarGUIRight = GUIRight;
        }

#if UNITY_2019_3_OR_NEWER
        public const float space = 8;
#else
        public const float space = 10;
#endif
        public const float largeSpace = 20;
        public const float buttonWidth = 32;
        public const float dropdownWidth = 80;
#if UNITY_2019_1_OR_NEWER
        public const float playPauseStopWidth = 140;
#else
        public const float playPauseStopWidth = 100;
#endif

        static void OnGUI()
        {
            if (m_commandStyle == null)
            {
                m_commandStyle = new GUIStyle("CommandLeft");
            }
            var screenWidth = EditorGUIUtility.currentViewWidth;

            // Following calculations match code reflected from Toolbar.OldOnGUI()
            float playButtonsPosition = Mathf.RoundToInt((screenWidth - playPauseStopWidth) / 2);

            Rect leftRect = new Rect(0, 0, screenWidth, Screen.height);
            leftRect.xMin += space; // Spacing left
            leftRect.xMin += buttonWidth * m_toolCount; // Tool buttons
#if UNITY_2019_3_OR_NEWER
            leftRect.xMin += space; // Spacing between tools and pivot
#else
            leftRect.xMin += largeSpace; // Spacing between tools and pivot
#endif
            leftRect.xMin += 64 * 2; // Pivot buttons
            leftRect.xMax = playButtonsPosition;

            Rect rightRect = new Rect(0, 0, screenWidth, Screen.height);
            rightRect.xMin = playButtonsPosition;
            rightRect.xMin += m_commandStyle.fixedWidth * 3; // Play buttons
            rightRect.xMax = screenWidth;
            rightRect.xMax -= space; // Spacing right
            rightRect.xMax -= dropdownWidth; // Layout
            rightRect.xMax -= space; // Spacing between layout and layers
            rightRect.xMax -= dropdownWidth; // Layers
#if UNITY_2019_3_OR_NEWER
            rightRect.xMax -= space; // Spacing between layers and account
#else
            rightRect.xMax -= largeSpace; // Spacing between layers and account
#endif
            rightRect.xMax -= dropdownWidth; // Account
            rightRect.xMax -= space; // Spacing between account and cloud
            rightRect.xMax -= buttonWidth; // Cloud
            rightRect.xMax -= space; // Spacing between cloud and collab
            rightRect.xMax -= 78; // Colab

            // Add spacing around existing controls
            leftRect.xMin += space;
            leftRect.xMax -= space;
            rightRect.xMin += space;
            rightRect.xMax -= space;

            // Add top and bottom margins
#if UNITY_2019_3_OR_NEWER
            leftRect.y = 4;
            leftRect.height = 22;
            rightRect.y = 4;
            rightRect.height = 22;
#else
            leftRect.y = 5;
            leftRect.height = 24;
            rightRect.y = 5;
            rightRect.height = 24;
#endif

            if (leftRect.width > 0)
            {
                GUILayout.BeginArea(leftRect);
                GUILayout.BeginHorizontal();
                foreach (var handler in LeftToolbarGUI)
                {
                    handler();
                }

                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            if (rightRect.width > 0)
            {
                GUILayout.BeginArea(rightRect);
                GUILayout.BeginHorizontal();
                foreach (var handler in RightToolbarGUI)
                {
                    handler();
                }

                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }

        public static void GUILeft()
        {
            GUILayout.BeginHorizontal();
            foreach (var handler in LeftToolbarGUI)
            {
                handler();
            }
            GUILayout.EndHorizontal();
        }

        public static void GUIRight()
        {
            GUILayout.BeginHorizontal();
            foreach (var handler in RightToolbarGUI)
            {
                handler();
            }
            GUILayout.EndHorizontal();
        }
    }
    #endregion
}
#endif