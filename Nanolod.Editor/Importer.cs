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

            string pathToAsmDef = AssetDatabase.GUIDToAssetPath("77926c82de2364debab5082355addfb4");
            string directory = Path.GetDirectoryName(pathToAsmDef);;
            string path = Path.Combine(directory, "Cache", $"{gameObject.GetInstanceID()}.asset");
            string fullPath = Path.GetFullPath(path);
            
            Directory.CreateDirectory(fullPath);
            
            if (AssetDatabase.DeleteAsset(path))
                AssetDatabase.Refresh();

            AssetDatabase.CreateAsset(settings, path);

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