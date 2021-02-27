using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nanolod
{
    [InitializeOnLoad]
    public static class ModelImporterEditorInjecter
    {
        private static Type _assetTabbedImporterType;
        private static Type _modelImporterEditorType;

        static ModelImporterEditorInjecter()
        {
            _assetTabbedImporterType = Type.GetType("UnityEditor.AssetImporterTabbedEditor, UnityEditor");
            _modelImporterEditorType = Type.GetType("UnityEditor.ModelImporterEditor, UnityEditor");
            Selection.selectionChanged += Inject;

            //Inject(); // Inject in case some importers are already in view
        }

        public static OptimizationSettings Current { get; private set; }

        private static void Inject()
        {
            foreach (Editor editor in ActiveEditorTracker.sharedTracker.activeEditors)
            {
                if (editor.GetType().IsAssignableFrom(_modelImporterEditorType))
                {
                    Current = ScriptableObject.CreateInstance<OptimizationSettings>();

                    string assetPath = AssetDatabase.GetAssetPath(editor.target as ModelImporter);
                    GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (gameObject != null)
                    {
                        LODGroup lodGroup = gameObject.GetComponent<LODGroup>();
                        if (lodGroup != null)
                        {
                            Current.lods.CreateFromLods(lodGroup.GetLODs());
                        }
                    }

                    SerializedObject serializedObject = new SerializedObject(Current);

                    SerializedProperty serializedPropertyMyInt = serializedObject.FindProperty("lods");

                    var tabs = (Array)_assetTabbedImporterType.GetField("m_Tabs", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(editor);
                    var modelTab = tabs.GetValue(0);
                    modelTab.GetType().GetField("m_ImportCameras", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(modelTab, serializedPropertyMyInt);

                    editor.Repaint();
                }
            }
        }

        public static void RepaintModelImporters()
        {
            foreach (Editor ed in ActiveEditorTracker.sharedTracker.activeEditors)
            {
                if (ed.GetType().IsAssignableFrom(_modelImporterEditorType))
                {
                    ed.Repaint();
                }
            }
        }
    }
}
