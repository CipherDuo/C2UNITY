using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CipherDuo.IPFS.Logger;
using UnityEngine;
using Newtonsoft.Json;

namespace CipherDuo.IPFS.Serialization
{
    public static class Serializator
    {
        private static ILog logger = LoggerFactory.GetLogger(nameof(Serializator));
        
        public static string SerializeJson<T>(T packet)
        {
            return JsonConvert.SerializeObject(packet);
        }

        public static T DeserializeJson<T>(Stream serPacket)
        {
            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(serPacket))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }

        public static byte[] SerializeXML<T>(T packet)
        {
            using (MemoryStream memoryStream = new MemoryStream())
                try
                {
                    new XmlSerializer(typeof(T)).Serialize(memoryStream, packet);
                    return memoryStream.ToArray();
                }
                catch (Exception error)
                {
                    logger.Log(error.Message);
                    return null;
                }
        }

        public static T DeserializeXML<T>(byte[] serPacket)
        {
            using (MemoryStream stream = new MemoryStream(serPacket))
                try
                {
                    var packet = (T) new XmlSerializer(typeof(T)).Deserialize(stream);
                    return packet;
                }
                catch (Exception error)
                {
                    logger.Log(error.Message);
                    return default;
                }

        }

        public static MaterialMetadata Metadata(Material material)
        {
            var metMaterial = new MaterialMetadata();
            var color = material.color;

            List<byte> colBytes = new List<byte>(16);

            colBytes.AddRange(BitConverter.GetBytes(color.r));
            colBytes.AddRange(BitConverter.GetBytes(color.g));
            colBytes.AddRange(BitConverter.GetBytes(color.b));
            colBytes.AddRange(BitConverter.GetBytes(color.a));

            metMaterial.color = colBytes.ToArray();

            return metMaterial;
        }

        public static MaterialFile Serialize(Material material)
        {
            MaterialFile serMaterial = new MaterialFile();
            
            Texture texture = material.mainTexture;
            Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
  
            RenderTexture currentRT = RenderTexture.active;
  
            RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 32);
            Graphics.Blit(texture, renderTexture);
  
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
  
            RenderTexture.active = currentRT;
            
            serMaterial.content = texture2D.EncodeToJPG();

            return serMaterial;
        }

        public static Material Deserialize(byte[] data, MaterialMetadata serMaterial)
        {
            Material material = new Material(Shader.Find("Standard"));
            byte[] color = serMaterial.color;
            
            int index = 0;
            Vector4 col = new Vector4();
            col.x = BitConverter.ToSingle(color, index);
            index += 4;
            col.y = BitConverter.ToSingle(color, index);
            index += 4;
            col.z = BitConverter.ToSingle(color, index);
            index += 4;
            col.w = BitConverter.ToSingle(color, index);
            index += 4;

            material.color = col;
            var mainTexture = new Texture2D(1, 1);
            ImageConversion.LoadImage(mainTexture, data);
            material.mainTexture = mainTexture;

            return material;
        }

        // MESH
        public static MeshMetadata Metadata(Mesh mesh)
        {
            var metMesh = new MeshMetadata();

            metMesh.bounds = new float[] {mesh.bounds.size.x, mesh.bounds.size.y, mesh.bounds.size.z};
            metMesh.pivot = new float[] {mesh.bounds.center.x, mesh.bounds.center.y, mesh.bounds.center.z};

            return metMesh;
        }

        public static MeshFile Serialize(Mesh mesh)
        {


            var serMesh = new MeshFile();

            List<byte> vertBytes = new List<byte>(12 * mesh.vertices.Length);
            foreach (Vector3 vert in mesh.vertices)
            {
                vertBytes.AddRange(BitConverter.GetBytes(vert.x));
                vertBytes.AddRange(BitConverter.GetBytes(vert.y));
                vertBytes.AddRange(BitConverter.GetBytes(vert.z));
            }

            List<byte> normBytes = new List<byte>(12 * mesh.normals.Length);
            foreach (Vector3 normal in mesh.normals)
            {
                normBytes.AddRange(BitConverter.GetBytes(normal.x));
                normBytes.AddRange(BitConverter.GetBytes(normal.y));
                normBytes.AddRange(BitConverter.GetBytes(normal.z));
            }

            List<byte> uvBytes = new List<byte>(8 * mesh.uv.Length);
            foreach (Vector2 uv in mesh.uv)
            {
                uvBytes.AddRange(BitConverter.GetBytes(uv.x));
                uvBytes.AddRange(BitConverter.GetBytes(uv.y));
            }

            List<byte> uv1Bytes = new List<byte>(8 * mesh.uv2.Length);
            foreach (Vector2 uv1 in mesh.uv2)
            {
                uv1Bytes.AddRange(BitConverter.GetBytes(uv1.x));
                uv1Bytes.AddRange(BitConverter.GetBytes(uv1.y));
            }

            
            serMesh.triangles = Conversion.IntArrayToByteArray(mesh.triangles);
            serMesh.verts = vertBytes.ToArray();
            serMesh.normals = normBytes.ToArray();
            serMesh.uv0 = uvBytes.ToArray();
            serMesh.uv1 = uv1Bytes.ToArray();

            return serMesh;
        }

        public static Mesh Deserialize(MeshFile serMesh)
        {
            Mesh newmesh = new Mesh();
            int index = 0;

            List<Vector3> newVerts = new List<Vector3>();
            for (int i = 0; i < serMesh.verts.Length / 12; i++)
            {
                Vector3 vert = new Vector3();
                vert.x = BitConverter.ToSingle(serMesh.verts, index);
                index += 4;
                vert.y = BitConverter.ToSingle(serMesh.verts, index);
                index += 4;
                vert.z = BitConverter.ToSingle(serMesh.verts, index);
                index += 4;

                newVerts.Add(vert);
            }

            index = 0;
            List<Vector3> newNormals = new List<Vector3>();
            for (int i = 0; i < serMesh.normals.Length / 12; i++)
            {
                Vector3 normal = new Vector3();
                normal.x = BitConverter.ToSingle(serMesh.normals, index);
                index += 4;
                normal.y = BitConverter.ToSingle(serMesh.normals, index);
                index += 4;
                normal.z = BitConverter.ToSingle(serMesh.normals, index);
                index += 4;

                newNormals.Add(normal);
            }

            index = 0;
            List<Vector2> newUV = new List<Vector2>();
            for (int i = 0; i < serMesh.uv0.Length / 8; i++)
            {
                Vector2 uv = new Vector2();
                uv.x = BitConverter.ToSingle(serMesh.uv0, index);
                index += 4;
                uv.y = BitConverter.ToSingle(serMesh.uv0, index);
                index += 4;

                newUV.Add(uv);
            }

            index = 0;
            List<Vector2> newUV1 = new List<Vector2>();
            for (int i = 0; i < serMesh.uv1.Length / 8; i++)
            {
                Vector2 uv1 = new Vector2();
                uv1.x = BitConverter.ToSingle(serMesh.uv1, index);
                index += 4;
                uv1.y = BitConverter.ToSingle(serMesh.uv1, index);
                index += 4;

                newUV1.Add(uv1);
            }

            newmesh.vertices = newVerts.ToArray();
            newmesh.normals = newNormals.ToArray();
            newmesh.uv = newUV.ToArray();
            newmesh.uv2 = newUV1.ToArray();
            newmesh.triangles = Conversion.byteArrayToIntArray(serMesh.triangles);
            return newmesh;
        }


        // IMAGE
        public static TextureMetadata Metadata(Texture2D tex)
        {
            var metTexture = new TextureMetadata();

            metTexture.heigth = tex.height;
            metTexture.width = tex.width;
            metTexture.format = tex.format;

            return metTexture;
        }

        public static TextureFile Serialize(Texture2D tex, bool useAlpha = false)
        {
            var serTexture = new TextureFile();
            
            serTexture.content = useAlpha ? tex.EncodeToPNG() : tex.EncodeToJPG();

            return serTexture;
        }

        public static Texture2D Deserialize(byte[] data, TextureMetadata serTexture)
        {
            Texture2D tex = new Texture2D(serTexture.heigth, serTexture.width, serTexture.format, false);

            ImageConversion.LoadImage(tex, data);

            return tex;
        }


        // STRING
        public static StringMetadata Metadata(string msg)
        {
            var metString = new StringMetadata();

            return metString;
        }

        public static StringFile Serialize(string msg)
        {
            var serString = new StringFile();

            serString.content = Encoding.UTF7.GetBytes(msg);

            return serString;
        }

        public static string Deserialize(byte[] data)
        {
            string str = Encoding.UTF7.GetString(data);

            return str;
        }
    }
    
    
    // BUNNY83 MeshSerializer

 
    [System.Serializable]
    public class MeshData
    {
        [SerializeField, HideInInspector]
        private byte[] m_Data;
        private Mesh m_Mesh;
        public byte[] Data { get{ return m_Data; } }
        public void SetMesh(Mesh aMesh)
        {
            m_Mesh = aMesh;
            if (aMesh == null)
                m_Data = null;
            else
                m_Data = MeshSerializer.SerializeMesh(m_Mesh);
        }
        public Mesh GetMesh()
        {
            if (m_Mesh == null && m_Data != null)
                m_Mesh = MeshSerializer.DeserializeMesh(m_Data);
            return m_Mesh;
        }
    }
 
 
    public static class MeshSerializer
    {
        /*
         * Structure:
         * - Magic string "Mesh" (4 bytes)
         * - vertex count [int] (4 bytes)
         * - submesh count [int] (4 bytes)
         * - vertices [array of Vector3]
         * 
         * - additional chunks:
         *   [vertex attributes]
         *   - Name (name of the Mesh object)
         *   - Normals [array of Vector3]
         *   - Tangents [array of Vector4]
         *   - Colors [array of Color32]
         *   - UV0-4 [
         *       - component count[byte](2/3/4)
         *       - array of Vector2/3/4
         *     ]
         *   - BoneWeights [array of 4x int+float pair]
         *   
         *   [other data]
         *   - Submesh [
         *       - topology[byte]
         *       - count[int]
         *       - component size[byte](1/2/4)
         *       - array of byte/ushort/int
         *     ]
         *   - Bindposes [
         *       - count[int]
         *       - array of Matrix4x4
         *     ]
         *   - BlendShape [
         *       - Name [string]
         *       - frameCount [int]
         *       - frames [ array of:
         *           - frameWeight [float]
         *           - array of [
         *               - position delta [Vector3]
         *               - normal delta [Vector3]
         *               - tangent delta [Vector3]
         *             ]
         *         ]
         *     ]
         */
        private enum EChunkID : byte
        {
            End,
            Name,
            Normals,
            Tangents,
            Colors,
            BoneWeights,
            UV0, UV1, UV2, UV3,
            Submesh,
            Bindposes,
            BlendShape,
        }
        const uint m_Magic = 0x6873654D; // "Mesh"
 
        public static byte[] SerializeMesh(Mesh aMesh)
        {
            using (var stream = new MemoryStream())
            {
                SerializeMesh(stream, aMesh);
                return stream.ToArray();
            }
        }
        public static void SerializeMesh(MemoryStream aStream, Mesh aMesh)
        {
            using (var writer = new BinaryWriter(aStream))
                SerializeMesh(writer, aMesh);
        }
        public static void SerializeMesh(BinaryWriter aWriter, Mesh aMesh)
        {
            aWriter.Write(m_Magic);
            var vertices = aMesh.vertices;
            int count = vertices.Length;
            int subMeshCount = aMesh.subMeshCount;
            aWriter.Write(count);
            aWriter.Write(subMeshCount);
            foreach (var v in vertices)
                aWriter.WriteVector3(v);
 
            // start of tagged chunks
            if (!string.IsNullOrEmpty(aMesh.name))
            {
                aWriter.Write((byte)EChunkID.Name);
                aWriter.Write(aMesh.name);
            }
            var normals = aMesh.normals;
            if (normals != null && normals.Length == count)
            {
                aWriter.Write((byte)EChunkID.Normals);
                foreach (var v in normals)
                    aWriter.WriteVector3(v);
                normals = null;
            }
            var tangents = aMesh.tangents;
            if (tangents != null && tangents.Length == count)
            {
                aWriter.Write((byte)EChunkID.Tangents);
                foreach (var v in tangents)
                    aWriter.WriteVector4(v);
                tangents = null;
            }
            var colors = aMesh.colors32;
            if (colors != null && colors.Length == count)
            {
                aWriter.Write((byte)EChunkID.Colors);
                foreach (var c in colors)
                    aWriter.WriteColor32(c);
                colors = null;
            }
            var boneWeights = aMesh.boneWeights;
            if (boneWeights != null && boneWeights.Length == count)
            {
                aWriter.Write((byte)EChunkID.BoneWeights);
                foreach (var w in boneWeights)
                    aWriter.WriteBoneWeight(w);
                boneWeights = null;
            }
            List<Vector4> uvs = new List<Vector4>();
            for (int i = 0; i < 4; i++)
            {
                uvs.Clear();
                aMesh.GetUVs(i, uvs);
                if (uvs.Count == count)
                {
                    aWriter.Write((byte)((byte)EChunkID.UV0 + i));
                    byte channelCount = 2;
                    foreach (var uv in uvs)
                    {
                        if (uv.z != 0f)
                            channelCount = 3;
                        if (uv.w != 0f)
                        {
                            channelCount = 4;
                            break;
                        }
                    }
                    aWriter.Write(channelCount);
                    if (channelCount == 2)
                        foreach (var uv in uvs)
                            aWriter.WriteVector2(uv);
                    else if (channelCount == 3)
                        foreach (var uv in uvs)
                            aWriter.WriteVector3(uv);
                    else
                        foreach (var uv in uvs)
                            aWriter.WriteVector4(uv);
                }
            }
            List<int> indices = new List<int>(count * 3);
            for (int i = 0; i < subMeshCount; i++)
            {
                indices.Clear();
                aMesh.GetIndices(indices, i);
                if (indices.Count > 0)
                {
                    aWriter.Write((byte)EChunkID.Submesh);
                    aWriter.Write((byte)aMesh.GetTopology(i));
                    aWriter.Write(indices.Count);
                    var max = indices.Max();
                    if (max < 256)
                    {
                        aWriter.Write((byte)1);
                        foreach (var index in indices)
                            aWriter.Write((byte)index);
                    }
                    else if (max < 65536)
                    {
                        aWriter.Write((byte)2);
                        foreach (var index in indices)
                            aWriter.Write((ushort)index);
                    }
                    else
                    {
                        aWriter.Write((byte)4);
                        foreach (var index in indices)
                            aWriter.Write(index);
                    }
                }
            }
            var bindposes = aMesh.bindposes;
            if (bindposes != null && bindposes.Length > 0)
            {
                aWriter.Write((byte)EChunkID.Bindposes);
                aWriter.Write(bindposes.Length);
                foreach (var b in bindposes)
                    aWriter.WriteMatrix4x4(b);
                bindposes = null;
            }
            int blendShapeCount = aMesh.blendShapeCount;
            if (blendShapeCount > 0)
            {
                var blendVerts = new Vector3[count];
                var blendNormals = new Vector3[count];
                var blendTangents = new Vector3[count];
                for (int i = 0; i < blendShapeCount; i++)
                {
                    aWriter.Write((byte)EChunkID.BlendShape);
                    aWriter.Write(aMesh.GetBlendShapeName(i));
                    var frameCount = aMesh.GetBlendShapeFrameCount(i);
                    aWriter.Write(frameCount);
                    for (int n = 0; n < frameCount; n++)
                    {
                        aMesh.GetBlendShapeFrameVertices(i, n, blendVerts, blendNormals, blendTangents);
                        aWriter.Write(aMesh.GetBlendShapeFrameWeight(i, n));
                        for (int k = 0; k < count; k++)
                        {
                            aWriter.WriteVector3(blendVerts[k]);
                            aWriter.WriteVector3(blendNormals[k]);
                            aWriter.WriteVector3(blendTangents[k]);
                        }
                    }
                }
            }
            aWriter.Write((byte)EChunkID.End);
        }
 
 
        public static Mesh DeserializeMesh(byte[] aData, Mesh aTarget = null)
        {
            using (var stream = new MemoryStream(aData))
                return DeserializeMesh(stream, aTarget);
        }
        public static Mesh DeserializeMesh(MemoryStream aStream, Mesh aTarget = null)
        {
            using (var reader = new BinaryReader(aStream))
                return DeserializeMesh(reader, aTarget);
        }
        public static Mesh DeserializeMesh(BinaryReader aReader, Mesh aTarget = null)
        {
            if (aReader.ReadUInt32() != m_Magic)
                return null;
            if (aTarget == null)
                aTarget = new Mesh();
            aTarget.Clear();
            aTarget.ClearBlendShapes();
            int count = aReader.ReadInt32();
            if (count > 65534)
                aTarget.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            int subMeshCount = aReader.ReadInt32();
            Vector3[] vector3Array = new Vector3[count];
            Vector3[] vector3Array2 = null;
            Vector3[] vector3Array3 = null;
            List<Vector4> vector4List = null;
            for (int i = 0; i < count; i++)
                vector3Array[i] = aReader.ReadVector3();
            aTarget.vertices = vector3Array;
            aTarget.subMeshCount = subMeshCount;
            int subMeshIndex = 0;
            byte componentCount = 0;
 
            // reading chunks
            var stream = aReader.BaseStream;
            while ((stream.CanSeek && stream.Position < stream.Length) || stream.CanRead)
            {
                var chunkID = (EChunkID)aReader.ReadByte();
                if (chunkID == EChunkID.End)
                    break;
                switch (chunkID)
                {
                    case EChunkID.Name:
                        aTarget.name = aReader.ReadString();
                        break;
                    case EChunkID.Normals:
                        for (int i = 0; i < count; i++)
                            vector3Array[i] = aReader.ReadVector3();
                        aTarget.normals = vector3Array;
                        break;
                    case EChunkID.Tangents:
                        if (vector4List == null)
                            vector4List = new List<Vector4>(count);
                        vector4List.Clear();
                        for (int i = 0; i < count; i++)
                            vector4List.Add(aReader.ReadVector4());
                        aTarget.SetTangents(vector4List);
                        break;
                    case EChunkID.Colors:
                        var colors = new Color32[count];
                        for (int i = 0; i < count; i++)
                            colors[i] = aReader.ReadColor32();
                        aTarget.colors32 = colors;
                        break;
                    case EChunkID.BoneWeights:
                        var boneWeights = new BoneWeight[count];
                        for (int i = 0; i < count; i++)
                            boneWeights[i] = aReader.ReadBoneWeight();
                        aTarget.boneWeights = boneWeights;
                        break;
                    case EChunkID.UV0:
                    case EChunkID.UV1:
                    case EChunkID.UV2:
                    case EChunkID.UV3:
                        int uvChannel = chunkID - EChunkID.UV0;
                        componentCount = aReader.ReadByte();
                        if (vector4List == null)
                            vector4List = new List<Vector4>(count);
                        vector4List.Clear();
 
                        if (componentCount == 2)
                        {
                            for (int i = 0; i < count; i++)
                                vector4List.Add(aReader.ReadVector2());
                        }
                        else if (componentCount == 3)
                        {
                            for (int i = 0; i < count; i++)
                                vector4List.Add(aReader.ReadVector3());
                        }
                        else if (componentCount == 4)
                        {
                            for (int i = 0; i < count; i++)
                                vector4List.Add(aReader.ReadVector4());
                        }
                        aTarget.SetUVs(uvChannel, vector4List);
                        break;
                    case EChunkID.Submesh:
                        var topology = (MeshTopology)aReader.ReadByte();
                        int indexCount = aReader.ReadInt32();
                        var indices = new int[indexCount];
                        componentCount = aReader.ReadByte();
                        if (componentCount == 1)
                        {
                            for (int i = 0; i < indexCount; i++)
                                indices[i] = aReader.ReadByte();
                        }
                        else if (componentCount == 2)
                        {
                            for (int i = 0; i < indexCount; i++)
                                indices[i] = aReader.ReadUInt16();
                        }
                        else if (componentCount == 4)
                        {
                            for (int i = 0; i < indexCount; i++)
                                indices[i] = aReader.ReadInt32();
                        }
                        aTarget.SetIndices(indices, topology, subMeshIndex++, false);
                        break;
                    case EChunkID.Bindposes:
                        int bindposesCount = aReader.ReadInt32();
                        var bindposes = new Matrix4x4[bindposesCount];
                        for (int i = 0; i < bindposesCount; i++)
                            bindposes[i] = aReader.ReadMatrix4x4();
                        aTarget.bindposes = bindposes;
                        break;
                    case EChunkID.BlendShape:
                        var blendShapeName = aReader.ReadString();
                        int frameCount = aReader.ReadInt32();
                        if (vector3Array2 == null)
                            vector3Array2 = new Vector3[count];
                        if (vector3Array3 == null)
                            vector3Array3 = new Vector3[count];
                        for (int i = 0; i < frameCount; i++)
                        {
                            float weight = aReader.ReadSingle();
                            for (int n = 0; n < count; n++)
                            {
                                vector3Array[n] = aReader.ReadVector3();
                                vector3Array2[n] = aReader.ReadVector3();
                                vector3Array3[n] = aReader.ReadVector3();
                            }
                            aTarget.AddBlendShapeFrame(blendShapeName, weight, vector3Array, vector3Array2, vector3Array3);
                        }
                        break;
                }
            }
            return aTarget;
        }
    }
 
 
    public static class BinaryReaderWriterUnityExt
    {
        public static void WriteVector2(this BinaryWriter aWriter, Vector2 aVec)
        {
            aWriter.Write(aVec.x); aWriter.Write(aVec.y);
        }
        public static Vector2 ReadVector2(this BinaryReader aReader)
        {
            return new Vector2(aReader.ReadSingle(), aReader.ReadSingle());
        }
        public static void WriteVector3(this BinaryWriter aWriter, Vector3 aVec)
        {
            aWriter.Write(aVec.x); aWriter.Write(aVec.y); aWriter.Write(aVec.z);
        }
        public static Vector3 ReadVector3(this BinaryReader aReader)
        {
            return new Vector3(aReader.ReadSingle(), aReader.ReadSingle(), aReader.ReadSingle());
        }
        public static void WriteVector4(this BinaryWriter aWriter, Vector4 aVec)
        {
            aWriter.Write(aVec.x); aWriter.Write(aVec.y); aWriter.Write(aVec.z); aWriter.Write(aVec.w);
        }
        public static Vector4 ReadVector4(this BinaryReader aReader)
        {
            return new Vector4(aReader.ReadSingle(), aReader.ReadSingle(), aReader.ReadSingle(), aReader.ReadSingle());
        }
 
        public static void WriteColor32(this BinaryWriter aWriter, Color32 aCol)
        {
            aWriter.Write(aCol.r); aWriter.Write(aCol.g); aWriter.Write(aCol.b); aWriter.Write(aCol.a);
        }
        public static Color32 ReadColor32(this BinaryReader aReader)
        {
            return new Color32(aReader.ReadByte(), aReader.ReadByte(), aReader.ReadByte(), aReader.ReadByte());
        }
 
        public static void WriteMatrix4x4(this BinaryWriter aWriter, Matrix4x4 aMat)
        {
            aWriter.Write(aMat.m00); aWriter.Write(aMat.m01); aWriter.Write(aMat.m02); aWriter.Write(aMat.m03);
            aWriter.Write(aMat.m10); aWriter.Write(aMat.m11); aWriter.Write(aMat.m12); aWriter.Write(aMat.m13);
            aWriter.Write(aMat.m20); aWriter.Write(aMat.m21); aWriter.Write(aMat.m22); aWriter.Write(aMat.m23);
            aWriter.Write(aMat.m30); aWriter.Write(aMat.m31); aWriter.Write(aMat.m32); aWriter.Write(aMat.m33);
        }
        public static Matrix4x4 ReadMatrix4x4(this BinaryReader aReader)
        {
            var m = new Matrix4x4();
            m.m00 = aReader.ReadSingle(); m.m01 = aReader.ReadSingle(); m.m02 = aReader.ReadSingle(); m.m03 = aReader.ReadSingle();
            m.m10 = aReader.ReadSingle(); m.m11 = aReader.ReadSingle(); m.m12 = aReader.ReadSingle(); m.m13 = aReader.ReadSingle();
            m.m20 = aReader.ReadSingle(); m.m21 = aReader.ReadSingle(); m.m22 = aReader.ReadSingle(); m.m23 = aReader.ReadSingle();
            m.m30 = aReader.ReadSingle(); m.m31 = aReader.ReadSingle(); m.m32 = aReader.ReadSingle(); m.m33 = aReader.ReadSingle();
            return m;
        }
 
        public static void WriteBoneWeight(this BinaryWriter aWriter, BoneWeight aWeight)
        {
            aWriter.Write(aWeight.boneIndex0); aWriter.Write(aWeight.weight0);
            aWriter.Write(aWeight.boneIndex1); aWriter.Write(aWeight.weight1);
            aWriter.Write(aWeight.boneIndex2); aWriter.Write(aWeight.weight2);
            aWriter.Write(aWeight.boneIndex3); aWriter.Write(aWeight.weight3);
        }
        public static BoneWeight ReadBoneWeight(this BinaryReader aReader)
        {
            var w = new BoneWeight();
            w.boneIndex0 = aReader.ReadInt32(); w.weight0 = aReader.ReadSingle();
            w.boneIndex1 = aReader.ReadInt32(); w.weight1 = aReader.ReadSingle();
            w.boneIndex2 = aReader.ReadInt32(); w.weight2 = aReader.ReadSingle();
            w.boneIndex3 = aReader.ReadInt32(); w.weight3 = aReader.ReadSingle();
            return w;
        }
    }
}