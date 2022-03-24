using System;
using CipherDuo.Utility;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CipherDuo.Ethereum;
using CipherDuo.Ethereum.Modules;
using CipherDuo.Ethereum.Constants;
using CipherDuo.IPFS.Modules;
using CipherDuo.IPFS.Utility;
using Nethereum.Hex.HexTypes;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

[ExecuteAlways, RequireComponent(typeof(EVMService)),RequireComponent(typeof(IPFSService))]
public class C2Benchmark : MonoBehaviour
{
    private EVMService eth => GetComponent<EVMService>();
    private IPFSService ipfs => GetComponent<IPFSService>();
    
    [ShowInInspector] private List<Texture> textures = new List<Texture>();

    //[ShowInInspector] private Dictionary<int, C2User> users = new Dictionary<int, C2User>();

    private void Start()
    {
        ipfs.Init();
    }



    [TitleGroup("BOTTEGA"), PropertySpace]
    [Button]
    public async Task CreateBottega(int walletIndex)
    {
        //await ETHBottega.CreateBottega(users.users[walletIndex].account);
    }
    
    [Button]
    public Task GetBottega(int walletIndex)
    {
        //TODO update smart contracts (missing function/hard to catch)
        return null;
    }

    [TitleGroup("DIGITALASSETS"), PropertySpace]
    [Button]
    public async Task CreateDigitalAsset(int walletIndex, Texture2D image)
    {
        var textureOnIPFS = await UnityWrite.WriteTexture(image);

        DigitalAsset DA = new DigitalAsset();
        DA.Amount = new HexBigInteger(Random.Range(1, 100));
        DA.AssetName = "test asset name";
        DA.DagNode = "test dagnode"; //textureOnIPFS.Hash.Digest.ByteArrayToString();
        DA.License = new DigitalAssetLicense { canBeListed = true, canBorrow = true, canEdit = true };
        DA.MainTag = "test tag";
        DA.SoloIntent = new HexBigInteger(Random.Range(1, 100));
        DA.State =  0;
        DA.Voted = false;

        //await ETHDigitalAsset.CreateDigitalAsset(eth.users[walletIndex].account, DA, new HexBigInteger(ETHUtility.GetEpochTimeNow()));
    }

    [Button]
    public async Task GetDigitalAsset(int walletIndex)
    {
        var assets = await ETHDigitalAssetEvents.GetDigitalAsset();

        var dagNode = assets[walletIndex].Event.dagNode;

        Debug.Log(dagNode);
        
         var cid = IPFSUtility.CalculateHash(dagNode);
         var encodedCid = cid.Encode();
         Debug.Log(encodedCid);
         Debug.Log(cid);
        
         var fileOnIPFS = await UnityRead.ReadTexture(cid);
         Debug.Log(fileOnIPFS.GetNativeTexturePtr());
        
        textures.Add(fileOnIPFS);
    }

}



