using Neo;
using Neo.Cryptography.ECC;
using Neo.Network.P2P.Payloads;
using Neo.Network.RPC;
using Neo.Network.RPC.Models;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.Wallets;
using RpcClientTest;
using System;
using System.Linq;
using System.Numerics;

namespace TestRpcClient
{
    class Test_Nep5API : TestBase
    {
        Nep5API nep5API;

        public Test_Nep5API(RpcClient rpcClient)
        {
            RpcClient = rpcClient;

            nep5API = new Nep5API(rpcClient);
        }

        public override void Run()
        {
            Test_Nep5TokenInfo();

            Test_CreateTransferTx_SingleToSingle();

            Test_CreateTransferTx_MultiSig();
        }

        public void Test_Nep5TokenInfo()
        {            
            string addr1 = Contract.CreateSignatureRedeemScript(keyPair0.PublicKey).ToScriptHash().ToAddress();

            BigInteger balance = nep5API.BalanceOf(NativeContract.GAS.Hash, Neo.Network.RPC.Utility.GetScriptHash(addr1)).Result;
            string name = nep5API.Name(NativeContract.GAS.Hash).Result;
            string symbol = nep5API.Symbol(NativeContract.GAS.Hash).Result;
            uint decimals = nep5API.Decimals(NativeContract.GAS.Hash).Result;
            BigInteger totalSupply = nep5API.TotalSupply(NativeContract.GAS.Hash).Result;
            RpcNep5TokenInfo nep5TokenInfo = nep5API.GetTokenInfo(NativeContract.GAS.Hash).Result;

            Console.WriteLine();
        }

        public void Test_CreateTransferTx_SingleToSingle()
        {
            UInt160 addr = Contract.CreateSignatureContract(keyPair1.PublicKey).ScriptHash;

            Transaction tx = nep5API.CreateTransferTx(gas, keyPair0, addr, new BigInteger(5)).Result;

            //broadcast
            RpcClient.SendRawTransaction(tx);

            Console.WriteLine($"Transaction {tx.Hash.ToString()} is broadcasted!");

            //print a message after the transaction is on chain
            WalletAPI neoAPI = new WalletAPI(RpcClient);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));

            //+getrawtx
        }

        public void Test_CreateTransferTx_MultiSig()
        {            
            Transaction tx = nep5API.CreateTransferTx(gas, 3, new ECPoint[] { keyPair0.PublicKey, keyPair1.PublicKey, keyPair2.PublicKey }, new KeyPair[] { keyPair0, keyPair1, keyPair2 }, Contract.CreateSignatureContract(keyPair0.PublicKey).ScriptHash, new BigInteger(10)).Result;

            //broadcast
            RpcClient.SendRawTransaction(tx);
            Console.WriteLine($"Transaction {tx.Hash.ToString()} is broadcasted!");

            //print a message after the transaction is on chain
            WalletAPI neoAPI = new WalletAPI(RpcClient);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));

            //+getrawtx
        }
    }
}
