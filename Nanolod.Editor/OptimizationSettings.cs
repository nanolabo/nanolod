using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nanolod
{
    public class OptimizationSettings : ScriptableObject
    {
        public Lods lods = new Lods();
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
    }

    [Serializable]
    public struct Lod
    {
        public float threshold;
        public GenerationSettings generationSettings;
        public float polyRatio;
    }

    public enum GenerationSettings
    {
        ThresholdBased,
        Custom,
        Culled,
    }
}
