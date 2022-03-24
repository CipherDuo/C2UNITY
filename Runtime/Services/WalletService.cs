using System;
using System.IO;
using System.Threading.Tasks;
using CipherDuo.Ethereum;
using CipherDuo.IPFS.Logger;
using Nethereum.KeyStore.Model;
using Nethereum.KeyStore;

using CipherDuo.Ethereum.Cryptography;

using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;

public static class WalletService
{
    public static Chain chain;

    private static ILog logger = LoggerFactory.GetLogger(nameof(ETH));

    public static async Task<Account> SetWallet(Mnemonic mnemonic, string password, int index)
    {
        if (!File.Exists(Path.Combine(ETHUtility.ETHPath, $"Wallet_Account_{index}.json")))
        {

            Wallet wallet = new Wallet(mnemonic.ToString(), password);

            logger.Log("Mnemo: " + mnemonic);

            var account = wallet.GetAccount(index);
            await SaveWallet(password, wallet, index);

            return account;
        }
        else
        {
            logger.Log("wallet with same index already exists");
            return null;
        }

    }

    private static async Task<string> SaveWallet(string password, Wallet wallet, int index)
    {
        try
        {
            ScryptParams scryptParams = new ScryptParams {Dklen = 32, N = 262144, R = 1, P = 8};
            KeyStoreScryptService keyStoreService = new KeyStoreScryptService();

            KeyStore<ScryptParams> key = keyStoreService.EncryptAndGenerateKeyStore
            (
                password,
                wallet.GetPrivateKey(index),
                wallet.GetPublicKey(index).ToString(),
                scryptParams
            );

            string jsonKey = keyStoreService.SerializeKeyStoreToJson(key);

            string encryptedJson = await Aes.AesEncryptAsync(jsonKey);


            ETHUtility.SaveProtectedJSON(encryptedJson, $"Wallet_Account_{index}.json");
            return jsonKey;

        }
        catch (Exception error)
        {
            logger.Log("wallet already exists. Error : " + error.Message);
            return null;
        }

    }

    public static int GetWalletCount()
    {
        var path = Path.Combine(ETHUtility.ETHPath);
        var files = Directory.GetFiles(path);
        return files.Length;
    }

    public static async Task<Account> GetWallet(string password, int index)
    {
        var path = Path.Combine(ETHUtility.ETHPath, $"Wallet_Account_{index}.json");

        if (File.Exists(path))
        {
            string encryptedJson = File.ReadAllText(path);

            try
            {
                string jsonKey = await Aes.AesDecryptAsync(encryptedJson);

                KeyStoreScryptService keyStoreService = new KeyStoreScryptService();

                byte[] key;
                if (password == "")
                {
                    key = keyStoreService.DecryptKeyStoreFromJson("CipherDuo", jsonKey);
                }
                else
                {
                    key = keyStoreService.DecryptKeyStoreFromJson(password, jsonKey);
                }

                var account = new Account(key, ETHUtility.m_chain);
                return await Task.Run(() => { return account; });
            }
            catch (Exception error)
            {
                logger.Log("Can't decrypt local wallet" + error.Message);
                return null;
            }
        }
        else
        {
            logger.Log("Ethereum.ETH Wallet doesn't exists");
            return null;
        }
    }

    public static Task<Account> RestoreWallet(string mnemonic, string newPassword, int index)
    {
        try
        {
            Wallet wallet = new Wallet(mnemonic, newPassword);
            return Task.Run(() => { return wallet.GetAccount(index); });
        }
        catch (Exception error)
        {
            logger.Log("can't restore wallet. Error: " + error.Message);
            return null;
        }
    }

}