using Nanomesh;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Nanolod
{
    public static class LODGroupMenu
    {
        [MenuItem("CONTEXT/LODGroup/Auto Generate LODs", priority = 0)]
        public static void GenerateLODs(MenuCommand command)
        {
            LODGroup lodGroup = (LODGroup)command.context;

            GenerateLODs(lodGroup);
        }

        public static void GenerateLODs(LODGroup lodGroup, HashSet<Mesh> newMeshes = null)
        {
            LOD[] lods = lodGroup.GetLODs();

            // Cleanup
            for (int i = 1; i < lods.Length; i++)
            {
                foreach (Renderer renderer in lods[i].renderers)
                {
                    UnityEngine.Object.DestroyImmediate(renderer.gameObject);
                }
            }

            // Assign LOD0
            Renderer[] renderers = lodGroup.GetComponentsInChildren<Renderer>();
            lods[0].renderers = renderers;

            Dictionary<Mesh, ConnectedMesh> uniqueMeshes = new Dictionary<Mesh, ConnectedMesh>();

            foreach (Renderer renderer in renderers)
            {
                if (renderer is MeshRenderer meshRenderer)
                {
                    MeshFilter meshFilter = renderer.gameObject.GetComponent<MeshFilter>();
                    if (meshFilter)
                    {
                        Mesh mesh = meshFilter.sharedMesh;
                        if (!uniqueMeshes.ContainsKey(mesh))
                        {
                            uniqueMeshes.Add(mesh, ConnectedMesh.Build(UnityConverter.ToSharedMesh(mesh)));
                        }
                    }
                }
                else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    Mesh mesh = skinnedMeshRenderer.sharedMesh;
                    if (!uniqueMeshes.ContainsKey(mesh))
                    {
                        uniqueMeshes.Add(mesh, ConnectedMesh.Build(UnityConverter.ToSharedMesh(mesh)));
                    }
                }
            }

            SceneDecimator sceneDecimator = new SceneDecimator();
            sceneDecimator.Initialize(uniqueMeshes.Values);

            // Build LODs
            for (int i = 1; i < lods.Length; i++)
            {
                // Decimates gradually
                sceneDecimator.DecimateToRatio(lods[i - 1].screenRelativeTransitionHeight);

                Dictionary<Mesh, Mesh> optimizedMeshes = uniqueMeshes.ToDictionary(x => x.Key, x => x.Value.ToSharedMesh().ToUnityMesh());

                List<Renderer> lodRenderers = new List<Renderer>();

                foreach (Renderer renderer in renderers)
                {
                    if (renderer is MeshRenderer meshRenderer)
                    {
                        MeshFilter meshFilter = renderer.gameObject.GetComponent<MeshFilter>();
                        if (meshFilter)
                        {
                            GameObject gameObject = new GameObject(renderer.gameObject.name + "_LOD" + i);
                            gameObject.transform.parent = renderer.transform;
                            gameObject.transform.localPosition = UnityEngine.Vector3.zero;
                            gameObject.transform.localRotation = UnityEngine.Quaternion.identity;
                            gameObject.transform.localScale = UnityEngine.Vector3.one;

                            MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
                            MeshFilter mf = gameObject.AddComponent<MeshFilter>();

                            Mesh originalMesh = meshFilter.sharedMesh;
                            Mesh optimizedMesh = optimizedMeshes[originalMesh]; // Todo : Don't create new mesh if it's the same (tri count);

                            optimizedMesh.name = originalMesh.name + "_LOD" + i;

                            mr.sharedMaterials = meshRenderer.sharedMaterials;
                            mf.sharedMesh = optimizedMesh;

                            newMeshes.Add(optimizedMesh);

                            lodRenderers.Add(mr);
                        }
                    }
                    else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                    {
                        GameObject gameObject = new GameObject(renderer.gameObject.name + "_LOD" + i);
                        gameObject.transform.parent = renderer.transform;
                        gameObject.transform.localPosition = UnityEngine.Vector3.zero;
                        gameObject.transform.localRotation = UnityEngine.Quaternion.identity;
                        gameObject.transform.localScale = UnityEngine.Vector3.one;

                        SkinnedMeshRenderer smr = gameObject.AddComponent<SkinnedMeshRenderer>();
                        smr.bones = skinnedMeshRenderer.bones;
                        smr.rootBone = skinnedMeshRenderer.rootBone;

                        Mesh originalMesh = skinnedMeshRenderer.sharedMesh;
                        Mesh optimizedMesh = optimizedMeshes[originalMesh]; // Todo : Don't create new mesh if it's the same (tri count);

                        optimizedMesh.name = originalMesh.name + "_LOD" + i;

                        smr.sharedMaterials = skinnedMeshRenderer.sharedMaterials;
                        smr.sharedMesh = optimizedMesh;

                        newMeshes.Add(optimizedMesh);

                        lodRenderers.Add(smr);
                    }
                }

                ///Debug.Log($"LOD{i} created with {lodRenderers.Count} renderers at {100f * lods[i - 1].screenRelativeTransitionHeight}% poly ratio");
                lods[i].renderers = lodRenderers.ToArray();
            }

            lodGroup.SetLODs(lods);
        }
    }
}