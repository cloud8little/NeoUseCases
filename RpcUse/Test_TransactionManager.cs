using Neo;
using Neo.Network.P2P.Payloads;
using Neo.Network.RPC;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestRpcClient
{
    class Test_TransactionManager
    {
        KeyPair keyPair1 = Neo.Network.RPC.Utility.GetKeyPair("L5TNJrPhrvsKhQfjX7Uyc9eaDKSpdZZZXjFmmpDCdm1kQe5ntA25");
        KeyPair keyPair2 = Neo.Network.RPC.Utility.GetKeyPair("L4N1Yh6NkjGqoGqNhCCotpkD1PpAHyYtmC7kE4vWRS9GBNr13bFM");
        KeyPair keyPair3 = Neo.Network.RPC.Utility.GetKeyPair("KyXXWrNkTTZN9kC5SHKLpDkSocx5uzp7o4PdQpjM6hh5okzm891C");

        public void Test_CreateTransferTx_SingletoMulti(RpcClient rpcClient)
        {
            // get the sender scriptHash, which will pay the system and network fee            
            UInt160 sender = Contract.CreateSignatureContract(keyPair1.PublicKey).ScriptHash;
            // create multi-signatures contract
            Contract multiContract = Contract.CreateMultiSigContract(2, keyPair1.PublicKey, keyPair2.PublicKey, keyPair3.PublicKey);
            // get the scripthash of the multi-signature Contract
            UInt160 multiAccount = multiContract.Script.ToScriptHash();

            // construct the script
            UInt160 scriptHash = NativeContract.GAS.Hash;
            byte[] script = scriptHash.MakeScript("transfer", sender, multiAccount, 15 * NativeContract.GAS.Factor);
            //script = script.Concat(new byte[] { (byte)OpCode.THROWIFNOT }).ToArray();

            // add Cosigners, this is a collection of scripthashs which need to be signed
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.CalledByEntry, Account = sender } };

            // initialize the TransactionManager with rpc client and sender scripthash
            Transaction tx = new TransactionManager(rpcClient)
                // fill the script, attribute, cosigner and network fee
                .MakeTransaction(script, signers)
                // add signature for the transaction with sendKey
                .AddSignature(keyPair1)
                // sign transaction with the added signature
                .SignAsync().Result;

            // broadcasts transaction over the NEO network.
            rpcClient.SendRawTransaction(tx);
            Console.WriteLine($"Transaction {tx.Hash.ToString()} is broadcasted!");

            // print a message after the transaction is on chain
            WalletAPI neoAPI = new WalletAPI(rpcClient);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));

            //+getrawtx
        }

        public void Test_CreateTransferTx_MultitoSingle(RpcClient rpcClient)
        {
            // create multi-signature contract
            Contract multiContract = Contract.CreateMultiSigContract(2, keyPair1.PublicKey, keyPair2.PublicKey, keyPair3.PublicKey);

            // construct the script
            UInt160 scriptHash = NativeContract.GAS.Hash;
            UInt160 multiAccount = multiContract.Script.ToScriptHash();
            UInt160 receiver = Contract.CreateSignatureContract(keyPair3.PublicKey).ScriptHash;
            byte[] script = scriptHash.MakeScript("transfer", multiAccount, receiver, 10 * NativeContract.GAS.Factor);
            //script = script.Concat(new byte[] { (byte)OpCode.THROWIFNOT }).ToArray();

            // add Cosigners, this is a collection of scripthashs which need to be signed
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.CalledByEntry, Account = multiAccount } };

            // initialize the TransactionManager with rpc client and sender scripthash
            Transaction tx = new TransactionManager(rpcClient)
                // fill the script, attribute, cosigner and network fee, multi-sign account need to fill networkfee by user
                .MakeTransaction(script, signers)
                // add multi-signature for the transaction with sendKey, at least use 2 KeyPairs
                .AddMultiSig(keyPair3, 2, keyPair3.PublicKey, keyPair1.PublicKey, keyPair2.PublicKey)
                .AddMultiSig(keyPair1, 2, keyPair3.PublicKey, keyPair1.PublicKey, keyPair2.PublicKey)
                // sign the transaction with the added signature
                .SignAsync().Result;

            // broadcast the transaction over the NEO network.
            rpcClient.SendRawTransaction(tx);
            Console.WriteLine($"Transaction {tx.Hash.ToString()} is broadcasted!");

            // print a message after the transaction is on chain
            WalletAPI neoAPI = new WalletAPI(rpcClient);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is on block {(await p).BlockHash}"));

            //+getrawtx
        }
    }
}
