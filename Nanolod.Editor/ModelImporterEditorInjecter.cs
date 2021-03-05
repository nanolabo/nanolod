using System;
using System.Reflection;
using UnityEditor;

namespace Nanolod
{
    [InitializeOnLoad]
    public static class ModelImporterEditorInjecter
    {
        private static readonly Type _assetTabbedImporterType;
        private static readonly Type _modelImporterEditorType;

        static ModelImporterEditorInjecter()
        {
            _assetTabbedImporterType = Type.GetType("UnityEditor.AssetImporterTabbedEditor, UnityEditor");
            _modelImporterEditorType = Type.GetType("UnityEditor.ModelImporterEditor, UnityEditor");

            Selection.selectionChanged += OnSelectionChanged;
            Editor.finishedDefaultHeaderGUI += OnEditorFinishedDefaultHeaderGUI;
        }

        private static void OnEditorFinishedDefaultHeaderGUI(Editor editor)
        {
            Inject(editor);
        }

        public static OptimizationSettings Current { get; private set; }

        private static void OnSelectionChanged()
        {
            Current = null;
            Inject(GetCurrentModelImporterEditor());
        }

        private static Editor GetCurrentModelImporterEditor()
        {
            foreach (Editor editor in ActiveEditorTracker.sharedTracker.activeEditors)
            {
                if (editor.GetType().IsAssignableFrom(_modelImporterEditorType))
                {
                    return editor;
                }
            }

            return null;
        }

        private static void Inject(Editor editor)
        {
            if (editor == null)
                return;

            if (editor == Current?.Editor)
                return;

            if (!editor.GetType().IsAssignableFrom(_modelImporterEditorType))
                return;

            Current = OptimizationSettings.Create(editor);

            SerializedObject serializedObject = new SerializedObject(Current);
            SerializedProperty serializedPropertyMyInt = serializedObject.FindProperty("lods");

            var tabs = (Array)_assetTabbedImporterType.GetField("m_Tabs", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(editor);
            var modelTab = tabs.GetValue(0);
            modelTab.GetType().GetField("m_SortHierarchyByName", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(modelTab, serializedPropertyMyInt);

            editor.Repaint();
        }
    }
}