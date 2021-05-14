using Nanomesh;
using System.Collections.Generic;
using UnityEngine;
using NBoneWeight = Nanomesh.BoneWeight;
using NColor32 = Nanomesh.Color32;
using NVector2F = Nanomesh.Vector2F;
using NVector3 = Nanomesh.Vector3;
using NVector3F = Nanomesh.Vector3F;
using UBoneWeight = UnityEngine.BoneWeight;
using UColor32 = UnityEngine.Color32;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;

namespace Nanolod
{
    public static class UnityConverter
    {
        static UnityConverter()
        {
#if DEBUG
            System.Console.SetOut(new UnityDebugWriter());
#endif
        }

        public static SharedMesh ToSharedMesh(this Mesh mesh)
        {
            System.Console.WriteLine($"Vertex count:{mesh.vertexCount}");

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

            UVector2[] uvs0 = mesh.uv;
            if (uvs0 != null && uvs0.Length > 0)
            {
                int k = attributeDefinitions.Count;
                attributeDefinitions.Add(new AttributeDefinition(AttributeType.UVs));
                attributes = attributes.AddAttributeType<NVector2F>();
                for (int i = 0; i < uvs0.Length; i++)
                {
                    attributes[i] = attributes[i].Set(k, new NVector2F(uvs0[i].x, uvs0[i].y));
                }
            }

            UVector2[] uvs2 = mesh.uv2;
            if (uvs2 != null && uvs2.Length > 0)
            {
                int k = attributeDefinitions.Count;
                attributeDefinitions.Add(new AttributeDefinition(AttributeType.UVs));
                attributes = attributes.AddAttributeType<NVector2F>();
                for (int i = 0; i < uvs2.Length; i++)
                {
                    attributes[i] = attributes[i].Set(k, new NVector2F(uvs2[i].x, uvs2[i].y));
                }
            }

            /*
            UVector2[] uvs3 = mesh.uv3;
            if (uvs3 != null && uvs3.Length > 0)
            {
                int k = attributeDefinitions.Count;
                attributeDefinitions.Add(new AttributeDefinition(AttributeType.UVs));
                attributes = attributes.AddAttributeType<NVector2F>();
                for (int i = 0; i < uvs3.Length; i++)
                {
                    attributes[i] = attributes[i].Set(k, new NVector2F(uvs3[i].x, uvs3[i].y));
                }
            }

            UVector2[] uvs4 = mesh.uv4;
            if (uvs4 != null && uvs4.Length > 0)
            {
                int k = attributeDefinitions.Count;
                attributeDefinitions.Add(new AttributeDefinition(AttributeType.UVs));
                attributes = attributes.AddAttributeType<NVector2F>();
                for (int i = 0; i < uvs4.Length; i++)
                {
                    attributes[i] = attributes[i].Set(k, new NVector2F(uvs4[i].x, uvs4[i].y));
                }
            }

            UVector2[] uvs5 = mesh.uv5;
            if (uvs5 != null && uvs5.Length > 0)
            {
                int k = attributeDefinitions.Count;
                attributeDefinitions.Add(new AttributeDefinition(AttributeType.UVs));
                attributes = attributes.AddAttributeType<NVector2F>();
                for (int i = 0; i < uvs5.Length; i++)
                {
                    attributes[i] = attributes[i].Set(k, new NVector2F(uvs5[i].x, uvs5[i].y));
                }
            }

            UVector2[] uvs6 = mesh.uv6;
            if (uvs6 != null && uvs6.Length > 0)
            {
                int k = attributeDefinitions.Count;
                attributeDefinitions.Add(new AttributeDefinition(AttributeType.UVs));
                attributes = attributes.AddAttributeType<NVector2F>();
                for (int i = 0; i < uvs6.Length; i++)
                {
                    attributes[i] = attributes[i].Set(k, new NVector2F(uvs6[i].x, uvs6[i].y));
                }
            }

            UVector2[] uvs7 = mesh.uv7;
            if (uvs7 != null && uvs7.Length > 0)
            {
                int k = attributeDefinitions.Count;
                attributeDefinitions.Add(new AttributeDefinition(AttributeType.UVs));
                attributes = attributes.AddAttributeType<NVector2F>();
                for (int i = 0; i < uvs7.Length; i++)
                {
                    attributes[i] = attributes[i].Set(k, new NVector2F(uvs7[i].x, uvs7[i].y));
                }
            }

            UVector2[] uvs8 = mesh.uv8;
            if (uvs8 != null && uvs8.Length > 0)
            {
                int k = attributeDefinitions.Count;
                attributeDefinitions.Add(new AttributeDefinition(AttributeType.UVs));
                attributes = attributes.AddAttributeType<NVector2F>();
                for (int i = 0; i < uvs8.Length; i++)
                {
                    attributes[i] = attributes[i].Set(k, new NVector2F(uvs8[i].x, uvs8[i].y));
                }
            }
            */

            UColor32[] colors = mesh.colors32;
            if (colors != null && colors.Length > 0)
            {
                int k = attributeDefinitions.Count;
                attributeDefinitions.Add(new AttributeDefinition(AttributeType.Colors, 0.0001));
                attributes = attributes.AddAttributeType<NColor32>();
                for (int i = 0; i < colors.Length; i++)
                {
                    attributes[i] = attributes[i].Set(k, new NColor32(colors[i].r, colors[i].g, colors[i].b, colors[i].a));
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
            mesh.Clear();

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

            mesh.RecalculateTangents();
        }
    }
}