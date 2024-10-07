using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace AkaitoAi
{
    public class SceneSwitcherEditor : EditorWindow
    {
        private string[] sceneNames;
        private int selectedSceneIndex = 0;
        private bool allowSceneSwitching = true; // Flag to control scene switching

        [MenuItem("AkaitoAi/Scene Switcher")]
        public static void ShowWindow()
        {
            GetWindow(typeof(SceneSwitcherEditor), false, "Scene Switcher");
        }

        private void OnEnable()
        {
            RefreshSceneNames();
            EditorBuildSettings.sceneListChanged += RefreshSceneNames;
        }

        private void OnDisable()
        {
            EditorBuildSettings.sceneListChanged -= RefreshSceneNames;
        }

        private void RefreshSceneNames()
        {
            // Get scenes from build settings
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            sceneNames = new string[scenes.Length];

            for (int i = 0; i < scenes.Length; i++)
            {
                sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
            }

            Repaint(); // Force the window to repaint and show the updated list
        }

        private void OnGUI()
        {
            GUILayout.Label("Scene Switcher", EditorStyles.boldLabel);

            if (sceneNames == null || sceneNames.Length == 0)
            {
                RefreshSceneNames();
            }

            // Play From First Scene button
            if (GUILayout.Button("Play From First Scene"))
            {
                PlayFromFirstScene();
            }

            // Display the dropdown for scene selection
            selectedSceneIndex = EditorGUILayout.Popup("Select Scene", selectedSceneIndex, sceneNames);

            // Switch to the selected scene immediately when a new scene is selected (if allowed)
            if (allowSceneSwitching)
            {
                SwitchToSelectedScene();
            }

            // Buttons for each scene in the dropdown
            for (int i = 0; i < sceneNames.Length; i++)
            {
                if (GUILayout.Button($"Switch to {sceneNames[i]}"))
                {
                    SwitchToSceneByIndex(i);
                    selectedSceneIndex = i; // Update the selectedSceneIndex when a button is pressed
                    RefreshSceneNames(); // Refresh the dropdown after switching
                }
            }
        }

        private void SwitchToSelectedScene()
        {
            if (selectedSceneIndex >= 0 && selectedSceneIndex < sceneNames.Length)
            {
                string selectedScenePath = EditorBuildSettings.scenes[selectedSceneIndex].path;

                // Check if the selected scene is different from the current active scene
                if (SceneManager.GetActiveScene().path != selectedScenePath)
                {
                    EditorSceneManager.OpenScene(selectedScenePath);
                }
            }
        }

        private void PlayFromFirstScene()
        {
            // Save current scene
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            // Open the first scene in the build settings
            EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(0));

            // Start playing the scene
            EditorApplication.isPlaying = true;

            // Disable scene switching when playing
            allowSceneSwitching = false;
        }

        private void SwitchToSceneByIndex(int index)
        {
            if (index >= 0 && index < sceneNames.Length)
            {
                string scenePath = EditorBuildSettings.scenes[index].path;

                // Check if the selected scene is different from the current active scene
                if (SceneManager.GetActiveScene().path != scenePath)
                {
                    EditorSceneManager.OpenScene(scenePath);
                }
            }
        }
    }
}
