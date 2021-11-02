using UnityEditor;
using UnityEngine;

namespace Nanolod
{
    public static class Preferences
    {
        [PreferenceItem("Nanolod")]
        private static void DrawGUI()
        {
            EditorGUIUtility.labelWidth = 300;

            EditorGUILayout.LabelField("Saving Preferences", EditorStyles.boldLabel);
            SaveMeshesInPrefab = EditorGUILayout.Toggle(new GUIContent(
                "Save LODs meshes in prefab",
                "If enabled, generated meshes will be saved in the prefab when generating LODs in prefab mode. Otherwise, it will be saved at outside of the prefab in the Asset folder."), SaveMeshesInPrefab);
            SaveMeshesInScene = EditorGUILayout.Toggle(new GUIContent(
                "Save LODs meshes in scene (if no prefab)",
                "If enabled, generated meshes will be saved in the scene when generating LODs in prefab mode. Otherwise, it will be saved at outside of the prefab in the Asset folder. In general, we recommend generating LODs in prefab mode."), SaveMeshesInScene);

            EditorGUIUtility.labelWidth = 150;

            EditorGUI.BeginDisabledGroup(SaveMeshesInPrefab && SaveMeshesInScene);
            SaveMeshesPath = EditorGUILayout.TextField(new GUIContent("Path", "Path where the generated LODs (meshes) will be saved."), SaveMeshesPath);
            EditorGUI.EndDisabledGroup();
        }

        public static bool SaveMeshesInPrefab
        {
            get => EditorPrefs.GetBool("Nanolod_SaveMeshesInPrefab", true);
            set => EditorPrefs.SetBool("Nanolod_SaveMeshesInPrefab", value);
        }

        public static bool SaveMeshesInScene
        {
            get => EditorPrefs.GetBool("Nanolod_SaveMeshesInScene", true);
            set => EditorPrefs.SetBool("Nanolod_SaveMeshesInScene", value);
        }

        public static string SaveMeshesPath
        {
            get => EditorPrefs.GetString("Nanolod_SaveMeshesPath", "Assets/Nanolod/Cache/");
            set => EditorPrefs.SetString("Nanolod_SaveMeshesPath", value);
        }
    }
}
