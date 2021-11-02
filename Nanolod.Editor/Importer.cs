using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Nanolod
{
    public class Importer : AssetPostprocessor
    {
        private void OnPostprocessModel(GameObject gameObject)
        {
            OptimizationSettings settings = ModelImporterEditorInjecter.Current;
            if (settings == null || settings.lods.lods.Length == 0)
            {
                return;
            }

            LODGroup lodGroup = gameObject.AddComponent<LODGroup>();

            lodGroup.SetLODs(settings.lods.ConvertToLods());

            HashSet<Mesh> newMeshes = new HashSet<Mesh>();

            LODGroupMenu.GenerateLODs(lodGroup, newMeshes);
            
            string path;
            
            if (AssetDatabase.Contains(settings))
            {
                // Settings are already serialized in an Asset file : we reuse it
                path = AssetDatabase.GetAssetPath(settings);
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (asset != settings)
                        AssetDatabase.RemoveObjectFromAsset(asset);
            }
            else
            {
                // Settings don't exists : we create a new asset in the cache directory
                string pathToAsmDef = AssetDatabase.GUIDToAssetPath("77926c82de2364debab5082355addfb4");
                string pluginDir = Path.GetDirectoryName(pathToAsmDef);
                string cacheDir = Path.Combine(pluginDir, "Cache");
                path = Path.Combine(cacheDir, $"{settings.GetInstanceID()}.asset");

                // Ensure cache directory exists
                Directory.CreateDirectory(Path.GetFullPath(cacheDir));
                
                AssetDatabase.CreateAsset(settings, path);
            }

            int i = 0;
            
            foreach (Mesh newMesh in newMeshes)
            {
                newMesh.name = "mesh_" + i++;
                AssetDatabase.AddObjectToAsset(newMesh, path);
            }

            AssetDatabase.SaveAssets();
        }
    }
}