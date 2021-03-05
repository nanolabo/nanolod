using UnityEditor;
using UnityEngine;

namespace Nanolod
{
    public static class EditorGUIExtensions
    {
        public static readonly Color EditorColorBasic = new Color(0.76f, 0.76f, 0.76f);
        public static readonly Color EditorColorPro = new Color(0.40f, 0.40f, 0.40f);
        public static readonly Color EditorColorBasicLight = new Color(0.87f, 0.87f, 0.87f);
        public static readonly Color EditorColorProLight = new Color(0.19f, 0.19f, 0.19f);

        public static Color EditorColor => EditorGUIUtility.isProSkin ? EditorColorPro : EditorColorBasic;
        public static Color EditorColorLight => EditorGUIUtility.isProSkin ? EditorColorProLight : EditorColorBasicLight;

        public static void GUIDrawRect(Rect position, Color color, int borderThickness = 0, string title = "")
        {
            Color tmp = color;
            tmp.a = 1;
            GUIDrawRect(position, color, tmp, borderThickness, new GUIContent(title), TextAnchor.MiddleCenter);
        }

        public static void GUIDrawRect(Rect position, Color color, Color borderColor, int borderThickness, GUIContent text, TextAnchor rectTextAnchor)
        {

            GUIStyle rectStyle = new GUIStyle();

            if (color != Color.white && color != Color.gray)
            {
                rectStyle.normal.textColor = Color.white;
            }

            rectStyle.clipping = TextClipping.Clip;
            rectStyle.border = new RectOffset(-borderThickness, -borderThickness, -borderThickness, -borderThickness);
            rectStyle.alignment = rectTextAnchor;
            rectStyle.fontSize = 10;

            Rect innerRect = position;

            if (borderThickness > 0)
            {
                EditorGUI.DrawRect(position, borderColor);
                innerRect = new Rect(position.x + borderThickness, position.y + borderThickness, position.width - borderThickness * 2, position.height - borderThickness * 2);
            }

            EditorGUI.DrawRect(innerRect, color);

            Rect contentRect = new Rect(position.x + borderThickness, position.y + borderThickness, position.width - borderThickness * 2, position.height - borderThickness * 2);
            GUI.Box(contentRect, text, rectStyle);
        }
    }
}