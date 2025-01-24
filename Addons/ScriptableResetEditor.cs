using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ScriptableObject), true)]
public class ScriptableResetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Check if the target ScriptableObject implements IResettable
        if (target is IResettable resettable)
        {
            // Add a "Reset to Default Values" button
            if (GUILayout.Button("Reset to Default Values"))
            {
                resettable.ResetToDefaults();
                EditorUtility.SetDirty(target); // Mark the object as dirty for changes to persist
                Debug.Log($"{target.name} reset to default values.");
            }
        }
    }
}
#endif