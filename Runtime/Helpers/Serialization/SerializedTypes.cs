using UnityEngine;

namespace CipherDuo.IPFS.Serialization
{
    
    //SCENE
    
    public struct ScenePacket
    {
        public string[] gameObjects;
        public string[] scales;
        public string[] positions;
    }
    
    
    //GAMEOBJECT TODO abstract complex data structures
    
    public struct GameObjectPacket
    {
        public string mesh;
        public string material;
        public string rotation;
    }

    //TODO create library based on type for simpler structs
    public struct Vector4Packet
    {
        public byte[] x;
        public byte[] y;
        public byte[] z;
        public byte[] w;
    }
    public struct Vector3Packet
    {
        public byte[] x;
        public byte[] y;
        public byte[] z;
    }
    
    
    //MULTIPLAYER
    
    public struct MultiplayerPacket
    {
        public int id;
        public byte[] x;
        public byte[] y;
    }

    
    //MATERIAL
    
    public struct MaterialMetadata
    {
        public string hash;
        public byte[] color;
    }

    public struct MaterialFile
    {
        public byte[] content;
    }

    
    //MESH

    public struct MeshMetadata
    {
        public string[] hashes;
        public float[] pivot;
        public float[] bounds;
    }

    public struct MeshFile
    {
        public byte[] triangles;
        public byte[] verts;
        public byte[] normals;
        public byte[] uv0;
        public byte[] uv1;
    }


    // IMAGE 

    public struct TextureMetadata
    {
        public string hash;
        public int heigth;
        public int width;
        public TextureFormat format;
    }

    public struct TextureFile
    {
        public byte[] content;
    }


    // STRING

    public struct StringMetadata
    {
        public string hash;
    }
    
    public struct StringFile
    {
        public byte[] content;
    }
}