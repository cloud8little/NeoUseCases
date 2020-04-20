using Neo;
using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.Network.P2P.Payloads;
using Neo.Network.RPC;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.Wallets;
using RpcClientTest;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TestRpcClient
{
    class Test_WalletAPI : TestBase
    {
        WalletAPI walletAPI;

        public Test_WalletAPI(RpcClient rpcClient)
        {
            walletAPI = new WalletAPI(rpcClient);
        }

        public override void Run()
        {
            var account0 = Contract.CreateSignatureContract(keyPair0.PublicKey);
            var account1 = Contract.CreateSignatureContract(keyPair1.PublicKey);

            Console.WriteLine("UnclaimedGas:" + walletAPI.GetUnclaimedGas(account0.ScriptHash.ToString()));
            Console.WriteLine("UnclaimedGas:" + walletAPI.GetUnclaimedGas(account0.ScriptHash));

            Console.WriteLine("NeoBalance:" + walletAPI.GetTokenBalance(neo.ToString(), account0.ScriptHash.ToString()));
            Console.WriteLine("NeoBalance:" + walletAPI.GetNeoBalance(account0.ScriptHash.ToString()));

            Console.WriteLine("GasBalance:" + walletAPI.GetTokenBalance(gas.ToString(), account0.ScriptHash.ToString()));
            Console.WriteLine("GasBalance:" + walletAPI.GetGasBalance(account0.ScriptHash.ToString()));

            Console.WriteLine(walletAPI.ClaimGas(keyPair0.Export()).Hash);
            Console.WriteLine(walletAPI.ClaimGas(keyPair0).Hash);

            //gas, 3, new ECPoint[] { keyPair1.PublicKey, keyPair2.PublicKey, keyPair3.PublicKey, keyPair4.PublicKey }, new KeyPair[] { keyPair1, keyPair2, keyPair3, keyPair4 }, Contract.CreateSignatureContract(keyPair0.PublicKey).ScriptHash, new BigInteger(10)
            Console.WriteLine(walletAPI.Transfer(gas, 3, new ECPoint[] { keyPair1.PublicKey, keyPair2.PublicKey, keyPair3.PublicKey, keyPair4.PublicKey }, new KeyPair[] { keyPair1, keyPair2, keyPair3 }, account0.ScriptHash, 22).Hash);

            Console.WriteLine(walletAPI.Transfer(neo.ToString(), keyPair0.Export(), account1.ScriptHash.ToString(), 166).Hash);
            Console.WriteLine(walletAPI.Transfer(neo, keyPair0, account1.ScriptHash, 166).Hash);
        }
    }
}
