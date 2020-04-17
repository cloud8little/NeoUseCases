using Neo;
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

        public Test_WalletAPI(RpcClient rpcClient) : base(rpcClient)
        {
            walletAPI = new WalletAPI(rpcClient);
        }

        public void Test()
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

            Console.WriteLine(walletAPI.Transfer(gas, 1, new[] { keyPair0.PublicKey }, new[] { keyPair0 }, account1.ScriptHash, 22).Hash);

            Console.WriteLine(walletAPI.Transfer(neo.ToString(), keyPair0.Export(), account1.ScriptHash.ToString(), 166).Hash);
            Console.WriteLine(walletAPI.Transfer(neo, keyPair0, account1.ScriptHash, 166).Hash);            
        }
    }
}
