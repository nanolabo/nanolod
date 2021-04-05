using Nanomesh;
using System.Collections.Generic;
using UnityEngine;
using NBoneWeight = Nanomesh.BoneWeight;
using NVector2F = Nanomesh.Vector2F;
using NVector3 = Nanomesh.Vector3;
using NVector3F = Nanomesh.Vector3F;
using UBoneWeight = UnityEngine.BoneWeight;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;

namespace Nanolod
{
    public static class UnityConverter
    {
        public static SharedMesh ToSharedMesh(this Mesh mesh)
        {
            UVector3[] vertices = mesh.vertices;

            SharedMesh sharedMesh = new SharedMesh();

            sharedMesh.positions = new NVector3[vertices.Length];

            MetaAttributeList attributes = new EmptyMetaAttributeList(vertices.Length);

            for (int i = 0; i < vertices.Length; i++)
            {
                sharedMesh.positions[i] = new NVector3(vertices[i].x, vertices[i].y, vertices[i].z);
            }

            List<AttributeDefinition> attributeDefinitions = new List<AttributeDefinition>();

            UVector3[] normals = mesh.normals;
            if (normals != null && normals.Length > 0)
            {
                int k = attributeDefinitions.Count;
                attributeDefinitions.Add(new AttributeDefinition(AttributeType.Normals));
                attributes = attributes.AddAttributeType<NVector3F>();
                for (int i = 0; i < normals.Length; i++)
                {
                    attributes[i] = attributes[i].Set(k, new NVector3F(normals[i].x, normals[i].y, normals[i].z));
                }
            }

            UVector2[] uvs = mesh.uv;
            if (uvs != null && uvs.Length > 0)
            {
                int k = attributeDefinitions.Count;
                attributeDefinitions.Add(new AttributeDefinition(AttributeType.UVs));
                attributes = attributes.AddAttributeType<NVector2F>();
                for (int i = 0; i < uvs.Length; i++)
                {
                    attributes[i] = attributes[i].Set(k, new NVector2F(normals[i].x, normals[i].y));
                }
            }

            UBoneWeight[] boneWeights = mesh.boneWeights;
            if (boneWeights != null && boneWeights.Length > 0)
            {
                int k = attributeDefinitions.Count;
                attributeDefinitions.Add(new AttributeDefinition(AttributeType.BoneWeights));
                attributes = attributes.AddAttributeType<NBoneWeight>();
                for (int i = 0; i < boneWeights.Length; i++)
                {
                    attributes[i] = attributes[i].Set(k, new NBoneWeight(
                        boneWeights[i].boneIndex0,
                        boneWeights[i].boneIndex1,
                        boneWeights[i].boneIndex2,
                        boneWeights[i].boneIndex3,
                        boneWeights[i].weight0,
                        boneWeights[i].weight1,
                        boneWeights[i].weight2,
                        boneWeights[i].weight3));
                }
            }

            sharedMesh.attributeDefinitions = attributeDefinitions.ToArray();
            sharedMesh.attributes = attributes;

            sharedMesh.triangles = mesh.triangles;
            sharedMesh.groups = new Group[mesh.subMeshCount];

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                UnityEngine.Rendering.SubMeshDescriptor submeshDesc = mesh.GetSubMesh(i);
                sharedMesh.groups[i] = new Group { firstIndex = submeshDesc.indexStart, indexCount = submeshDesc.indexCount };
            }

            return sharedMesh;
        }

        public static Mesh ToUnityMesh(this SharedMesh sharedMesh)
        {
            Mesh newMesh = new Mesh();
            sharedMesh.ToUnityMesh(newMesh);
            return newMesh;
        }

        public static void ToUnityMesh(this SharedMesh sharedMesh, Mesh mesh)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            UVector3[] vertices = new UVector3[sharedMesh.positions.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new UVector3((float)sharedMesh.positions[i].x, (float)sharedMesh.positions[i].y, (float)sharedMesh.positions[i].z);
            }

            mesh.vertices = vertices;

            if (sharedMesh.attributes != null)
            {
                for (int i = 0; i < sharedMesh.attributeDefinitions.Length; i++)
                {
                    if (sharedMesh.attributeDefinitions[i].type == AttributeType.Normals)
                    {
                        UVector3[] normals = new UVector3[sharedMesh.positions.Length];
                        for (int j = 0; j < sharedMesh.attributes.Count; j++)
                        {
                            NVector3F normal = sharedMesh.attributes[j].Get<NVector3F>(i);
                            normals[j] = new UVector3(normal.x, normal.y, normal.z);
                        }
                        mesh.normals = normals;
                    }
                    else if (sharedMesh.attributeDefinitions[i].type == AttributeType.UVs)
                    {
                        UVector2[] uvs = new UVector2[sharedMesh.positions.Length];
                        for (int j = 0; j < sharedMesh.attributes.Count; j++)
                        {
                            NVector2F uv = sharedMesh.attributes[j].Get<NVector2F>(i);
                            uvs[j] = new UVector2(uv.x, uv.y);
                        }
                        mesh.uv = uvs;
                    }
                    else if (sharedMesh.attributeDefinitions[i].type == AttributeType.BoneWeights)
                    {
                        UBoneWeight[] boneWeights = new UBoneWeight[sharedMesh.positions.Length];
                        for (int j = 0; j < sharedMesh.attributes.Count; j++)
                        {
                            NBoneWeight boneWeight = sharedMesh.attributes[j].Get<NBoneWeight>(i);
                            boneWeights[j] = new UBoneWeight
                            {
                                boneIndex0 = boneWeight.index0,
                                boneIndex1 = boneWeight.index1,
                                boneIndex2 = boneWeight.index2,
                                boneIndex3 = boneWeight.index3,
                                weight0 = boneWeight.weight0,
                                weight1 = boneWeight.weight1,
                                weight2 = boneWeight.weight2,
                                weight3 = boneWeight.weight3,
                            };
                        }
                        mesh.boneWeights = boneWeights;
                    }
                }
            }

            mesh.triangles = sharedMesh.triangles;
            mesh.subMeshCount = sharedMesh.groups.Length;

            for (int i = 0; i < sharedMesh.groups.Length; i++)
            {
                mesh.SetSubMesh(i, new UnityEngine.Rendering.SubMeshDescriptor(sharedMesh.groups[i].firstIndex, sharedMesh.groups[i].indexCount));
            }
        }
    }
}