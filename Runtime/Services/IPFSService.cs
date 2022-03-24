using System;
using System.Text;
using CipherDuo.IPFS;
using CipherDuo.IPFS.Constants;
using CipherDuo.IPFS.Modules;
using CipherDuo.IPFS.Utility;
using Ipfs;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
public class IPFSService : MonoBehaviour
{
    [TitleGroup("NETWORK")] [ShowInInspector, ReadOnly]
    private string _actualNode;

    private IPFSNetworks _network;

    [ShowInInspector]
    private IPFSNetworks network
    {
        get { return _network; }
        set
        {
            IPFS.Stop();
            _actualNode = null;
            _network = value;
        }
    }

    public void Init()
    {
        if (IPFS.ipfsRelay == null)
        {
            IPFS.Start($"CipherDuo_{_network}", _network);
            UnityThread.initUnityThread();
            
            _actualNode = IPFSUtility.IPFSNetworkProvider[_network];
        }

    }

    private void OnDisable()
    {
        IPFS.Stop();
    }
}