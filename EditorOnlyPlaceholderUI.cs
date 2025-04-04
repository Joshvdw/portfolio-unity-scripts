using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode] // Ensure the script runs in the editor
public class EditorOnlyPlaceholderUI : MonoBehaviour
{
    private void Awake()
    {
        // Ensure this runs only in editor mode
        #if UNITY_EDITOR
        EnablePlaceholderUI();
        #else
        DisablePlaceholderUI();
        #endif
    }

    private void OnValidate()
    {
        // Handle changes in the editor
        #if UNITY_EDITOR
        EnablePlaceholderUI();
        #endif
    }

    private void EnablePlaceholderUI()
    {
        // Enable the Canvas for editor mode
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.enabled = true;
        }
    }

    private void DisablePlaceholderUI()
    {
        // Disable the Canvas for builds
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.enabled = false;
        }
    }

    #if UNITY_EDITOR
    // Custom Inspector to ensure the PlaceholderUI is not accidentally enabled in builds
    [CustomEditor(typeof(EditorOnlyPlaceholderUI))]
    public class EditorOnlyPlaceholderUIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.HelpBox("This UI element is for Editor use only and will not be included in the build.", MessageType.Info);
        }
    }
    #endif
}
