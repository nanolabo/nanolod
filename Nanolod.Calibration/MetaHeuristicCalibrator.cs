using Nanolod.Calibration.Common;
using Nanolod.Calibration.GA;
using Nanolod.Calibration.HillClimb;
using Nanolod.Calibration.Phash;
using Nanomesh;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nanolod.Calibration
{
    public enum MetaHeuristicAlgo
    {
        GeneticAlgorithm,
        HillClimbing,
    }

    public class MetaHeuristicCalibrator : MonoBehaviour
    {
        public int iterations = 10;
        public int populationSize = 10;

        [Range(0f, 1f)]
        public float polycountTarget = 0.5f;

        [Range(1, 10)]
        public int mutationsPerIteration = 1;

        public float mutationMaximum = 10f;

        public MetaHeuristicAlgo metaHeuristicAlgo;

        private List<Mesh> _originalMeshes;
        private List<Mesh> _meshes;

        private Digest _originalDigest;
        private Camera _camera;

        private IMetaHeuristic<Dictionary<string, float>> _metaheuristicAlgorithm;

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

            _camera = Camera.main;

            Texture2D textureOriginal = CalibrationUtils.CaptureScreenshot(_camera, 1000, 1000);
            _originalDigest = ImagePhash.ComputeDigest(CalibrationUtils.ToLuminanceImage(textureOriginal));

            switch (metaHeuristicAlgo)
            {
                case MetaHeuristicAlgo.GeneticAlgorithm:
                    _metaheuristicAlgorithm = new GeneticAlgorithm<Dictionary<string, float>>(Crossover);
                    break;
                case MetaHeuristicAlgo.HillClimbing:
                    _metaheuristicAlgorithm = new HillClimb<Dictionary<string, float>>();
                    break;
            }

            var config = new Configuration<Dictionary<string, float>>();
            config.initializeSolutionFunction = GetInitialState;
            config.cloneFunction = Clone;
            config.mutationFunction = Mutate;
            config.movement = Search.Direction.Optimization;
            config.selectionFunction = Selection.RankBased;
            config.objectiveFunction = GetFitness;
            config.populationSize = populationSize;
            config.noOfIterations = iterations;
            config.writeToConsole = false;
            config.enforceHardObjective = false;
            _metaheuristicAlgorithm.Create(config);

            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            Dictionary<string, float> bestIndividual = null;

            for (int i = 0; i < iterations; i++)
            {
                bestIndividual = _metaheuristicAlgorithm.SingleIteration();
                Debug.Log($"Iteration = {i}, Best Fitness = {1f - _metaheuristicAlgorithm.GetBestFitness()}");

                StringBuilder strbldr = new StringBuilder();
                foreach (KeyValuePair<string, float> pair in bestIndividual)
                {
                    strbldr.Append($"{pair.Key} = {pair.Value}\n");
                }
                Debug.Log(strbldr);

                yield return null;
            }

            SetDecimation(polycountTarget, bestIndividual);
        }

        private double GetFitness(Dictionary<string, float> values)
        {
            SetDecimation(polycountTarget, values);

            Texture2D textureDecimated = CalibrationUtils.CaptureScreenshot(_camera, 1000, 1000);
            Digest modified = ImagePhash.ComputeDigest(CalibrationUtils.ToLuminanceImage(textureDecimated));

            return 1f - ImagePhash.GetCrossCorrelation(_originalDigest, modified);
        }

        private Dictionary<string, float> GetInitialState()
        {
            var output = new Dictionary<string, float>();
            output["NormalWeight"] = Random.Range(0f, 100f);
            //output["MergeThreshold"] = Random.Range(0.00001f, 0.1f);
            output["MergeNormalsThreshold"] = Random.Range(5f, 140f) * MathF.PI / 180f;
            output["UseEdgeLength"] = Random.Range(0f, 1f);
            //output["UpdateFarNeighbors"] = Random.Range(0f, 0.75f);
            //output["UpdateMinsOnCollapse"] = Random.Range(0.25f, 1f);
            output["EdgeBorderPenalty"] = Random.Range(0f, 1000f);
            return output;
        }

        private Dictionary<string, float> Clone(Dictionary<string, float> values)
        {
            return values.ToDictionary(y => y.Key, y => y.Value);
        }

        private unsafe Dictionary<string, float> Mutate(Dictionary<string, float> values)
        {
            Dictionary<string, float> output = Clone(values);
            for (int i = 0; i < mutationsPerIteration; i++)
            {
                var pair = output.ElementAt(Random.Range(0, values.Count));
                output[pair.Key] *= Random.Range(1f / 10f, 10f);
            }

            //float f = pair.Value;
            //float* fp = &f;
            //int* ip = (int*)fp;
            //int i = ip[0];
            //i ^= (1 << Random.Range(0, 32));
            //ip = &i;
            //fp = (float*)ip;
            //f = fp[0];
            //output[pair.Key] = f;
            return output;
        }

        private Dictionary<string, float> Crossover(Dictionary<string, float> sol1, Dictionary<string, float> sol2)
        {
            Dictionary<string, float> output = new Dictionary<string, float>();
            foreach (var pair in sol1)
            {
                output[pair.Key] = (pair.Value + sol2[pair.Key]) / 2;
            }
            return output;
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
                connectedMesh.MergePositions(0.01f/*variables["MergeThreshold"]*/);
                connectedMesh.MergeAttributes();
                connectedMesh.Compact();
            }

            DecimateModifier.MergeNormalsThreshold = variables["MergeNormalsThreshold"];
            //DecimateModifier.UpdateFarNeighbors = variables["UpdateFarNeighbors"] > 0.5;
            //DecimateModifier.UpdateMinsOnCollapse = variables["UpdateMinsOnCollapse"] > 0.5;
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