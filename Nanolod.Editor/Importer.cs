using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Nanolod
{
    public class Importer : AssetPostprocessor
    {
        private void OnPostprocessModel(GameObject gameObject)
        {
            var settings = ModelImporterEditorInjecter.Current;
            if (settings.lods.lods.Length == 0)
                return;

            LODGroup lodGroup = gameObject.AddComponent<LODGroup>();

            lodGroup.SetLODs(settings.lods.ConvertToLods());

            List<Mesh> newMeshes = new List<Mesh>();

            LODGroupMenu.GenerateLODs(lodGroup, newMeshes);

            int i = 0;

            foreach (Mesh newMesh in newMeshes)
            {
                context.AddObjectToAsset("nanolod_lod_" + i++, newMesh);
            }
        }
    }
}