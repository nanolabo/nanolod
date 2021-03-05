using System;
using UnityEditor;
using UnityEngine;

namespace Nanolod
{
    [CustomPropertyDrawer(typeof(LODs))]
    public class LODsSettingsDrawer : PropertyDrawer
    {
        private const int SPLITTER_WIDTH = 12;
        private const float MINIMUM_LOD_RANGE = 0.01f;

        public static readonly Color[] LOD_COLORS_FOCUS = new Color[] {
            new Color(0.38039f, 0.49020f, 0.01961f),
            new Color(0.21961f, 0.32157f, 0.45882f),
            new Color(0.16471f, 0.41961f, 0.51765f),
            new Color(0.41961f, 0.12549f, 0.01961f),
            new Color(0.30196f, 0.22745f, 0.41569f),
            new Color(0.63137f, 0.34902f, 0.00000f),
            new Color(0.35294f, 0.32157f, 0.03922f),
            new Color(0.61176f, 0.50196f, 0.01961f),
        };

        // Todo : Light theme colors are different
        public static readonly Color[] LOD_COLORS = new Color[] {
            new Color(0.23529f, 0.27451f, 0.10196f),
            new Color(0.18039f, 0.21569f, 0.26275f),
            new Color(0.15686f, 0.25098f, 0.28627f),
            new Color(0.25098f, 0.14510f, 0.10588f),
            new Color(0.20784f, 0.18039f, 0.24706f),
            new Color(0.32549f, 0.22745f, 0.09804f),
            new Color(0.22745f, 0.21569f, 0.11373f),
            new Color(0.32157f, 0.27843f, 0.10588f),
        };

        public static readonly Color CULLED_COLOR = new Color(0.31373f, 0f, 0f);
        public static readonly Color CULLED_COLOR_FOCUS = new Color(0.62745f, 0f, 0f);
        public static readonly Color FRAME_COLOR_FOCUS = new Color(0.23922f, 0.37647f, 0.56863f);

        private int selectedLodIndexPending = 0;
        private int selectedLodIndex = 0;
        private int grabbing = -1;

        public static Color GetLodColor(int lodNbr, bool isCulled, bool isSelected)
        {
            return isCulled ? (isSelected ? CULLED_COLOR_FOCUS : CULLED_COLOR) : (isSelected ? LOD_COLORS_FOCUS[lodNbr] : LOD_COLORS[lodNbr]);
        }

        public static GUIStyle _LodPercentTextStyle;
        public static GUIStyle LodPercentTextStyle
        {
            get
            {
                if (_LodPercentTextStyle == null)
                {
                    _LodPercentTextStyle = new GUIStyle();
                    _LodPercentTextStyle.alignment = TextAnchor.MiddleRight;
                    _LodPercentTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(.8f, .8f, .8f) : new Color(.1f, .1f, .1f);
                }
                return _LodPercentTextStyle;
            }
        }

        private float Scale(float value)
        {
            return Mathf.Pow(value, 0.5f);
        }

        private float Descale(float value)
        {
            return Mathf.Pow(value, 2);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _serializedProperty = property;

            EditorGUI.BeginProperty(position, label, property);

            DrawLODs(property);

            EditorGUI.EndProperty();
        }

        private void DrawLODs(SerializedProperty property)
        {
            if (Event.current.type == EventType.Layout)
            {
                selectedLodIndex = selectedLodIndexPending;
            }

            var lodsArray = property.FindPropertyRelative("lods");
            int count = lodsArray.arraySize;

            EditorGUILayout.LabelField("LODs", EditorStyles.boldLabel);

            bool createLods = EditorGUILayout.Toggle("Create LODs", count > 0);

            GUILayout.Space(20);

            if (count == 0)
            {
                if (createLods)
                {
                    lodsArray.InsertArrayElementAtIndex(0);
                    lodsArray.InsertArrayElementAtIndex(0);
                    lodsArray.InsertArrayElementAtIndex(0);

                    lodsArray.GetArrayElementAtIndex(0).FindPropertyRelative("threshold").floatValue = 0.5f;
                    lodsArray.GetArrayElementAtIndex(1).FindPropertyRelative("threshold").floatValue = 0.25f;
                    lodsArray.GetArrayElementAtIndex(2).FindPropertyRelative("threshold").floatValue = 0.05f;

                    SetDirty();
                    return;
                }
            }
            else
            {
                if (!createLods)
                {
                    for (int i = 0; i < count; i++)
                    {
                        lodsArray.DeleteArrayElementAtIndex(0);
                    }

                    SetDirty();
                    return;
                }

                Rect sliderRect = EditorGUILayout.GetControlRect();
                sliderRect.y -= 2;
                sliderRect.height = 30;
                GUILayout.Space(20);

                float previousThreshold = 1f;

                Rect rect = new Rect();
                for (int i = 0; i < count; i++)
                {
                    rect = DrawLOD(lodsArray, count, ref sliderRect, ref previousThreshold, i);
                    if (i < count)
                    {
                        DrawSplitter(i, previousThreshold, rect);
                    }
                }

                if (previousThreshold > 0f)
                {
                    DrawSplitter(count - 1, previousThreshold, rect);
                    DrawLOD(lodsArray, count, ref sliderRect, ref previousThreshold, count);
                }

                if (Event.current.type == EventType.MouseUp)
                    grabbing = int.MinValue;

                float mouseDeltaX = 0;
                if (grabbing != int.MinValue && Event.current.type == EventType.MouseDrag)
                {
                    mouseDeltaX = Event.current.delta.x;
                }

                if (mouseDeltaX != 0)
                {
                    float threshold = lodsArray.GetArrayElementAtIndex(grabbing).FindPropertyRelative("threshold").floatValue;
                    float delta = -mouseDeltaX / sliderRect.width;

                    // Moves dragging LOD
                    float max = (grabbing > 0) ? lodsArray.GetArrayElementAtIndex(grabbing - 1).FindPropertyRelative("threshold").floatValue - MINIMUM_LOD_RANGE : 1 - MINIMUM_LOD_RANGE;
                    float min = (grabbing < count - 1) ? lodsArray.GetArrayElementAtIndex(grabbing + 1).FindPropertyRelative("threshold").floatValue + MINIMUM_LOD_RANGE : 0f;
                    float newThreshold = Descale(Scale(threshold) + delta);
                    newThreshold = Mathf.Clamp(newThreshold, min, max);
                    lodsArray.GetArrayElementAtIndex(grabbing).FindPropertyRelative("threshold").doubleValue = newThreshold;

                    // Triggers change (for Repaint in Editors)
                    SetDirty();
                }
            }
        }

        private Rect DrawLOD(SerializedProperty lodsArray, int count, ref Rect sliderRect, ref float previousThreshold, int i)
        {
            bool culled = false;
            float currentThreshold;

            if (i >= count)
            {
                culled = true;
                currentThreshold = 0f;
            }
            else
            {
                var lod = lodsArray.GetArrayElementAtIndex(i);
                currentThreshold = lod.FindPropertyRelative("threshold").floatValue;
            }

            float width = Scale(previousThreshold) - Scale(currentThreshold);

            // Draw Block
            Rect labelRect = new Rect(
                new Vector2(sliderRect.position.x + (1 - (float)Scale(previousThreshold)) * sliderRect.width, sliderRect.position.y),
                new Vector2(sliderRect.width * width, sliderRect.height)
            );

            string subInfo = null;

            if (i > 0)
            {
                if (culled)
                {
                    subInfo = "Culled";
                }
                else
                {
                    subInfo = Math.Round(100 * previousThreshold, 1) + "% tris";
                }
            }

            GUIContent title = new GUIContent($" LOD {i}\n " + subInfo);
            title.tooltip = "Right click to insert or remove an LOD";

            EditorGUIExtensions.GUIDrawRect(labelRect, GetLodColor(i, culled, selectedLodIndex == i),
                FRAME_COLOR_FOCUS, selectedLodIndex == i ? 3 : 0, title, TextAnchor.MiddleLeft);

            // Check if click on LOD
            EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Link);
            if (labelRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0)
                    {
                        selectedLodIndexPending = i;
                        // Triggers change (for Repaint in Editors)
                        SetDirty();
                    }
                    else if (Event.current.button == 1)
                    {
                        selectedLodIndexPending = i;
                        GenericMenu genericMenu = new GenericMenu();

                        genericMenu.AddItem(new GUIContent("Add"), false, () =>
                        {
                            InsertLOD(lodsArray, selectedLodIndexPending);
                            SetDirty();
                        });

                        if (count > 1)
                        {
                            genericMenu.AddItem(new GUIContent("Remove"), false, () =>
                            {
                                DeleteLOD(lodsArray, selectedLodIndexPending);
                                SetDirty();
                            });
                        }
                        else
                        {
                            genericMenu.AddDisabledItem(new GUIContent("Remove"));
                        }

                        genericMenu.ShowAsContext();
                    }
                }
            }

            previousThreshold = currentThreshold;

            return labelRect;
        }

        private void DrawSplitter(int i, float currentThreshold, Rect labelRect)
        {
            Rect splitter = new Rect(labelRect.x + labelRect.width, labelRect.y, SPLITTER_WIDTH, labelRect.height);
            EditorGUI.LabelField(new Rect(splitter.x - 20, splitter.y - 20, 40, 20), (Math.Round(currentThreshold * 100)) + "%", LodPercentTextStyle);
            EditorGUIUtility.AddCursorRect(splitter, MouseCursor.ResizeHorizontal);
            if (splitter.Contains(Event.current.mousePosition) && (Event.current.type == EventType.MouseDown && Event.current.button == 0))
            {
                grabbing = i;
            }
        }

        private SerializedProperty _serializedProperty;

        public SerializedObject serializedObject => _serializedProperty.serializedObject;

        public OptimizationSettings optimizationSettings => serializedObject.targetObject as OptimizationSettings;

        private void SetDirty()
        {
            // Save to extra properties
            optimizationSettings.SaveToImporter();

            // Apply
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.Update();

            // Repaint editor
            optimizationSettings.RepaintEditor();
        }

        private void DeleteLOD(SerializedProperty lodsArray, int index)
        {
            lodsArray.DeleteArrayElementAtIndex(index);

            selectedLodIndexPending--;
            selectedLodIndexPending = Mathf.Clamp(selectedLodIndexPending, 0, lodsArray.arraySize - 1);

            grabbing = int.MinValue;

            RearrangeThresholds(lodsArray);
        }

        private void InsertLOD(SerializedProperty lodsArray, int index)
        {
            double before = index == 0 ? 1 : lodsArray.GetArrayElementAtIndex(index - 1).FindPropertyRelative("threshold").doubleValue;
            double after = lodsArray.GetArrayElementAtIndex(index).FindPropertyRelative("threshold").doubleValue;

            lodsArray.InsertArrayElementAtIndex(index);
            lodsArray.GetArrayElementAtIndex(index).FindPropertyRelative("threshold").doubleValue = (after + before) / 2;

            selectedLodIndexPending++;

            RearrangeQualities(lodsArray, 0);
            RearrangeQualities(lodsArray, lodsArray.arraySize - 1);
            RearrangeThresholds(lodsArray);
        }

        /// <summary>
        /// Rearranges lod qualities so that it is continuous, from LOD0 with the highest quality and LODN the lowest
        /// </summary>
        /// <param name="lodsArray"></param>
        private void RearrangeQualities(SerializedProperty lodsArray, int startingIndex)
        {

        }

        /// <summary>
        /// Rearranges thresholds to that the order is kept and there is a minimum delta between two lods
        /// </summary>
        /// <param name="lodsArray"></param>
        private void RearrangeThresholds(SerializedProperty lodsArray)
        {
            for (int i = 0; i < lodsArray.arraySize; ++i)
            {
                if (i + 1 < lodsArray.arraySize)
                {
                    SerializedProperty leftLOD = lodsArray.GetArrayElementAtIndex(i);
                    SerializedProperty rightLOD = lodsArray.GetArrayElementAtIndex(i + 1);
                    if (leftLOD.FindPropertyRelative("threshold").doubleValue <= rightLOD.FindPropertyRelative("threshold").doubleValue + 0.027)
                        leftLOD.FindPropertyRelative("threshold").doubleValue = rightLOD.FindPropertyRelative("threshold").doubleValue + 0.05;
                }
                if (i == lodsArray.arraySize - 1)
                {
                    lodsArray.GetArrayElementAtIndex(i).FindPropertyRelative("threshold").doubleValue = 0;
                }
            }
        }
    }
}