using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CipherDuo.IPFS.Logger;
using CipherDuo.IPFS.Modules;
using CipherDuo.IPFS.Serialization;
using Ipfs;
using UnityEngine;

public static class UnityWrite
{
    private static ILog logger = LoggerFactory.GetLogger(nameof(IPFSWrite));

    //TODO ensure this is the best solution
    public static async Task<Cid> WriteScene(GameObject sceneRootObj)
    {
        ScenePacket packet = new ScenePacket();

        var GOs = sceneRootObj.transform.GetComponentsInChildren<Transform>();
        packet.gameObjects = new string[GOs.Length - 1];
        packet.positions = new string[GOs.Length - 1];
        packet.scales = new string[GOs.Length - 1];

        // skip scene
        for (int i = 1; i < GOs.Length; i++)
        {
            var go = GOs[i];

            var goCid = await WriteGameObject(go.gameObject);
            packet.gameObjects[i - 1] = goCid;

            Vector3Packet posPacket = new Vector3Packet();
            var position = go.transform.position;

            posPacket.x = BitConverter.GetBytes(position.x);
            posPacket.y = BitConverter.GetBytes(position.y);
            posPacket.z = BitConverter.GetBytes(position.z);

            packet.positions[i - 1] = await IPFSWrite.WriteObject(posPacket);

            Vector3Packet sizePacket = new Vector3Packet();
            var scale = go.transform.lossyScale;

            sizePacket.x = BitConverter.GetBytes(scale.x);
            sizePacket.y = BitConverter.GetBytes(scale.y);
            sizePacket.z = BitConverter.GetBytes(scale.z);

            packet.scales[i - 1] = await IPFSWrite.WriteObject(sizePacket);
        }

        var dag = await IPFSWrite.WriteObject(packet);

        return dag;

    }

    public static async Task<Cid> WriteGameObject(GameObject go)
    {
        var material = go.GetComponent<MeshRenderer>().sharedMaterial;
        var mesh = go.GetComponent<MeshFilter>().sharedMesh;

        GameObjectPacket packet = new GameObjectPacket();
        packet.mesh = await WriteMesh(mesh);
        packet.material = await WriteMaterial(material);

        Vector4Packet rotPacket = new Vector4Packet();
        var rotation = go.transform.rotation;

        rotPacket.x = BitConverter.GetBytes(rotation.x);
        rotPacket.y = BitConverter.GetBytes(rotation.y);
        rotPacket.z = BitConverter.GetBytes(rotation.z);
        rotPacket.w = BitConverter.GetBytes(rotation.w);

        packet.rotation = await IPFSWrite.WriteObject(rotPacket);

        var dag = await IPFSWrite.WriteObject(packet);

        return dag;

    }

    public static async Task<Cid> WriteMaterial(Material material)
    {
        MaterialMetadata matMeta = Serializator.Metadata(material);
        MaterialFile sourceMat = Serializator.Serialize(material);

        var hash = await IPFSWrite.WriteByte(sourceMat.content);
        matMeta.hash = hash;

        var dag = await IPFSWrite.WriteObject(matMeta);

        return dag;

    }

    //TODO Uniform ANY to this standard if possible
    public static async Task<Cid> WriteMesh(Mesh model)
    {
        MeshData data = new MeshData();
        data.SetMesh(model);
        var hash = await IPFSWrite.WriteByte(data.Data);
        return hash;

    }

    public static async Task<Cid> WriteTexture(Texture2D image)
    {
        TextureMetadata texMeta = Serializator.Metadata(image);
        TextureFile sourceTex = Serializator.Serialize(image);

        var hash = await IPFSWrite.WriteByte(sourceTex.content);
        texMeta.hash = hash;

        var dag = await IPFSWrite.WriteObject(texMeta);

        return dag;

    }

    public static async Task<Cid> WriteString(string str)
    {
        StringMetadata strMeta = Serializator.Metadata(str);
        StringFile sourceStr = Serializator.Serialize(str);

        var hash = await IPFSWrite.WriteByte(sourceStr.content);
        strMeta.hash = hash;

        var dag = await IPFSWrite.WriteObject(strMeta);

        return dag;
    }
}