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
        
        public void Test_Nep5TokenInfo(RpcClient rpcClient)
        {
            nep5API = new Nep5API(rpcClient);
            string addr1 = Contract.CreateSignatureRedeemScript(keyPair0.PublicKey).ToScriptHash().ToAddress();

            BigInteger balance = nep5API.BalanceOf(NativeContract.GAS.Hash, Neo.Network.RPC.Utility.GetScriptHash(addr1));
            string name = nep5API.Name(NativeContract.GAS.Hash);
            string symbol = nep5API.Symbol(NativeContract.GAS.Hash);
            uint decimals = nep5API.Decimals(NativeContract.GAS.Hash);
            BigInteger totalSupply = nep5API.TotalSupply(NativeContract.GAS.Hash);
            RpcNep5TokenInfo nep5TokenInfo = nep5API.GetTokenInfo(NativeContract.GAS.Hash);
        }

        public void Test_CreateTransferTx_SingleToSingle(RpcClient rpcClient)
        {
            nep5API = new Nep5API(rpcClient);

            UInt160 addr = Contract.CreateSignatureContract(keyPair1.PublicKey).ScriptHash;

            Transaction tx = nep5API.CreateTransferTx(NativeContract.NEO.Hash, keyPair0, addr, new BigInteger(10));

            //broadcast
            rpcClient.SendRawTransaction(tx);

            Console.WriteLine($"Transaction {tx.Hash.ToString()} is broadcasted!");

            //print a message after the transaction is on chain
            WalletAPI neoAPI = new WalletAPI(rpcClient);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));

            //+getrawtx
        }

        public void Test_CreateTransferTx_MultiSig(RpcClient rpcClient)
        {
            nep5API = new Nep5API(rpcClient);

            Transaction tx = nep5API.CreateTransferTx(NativeContract.NEO.Hash, 3, new ECPoint[] { keyPair1.PublicKey, keyPair2.PublicKey, keyPair3.PublicKey, keyPair4.PublicKey }, new KeyPair[] { keyPair1, keyPair2, keyPair3, keyPair4 }, keyPair0.PublicKeyHash, new BigInteger(10));

            //broadcast
            rpcClient.SendRawTransaction(tx);
            Console.WriteLine($"Transaction {tx.Hash.ToString()} is broadcasted!");

            //print a message after the transaction is on chain
            WalletAPI neoAPI = new WalletAPI(rpcClient);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));

            //+getrawtx
        }
    }
}
