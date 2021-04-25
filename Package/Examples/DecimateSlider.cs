using System.Collections.Generic;
using System.Linq;
using Nanolod;
using Nanomesh;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class DecimateSlider : MonoBehaviour
{
    public GameObject original;

    private Slider _slider;
    private List<Mesh> _originalMeshes;
    private List<Mesh> _meshes;

    void Start()
    {
        _originalMeshes = new List<Mesh>();
        _meshes = new List<Mesh>();

        _slider = GetComponent<Slider>();
        _slider.onValueChanged.AddListener(OnValueChanged);

        Renderer[] renderers = original.GetComponentsInChildren<Renderer>();

        HashSet<Mesh> uniqueMeshes = new HashSet<Mesh>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer is MeshRenderer meshRenderer)
            {
                MeshFilter meshFilter = renderer.gameObject.GetComponent<MeshFilter>();
                if (meshFilter)
                {
                    Mesh mesh = meshFilter.sharedMesh;
                    if (uniqueMeshes.Add(mesh))
                    {
                        _originalMeshes.Add(mesh);
                        // Clone mesh
                        mesh = meshFilter.sharedMesh = UnityConverter.ToSharedMesh(mesh).ToUnityMesh();
                        _meshes.Add(mesh);
                    }
                }
            }
            else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                Mesh mesh = skinnedMeshRenderer.sharedMesh;
                if (uniqueMeshes.Add(mesh))
                {
                    _originalMeshes.Add(mesh);
                    // Clone mesh
                    mesh = skinnedMeshRenderer.sharedMesh = UnityConverter.ToSharedMesh(mesh).ToUnityMesh();
                    _meshes.Add(mesh);
                }
            }
        }
    }

    void OnValueChanged(float value)
    {
        Profiling.Start("Convert");

        var connectedMeshes = _originalMeshes.Select(x => UnityConverter.ToSharedMesh(x).ToConnectedMesh()).ToArray();

        Debug.Log(Profiling.End("Convert"));
        Profiling.Start("Clean");

        foreach (var connectedMesh in connectedMeshes)
        {
            // Important step :
            // We merge positions to increase chances of having correct topology information
            // We merge attributes in order to make interpolation properly operate on every face
            connectedMesh.MergePositions(0);
            connectedMesh.MergeAttributes();
            connectedMesh.Compact();
        }

        Debug.Log(Profiling.End("Clean"));
        Profiling.Start("Decimate");

        SceneDecimator sceneDecimator = new SceneDecimator();
        sceneDecimator.Initialize(connectedMeshes);

        sceneDecimator.DecimateToRatio(value);

        Debug.Log(Profiling.End("Decimate"));
        Profiling.Start("Convert back");

        for (int i = 0; i < connectedMeshes.Length; i++)
        {
            _meshes[i].Clear();
            connectedMeshes[i].ToSharedMesh().ToUnityMesh(_meshes[i]);
            _meshes[i].bindposes = _originalMeshes[i].bindposes;
        }

        Debug.Log(Profiling.End("Convert back"));
    }
}