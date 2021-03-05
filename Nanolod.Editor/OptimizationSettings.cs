using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nanolod
{
    public class OptimizationSettings : ScriptableObject
    {
        public Editor Editor { get; private set; }

        public ModelImporter ModelImporter { get; private set; }

        public LODs lods;

        public static OptimizationSettings Create(Editor modelImporterEditor)
        {
            if (!(modelImporterEditor.target is ModelImporter modelImporter))
                throw new Exception("Editor must be a ModelImporterEditor !");

            var optimizationSettings = CreateInstance<OptimizationSettings>();
            optimizationSettings.ModelImporter = modelImporter;
            optimizationSettings.Editor = modelImporterEditor;
            optimizationSettings.lods = new LODs();

            optimizationSettings.LoadFromImporter();

            return optimizationSettings;
        }

        public void RepaintEditor()
        {
            Editor.Repaint();
        }

        public void LoadFromImporter()
        {
            string val = ExtraPropertyValue;
            if (val == "nanolod")
            {
                // Attempt to load from LODGroup if there is one already
                string assetPath = AssetDatabase.GetAssetPath(ModelImporter);
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
            for (int i = 0; i < ModelImporter.extraUserProperties.Length; i++)
            {
                if (ModelImporter.extraUserProperties[i].StartsWith("nanolod"))
                    return i;
            }
            return -1; // not found
        }

        private string ExtraPropertyValue
        {
            get
            {
                int index = GetExtraPropertyIndex();
                if (index == -1)
                    return "nanolod";
                else
                    return ModelImporter.extraUserProperties[index];
            }
            set
            {
                int index = GetExtraPropertyIndex();
                if (index == -1)
                {
                    var props = ModelImporter.extraUserProperties;
                    index = props.Length;
                    CollectionExtensions.Append(ref props, value);
                    ModelImporter.extraUserProperties = props;
                }
                else
                {
                    var props = ModelImporter.extraUserProperties;
                    props[index] = value;
                    ModelImporter.extraUserProperties = props;
                }
                ///Debug.Log($"Save '{value}' at index {index}");
            }
        }
    }

    [Serializable]
    public class LODs
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
