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

            int i = 0;

            string directory = "Packages/com.nanolabo.nanolod/Cache";
            string path = Path.Combine(directory, $"{gameObject.GetInstanceID()}.asset");

            Directory.CreateDirectory(Path.GetFullPath(path));

            AssetDatabase.CreateAsset(settings, path);

            foreach (Mesh newMesh in newMeshes)
            {
                newMesh.name = "mesh_" + i;
                AssetDatabase.AddObjectToAsset(newMesh, path);
            }

            AssetDatabase.SaveAssets();
        }
    }
}