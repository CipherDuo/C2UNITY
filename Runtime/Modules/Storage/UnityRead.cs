using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CipherDuo.IPFS.Logger;
using CipherDuo.IPFS.Modules;
using CipherDuo.IPFS.Serialization;
using UnityEngine;

public static class UnityRead
{
    private static ILog logger = LoggerFactory.GetLogger(nameof(IPFSRead));

    //TODO pass a better container than transform for scene
    public static async Task<Transform> ReadScene(string cid)
    {
        ScenePacket packet = await IPFSRead.ReadDag<ScenePacket>(cid);

        Transform sceneRoot = new GameObject("Scene").transform;

        for (int i = 0; i < packet.gameObjects.Length; i++)
        {
            var go = await ReadGameObject(packet.gameObjects[i]);

            var position = await  IPFSRead.ReadDag<Vector3Packet>(packet.positions[i]);
            var positionX = BitConverter.ToSingle(position.x);
            var positionY = BitConverter.ToSingle(position.y);
            var positionZ = BitConverter.ToSingle(position.z);

            go.transform.position = new Vector3(positionX, positionY, positionZ);

            var scale = await  IPFSRead.ReadDag<Vector3Packet>(packet.scales[i]);
            var scaleX = BitConverter.ToSingle(scale.x);
            var scaleY = BitConverter.ToSingle(scale.y);
            var scaleZ = BitConverter.ToSingle(scale.z);

            go.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

            go.transform.parent = sceneRoot;
        }

        return sceneRoot;
    }

    public static async Task<GameObject> ReadGameObject(string cid)
    {
        GameObjectPacket packet = await  IPFSRead.ReadDag<GameObjectPacket>(cid);

        GameObject go = new GameObject("GameObject");
        var filter = go.AddComponent<MeshFilter>();
        var renderer = go.AddComponent<MeshRenderer>();

        filter.mesh = await ReadMesh(packet.mesh);
        renderer.material = await ReadMaterial(packet.material);

        var rotation = await  IPFSRead.ReadDag<Vector4Packet>(packet.rotation);
        var x = BitConverter.ToSingle(rotation.x);
        var y = BitConverter.ToSingle(rotation.y);
        var z = BitConverter.ToSingle(rotation.z);
        var w = BitConverter.ToSingle(rotation.w);

        go.transform.rotation = new Quaternion(x, y, z, w);

        return go;
    }

    public static async Task<Material> ReadMaterial(string cid)
    {
        MaterialMetadata serMaterial = await  IPFSRead.ReadDag<MaterialMetadata>(cid);

        var content = await  IPFSRead.ReadBytes(serMaterial.hash);

        return Serializator.Deserialize(content, serMaterial);
    }


    //TODO Uniform ANY to this standard if possible
    public static async Task<Mesh> ReadMesh(string cid)
    {
        Mesh result = MeshSerializer.DeserializeMesh(await  IPFSRead.ReadBytes(cid));
        return result;
    }

    public static async Task<Texture2D> ReadTexture(string cid)
    {
        TextureMetadata serTexture = await  IPFSRead.ReadDag<TextureMetadata>(cid);

        var content = await  IPFSRead.ReadBytes(serTexture.hash);

        return Serializator.Deserialize(content, serTexture);
    }

    public static async Task<string> ReadString(string cid)
    {
        StringMetadata serString = await  IPFSRead.ReadDag<StringMetadata>(cid);

        var content = await  IPFSRead.ReadBytes(serString.hash);

        return Serializator.Deserialize(content);
    }
}
