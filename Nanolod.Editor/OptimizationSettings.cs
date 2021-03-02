using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nanolod
{
    public class OptimizationSettings : ScriptableObject
    {
        private ModelImporter modelImporter;
        private Editor editor;
        public Lods lods = new Lods();

        public static OptimizationSettings Create(Editor modelImporterEditor)
        {
            if (!(modelImporterEditor.target is ModelImporter modelImporter))
                throw new Exception("Editor must be a ModelImporterEditor !");

            var optimizationSettings = CreateInstance<OptimizationSettings>();
            optimizationSettings.modelImporter = modelImporter;
            optimizationSettings.editor = modelImporterEditor;

            optimizationSettings.LoadFromImporter();

            return optimizationSettings;
        }

        public void RepaintEditor()
        {
            editor.Repaint();
        }

        public void LoadFromImporter()
        {
            string val = ExtraPropertyValue;
            if (val == "nanolod")
            {
                // Attempt to load from LODGroup if there is one already
                string assetPath = AssetDatabase.GetAssetPath(modelImporter);
                GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (gameObject != null)
                {
                    LODGroup lodGroup = gameObject.GetComponent<LODGroup>();
                    if (lodGroup != null)
                    {
                        ///Debug.Log("Loaded from existing LODs");
                        lods.CreateFromLods(lodGroup.GetLODs());
                        return;
                    }
                }
                // Otherwise start with no LODs
                ///Debug.Log("Create new LODs");
                lods.lods = new Lod[0];
            }
            else
            {
                // Load from property
                ///Debug.Log("Load from property (" + val + ")");
                char[] splitchars = new[] { '_' };
                string[] split = val.Split(splitchars);
                lods.lods = new Lod[split.Length - 1];
                for (int i = 1; i < split.Length; i++)
                {
                    lods.lods[i - 1] = new Lod { threshold = float.Parse(split[i]) };
                }
            }
        }

        public void SaveToImporter()
        {
            ExtraPropertyValue = "nanolod" + lods.ToString();
        }

        private int GetExtraPropertyIndex()
        {
            for (int i = 0; i < modelImporter.extraUserProperties.Length; i++)
            {
                if (modelImporter.extraUserProperties[i].StartsWith("nanolod"))
                    return i;
            }
            return -1; // not found
        }

        private string ExtraPropertyValue
        {
            get {
                int index = GetExtraPropertyIndex();
                if (index == -1)
                    return "nanolod";
                else
                    return modelImporter.extraUserProperties[index];
            }
            set
            {
                int index = GetExtraPropertyIndex();
                if (index == -1)
                {
                    var props = modelImporter.extraUserProperties;
                    index = props.Length;
                    CollectionExtensions.Append(ref props, value);
                    modelImporter.extraUserProperties = props;
                }
                else
                {
                    var props = modelImporter.extraUserProperties;
                    props[index] = value;
                    modelImporter.extraUserProperties = props;
                }
                ///Debug.Log($"Save '{value}' at index {index}");
            }
        }
    }

    [Serializable]
    public class Lods
    {
        public Lod[] lods = new Lod[0];

        public void CreateDefault()
        {
            lods = new Lod[] {
                new Lod { threshold = 0.5f },
                new Lod { threshold = 0.25f },
                new Lod { threshold = 0.05f },
                new Lod { threshold = 0.0f },
            };
        }

        public LOD[] ConvertToLods()
        {
            List<LOD> unityLods = new List<LOD>();
            for (int i = 0; i < lods.Length; i++)
            {
                unityLods.Add(new LOD { screenRelativeTransitionHeight = lods[i].threshold });
            }
            return unityLods.ToArray();
        }

        public void CreateFromLods(LOD[] unityLods)
        {
            lods = new Lod[unityLods.Length];
            for (int i = 0; i < unityLods.Length; i++)
            {
                lods[i].threshold = unityLods[i].screenRelativeTransitionHeight;
            }
        }

        public override string ToString()
        {
            string output = "";
            for (int i = 0; i < lods.Length; i++)
            {
                output += "_" + lods[i].ToString();
            }
            return output;
        }
    }

    [Serializable]
    public struct Lod
    {
        public float threshold;
        public GenerationSettings generationSettings;
        public float polyRatio;

        public override string ToString()
        {
            return Math.Round(threshold, 3).ToString();
        }
    }

    public enum GenerationSettings
    {
        ThresholdBased,
        Custom,
        Culled,
    }
}
