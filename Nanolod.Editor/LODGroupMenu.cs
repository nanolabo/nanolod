﻿using Nanomesh;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Nanolod
{
    public static class LODGroupMenu
    {
        [MenuItem("CONTEXT/LODGroup/Auto Generate LODs", priority = 0)]
        public static void GenerateLODs(MenuCommand command)
        {
            LODGroup lodGroup = (LODGroup)command.context;

            var prefabRoot = lodGroup.gameObject;

            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                if (!stage.IsPartOfPrefabContents(prefabRoot))
                {
                    Debug.LogWarning($"Nanolod: The prefab mode is open but the selected lod group is not part of it! LOD generation aborted.");
                    return;
                }
            }

            HashSet<Mesh> newMeshes = new HashSet<Mesh>();
            HashSet<Mesh> deletedMeshes = new HashSet<Mesh>();

            GenerateLODs(lodGroup, newMeshes, deletedMeshes);

            // Try to delete unused assets, if any
            bool assetsDeleted = false;
            foreach (var deletedMesh in deletedMeshes)
            {
                try
                {
                    var assetPath = AssetDatabase.GetAssetPath(deletedMesh);
                    var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    if (mainAsset is Mesh mesh && mesh.name.StartsWith("lod_"))
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                        assetsDeleted = true;
                    }
                }
                catch { }
            }

            if (stage)
            {
                var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(stage.assetPath);

                foreach (var deletedMesh in deletedMeshes)
                {
                    try
                    {
                        AssetDatabase.RemoveObjectFromAsset(deletedMesh);
                    }
                    catch { }
                }

                if (Preferences.SaveMeshesInPrefab)
                {
                    foreach (var newMesh in newMeshes)
                    {
                        AssetDatabase.AddObjectToAsset(newMesh, prefabAsset);
                    }
                }
                else
                {
                    SaveMeshesAsAssets(newMeshes);
                }

                // Trigger save
                EditorSceneManager.MarkSceneDirty(stage.scene);
            }
            else
            {
                if (!Preferences.SaveMeshesInScene)
                {
                    SaveMeshesAsAssets(newMeshes);
                }
            }

            if (assetsDeleted)
            {
                AssetDatabase.Refresh();
            }
        }

        private static void SaveMeshesAsAssets(IEnumerable<Mesh> meshes)
        {
            foreach (var mesh in meshes)
            {
                string path = Path.Combine(Preferences.SaveMeshesPath, $"lod_{mesh.GetInstanceID()}.asset");

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetFullPath(path));

                AssetDatabase.CreateAsset(mesh, path);
            }

            AssetDatabase.SaveAssets();
        }

        private static bool TryGetMesh(this Renderer renderer, out Mesh mesh)
        {
            mesh = null;

            if (renderer != null)
            {
                if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    mesh = skinnedMeshRenderer.sharedMesh;
                }
                else
                {
                    var meshFilter = renderer.gameObject.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        mesh = meshFilter.sharedMesh;
                    }
                }
            }

            return mesh != null;
        }

        public static void GenerateLODs(LODGroup lodGroup, HashSet<Mesh> newMeshes = null, HashSet<Mesh> deletedMeshes = null)
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();

            LOD[] lods = lodGroup.GetLODs();

            // Cleanup
            for (int i = 1; i < lods.Length; i++)
            {
                foreach (Renderer renderer in lods[i].renderers)
                {
                    if (renderer != null)
                    {
                        if (deletedMeshes != null) {
                            if (TryGetMesh(renderer, out Mesh mesh))
                            {
                                deletedMeshes.Add(mesh);
                            }
                        }

                        Object.DestroyImmediate(renderer.gameObject);
                    }
                }
            }

            if (newMeshes == null)
                newMeshes = new HashSet<Mesh>();

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
                            uniqueMeshes.TryAdd(mesh, m => UnityConverter.ToSharedMesh(m).ToConnectedMesh());
                        }
                    }
                }
                else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    Mesh mesh = skinnedMeshRenderer.sharedMesh;
                    if (!uniqueMeshes.ContainsKey(mesh))
                    {
                        uniqueMeshes.TryAdd(mesh, m => UnityConverter.ToSharedMesh(m).ToConnectedMesh());
                    }
                }
            }

            foreach (KeyValuePair<Mesh, ConnectedMesh> uniqueMesh in uniqueMeshes)
            {
                uniqueMesh.Value.MergePositions(0.0001);
                uniqueMesh.Value.MergeAttributes();
                uniqueMesh.Value.Compact();
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

                        optimizedMesh.bindposes = originalMesh.bindposes; // Copy poses

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