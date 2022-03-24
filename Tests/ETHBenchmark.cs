using CipherDuo.Utility;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using CipherDuo.Ethereum;
using CipherDuo.Ethereum.Modules;
using CipherDuo.Ethereum.Constants;
using System.Text;
using CipherDuo.IPFS.Modules;
using CipherDuo.IPFS.Utility;
using NBitcoin;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3.Accounts;

public class ETHBenchmark : MonoBehaviour
{
    List<Texture2D> textures = new List<Texture2D>();

    List<Mesh> meshes = new List<Mesh>();

    List<Account> accounts = new List<Account>();
    
    bool engineStatus { get { return isEngineON; } set { value = isEngineON; } }

    bool isEngineON = false;

    private int count;

    private void Start()
    {
        StartIPFS();
    }

    private void OnDisable()
    {
        StopIPFS();
    }

    private void StartIPFS()
    {
        //ipfs.Start("CipherDuo", IPFSNetworks.CustomIPFS);
        isEngineON = true;
    }

    private void StopIPFS()
    {
        //ipfs.Stop();
    }

    public void ConnectToPubSub(string topic = "ConsoleChat")
    {
        IPFSPubSub.SubribeToTopic(topic);
    }

    public void WriteToPubSub(string topic, string msg)
    {
        var bytes = Encoding.ASCII.GetBytes(msg);
        IPFSPubSub.WriteToTopic(topic, bytes);
    }

    public async void CreateWallet(string password)
    {
        await CreateWallet(password, count+1);
    }
    
    public async Task CreateWallet(string password, int index)
    {
        var mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
        var account = await WalletService.SetWallet(mnemonic, password, index);
        accounts.Add(account);
        count += 1;
    }

    public async void SendTransaction()
    {
        await accounts[0].TransactionManager
            .SendTransactionAsync(accounts[0].PublicKey, accounts[0].PublicKey, new HexBigInteger((BigInteger) 0.001f));
    }

    public async Task UnlockWallet(string password, int index)
    {
        var account = await WalletService.GetWallet(password, index);
        accounts.Add(account);
    }

    // public async Task CreateUser(int walletIndex, C2User user)
    // {
    //     var cid = await UnityWrite.WriteString(user.NickName);
    //     //var peerId = await ipfs.GetRelayPeer();
    //     await ETHUser.CreateUser(accounts[walletIndex], cid.Hash.Digest, user.NickName, peerId.Id.Digest);
    // }

    public async Task CreateBottega(int walletIndex)
    {
        await ETHBottega.CreateBottega(accounts[walletIndex]);
    }

    public async Task CreateDigitalAsset(int walletIndex, Texture2D image)
    {
        var textureOnIPFS = await UnityWrite.WriteTexture(image);

        DigitalAsset DA = new DigitalAsset();
        DA.Amount = new HexBigInteger(1);
        DA.AssetName = "FirstAsset";
        DA.DagNode = textureOnIPFS.Hash.Digest.ByteArrayToString();
        DA.License = new DigitalAssetLicense { canBeListed = true, canBorrow = true, canEdit = true };
        DA.MainTag = "Hello World!";
        DA.SoloIntent = new HexBigInteger(1);
        DA.State =  0;
        DA.Voted = false;

        await ETHDigitalAsset.CreateDigitalAsset(accounts[walletIndex], DA, new HexBigInteger(ETHUtility.GetEpochTimeNow()));
    }


    public async Task GetDigitalAsset(int index)
    {
        var assets = await ETHDigitalAssetEvents.GetDigitalAsset();

        var dagNode = assets[index].Event.dagNode;

        var cid = IPFSUtility.CalculateHash(dagNode);
        var encodedCid = cid.Encode();
        Debug.Log(dagNode);
        Debug.Log(encodedCid);
        Debug.Log(cid);
        var fileOnIPFS = await UnityRead.ReadTexture(cid);
        textures.Add(fileOnIPFS);
    }
}



