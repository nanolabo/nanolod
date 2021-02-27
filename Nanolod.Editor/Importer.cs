using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Nanolod
{
    public class Importer : AssetPostprocessor
    {
        private void OnPostprocessModel(GameObject gameObject)
        {
            LODGroup lodGroup = gameObject.AddComponent<LODGroup>();

            var settings = ModelImporterEditorInjecter.Current;

            lodGroup.SetLODs(settings.lods.ConvertToLods());

            List<Mesh> newMeshes = new List<Mesh>();

            LODGroupMenu.GenerateLODs(lodGroup, newMeshes);

            foreach (Mesh newMesh in newMeshes)
            {
                Debug.Log("cc " + newMesh);
                AssetDatabase.AddObjectToAsset(newMesh, assetPath);
            }
        }
    }
}