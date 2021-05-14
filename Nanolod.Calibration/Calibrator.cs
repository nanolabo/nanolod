using Nanolod.Calibration.Phash;
using Nanomesh;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nanolod.Calibration
{
    public class Calibrator : MonoBehaviour
    {
        public int iterations = 10;

        private List<Mesh> _originalMeshes;
        private List<Mesh> _meshes;

        private void Start()
        {
            _originalMeshes = new List<Mesh>();
            _meshes = new List<Mesh>();

            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

            HashSet<Mesh> uniqueMeshes = new HashSet<Mesh>();

            foreach (Renderer renderer in renderers)
            {
                if (renderer is MeshRenderer meshRenderer)
                {
                    MeshFilter meshFilter = renderer.gameObject.GetComponent<MeshFilter>();
                    if (!meshFilter)
                        continue;

                    Mesh mesh = meshFilter.sharedMesh;

                    if (!uniqueMeshes.Add(mesh))
                        continue;

                    _originalMeshes.Add(mesh);

                    // Clone mesh
                    mesh = meshFilter.sharedMesh = UnityConverter.ToSharedMesh(mesh).ToUnityMesh();
                    _meshes.Add(mesh);
                }
                else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    Mesh mesh = skinnedMeshRenderer.sharedMesh;

                    if (!uniqueMeshes.Add(mesh))
                        continue;

                    _originalMeshes.Add(mesh);

                    // Clone mesh
                    mesh = skinnedMeshRenderer.sharedMesh = mesh.ToSharedMesh().ToUnityMesh();
                    _meshes.Add(mesh);
                }
            }

            Camera camera = Camera.main;

            Texture2D textureOriginal = CalibrationUtils.CaptureScreenshot(camera, 1000, 1000);
            File.WriteAllBytes(@"C:\Users\oginiaux\Downloads\trace\original.jpg", textureOriginal.EncodeToJPG());
            Digest originalDigest = ImagePhash.ComputeDigest(CalibrationUtils.ToLuminanceImage(textureOriginal));

            List<Dictionary<string, float>> results = new List<Dictionary<string, float>>();

            float highestCorrelation = float.MinValue;

            for (int i = 0; i < iterations; i++)
            {
                Dictionary<string, float> values = new Dictionary<string, float>();

                values["NormalWeight"] = Random.Range(0f, 100f);
                values["MergeThreshold"] = Random.Range(0.00001f, 0.1f);
                values["MergeNormalsThreshold"] = MathF.Cos(Random.Range(5f, 140f) * MathF.PI / 180f);
                values["UseEdgeLength"] = Random.Range(0f, 1f);
                values["UpdateFarNeighbors"] = Random.Range(0f, 0.75f);
                values["UpdateMinsOnCollapse"] = Random.Range(0.25f, 1f);
                values["EdgeBorderPenalty"] = Random.Range(0f, 1000f);
                values["Case"] = i;

                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                SetDecimation(0.5f, values);

                sw.Stop();
                values["Time"] = (float)sw.Elapsed.TotalMilliseconds;

                Texture2D textureDecimated = CalibrationUtils.CaptureScreenshot(camera, 1000, 1000);
                Digest modified = ImagePhash.ComputeDigest(CalibrationUtils.ToLuminanceImage(textureDecimated));

                values["Correlation"] = ImagePhash.GetCrossCorrelation(originalDigest, modified);

                if (highestCorrelation < values["Correlation"])
                {
                    highestCorrelation = values["Correlation"];
                    File.WriteAllBytes($@"C:\Users\oginiaux\Downloads\trace\case_{i}.jpg", textureDecimated.EncodeToJPG());
                }

                results.Add(values);
            }

            foreach (Dictionary<string, float> result in results.OrderByDescending(x => x["Correlation"]).Take(3))
            {
                StringBuilder strbldr = new StringBuilder();
                foreach (KeyValuePair<string, float> pair in result.OrderBy(x => x.Key))
                {
                    strbldr.Append($"{pair.Key} = {pair.Value}\n");
                }
                Debug.Log(strbldr);
            }
        }

        private void SetDecimation(float value, Dictionary<string, float> variables)
        {
            ConnectedMesh[] connectedMeshes = _originalMeshes.Select(x => UnityConverter.ToSharedMesh(x).ToConnectedMesh()).ToArray();

            foreach (ConnectedMesh connectedMesh in connectedMeshes)
            {
                for (int i = 0; i < connectedMesh.attributeDefinitions.Length; i++)
                {
                    switch (connectedMesh.attributeDefinitions[i].type)
                    {
                        case AttributeType.Normals:
                            connectedMesh.attributeDefinitions[i].weight = variables["NormalWeight"];
                            break;
                    }
                }
            }

            foreach (ConnectedMesh connectedMesh in connectedMeshes)
            {
                // Important step :
                // We merge positions to increase chances of having correct topology information
                // We merge attributes in order to make interpolation properly operate on every face
                connectedMesh.MergePositions(variables["MergeThreshold"]);
                connectedMesh.MergeAttributes();
                connectedMesh.Compact();
            }

            DecimateModifier.MergeNormalsThreshold = variables["MergeNormalsThreshold"];
            DecimateModifier.UpdateFarNeighbors = variables["UpdateFarNeighbors"] > 0.5;
            DecimateModifier.UpdateMinsOnCollapse = variables["UpdateMinsOnCollapse"] > 0.5;
            DecimateModifier.UseEdgeLength = variables["UseEdgeLength"] > 0.5;
            ConnectedMesh.EdgeBorderPenalty = variables["EdgeBorderPenalty"];

            SceneDecimator sceneDecimator = new SceneDecimator();
            sceneDecimator.Initialize(connectedMeshes);

            sceneDecimator.DecimateToRatio(value);

            for (int i = 0; i < connectedMeshes.Length; i++)
            {
                _meshes[i].Clear();
                connectedMeshes[i].ToSharedMesh().ToUnityMesh(_meshes[i]);
                _meshes[i].bindposes = _originalMeshes[i].bindposes;
            }
        }
    }
}