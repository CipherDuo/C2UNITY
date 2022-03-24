using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CipherDuo;
using UnityEngine;
using CipherDuo.Ethereum;
using CipherDuo.Ethereum.Constants;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;
using Sirenix.OdinInspector;

public class EVMService : MonoBehaviour
{
    private ETHNetworks m_network = ETHNetworks.Infura;
    private Chain m_chain = Chain.Ropsten;
    
    [TitleGroup("NETWORK")]
    [ShowInInspector] private ETHNetworks Network {
        get => m_network;
        set
        {
            m_network = value;
            SetProvider(value);
        }
    }    
    
    [ShowInInspector] private Chain Chain {
        get => m_chain;
        set
        {
            m_chain = value;
            SetChain(value);
        }
    }

    private void Start()
    {
        SetProvider(m_network);
        SetChain(m_chain);
    }

    [Button]
    public void SetProvider(ETHNetworks network)
    {
        ETH.provider = ETHUtility.ETHNetworksList[network];
    }

    [Button]
    public void SetChain(Chain chain)
    {
        //WalletService.m_chain = chain;
    }
}
