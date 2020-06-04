using Neo;
using Neo.IO;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using System;

namespace Neo2_Test
{
    internal class InvokeContract
    {
        public static void SendRawTransfer()
        {
            byte[] prikey = Wallet.GetPrivateKeyFromWIF("L2byQYg4BCmV2Eu6eXs3fvwtgXZkjdAWRX4v7QryMPmN67dvapct");
            KeyPair keyPair = new KeyPair(prikey);

            var contract = Contract.CreateSignatureContract(keyPair.PublicKey);

            UInt160 contract_hash = UInt160.Parse("0x80720db6a0c9dc66c97b2676929905ddc8ad0b3e");//合约 hash

            //构建交易

            byte[] script;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitAppCall(contract_hash, "deploy");
                script = sb.ToArray();
            }

            //拼交易
            InvocationTransaction tx = Helper.MakeTran(null, contract.Address, 0, script);

            tx.Attributes = Helper.GetAttribute(contract.ScriptHash);

            var signature = tx.Sign(keyPair);

            tx.Witnesses = Helper.GetWitness(signature, keyPair.PublicKey);

            Console.WriteLine("txid: " + tx.Hash.ToString());

            byte[] data = tx.ToArray();
            string rawdata = data.ToHexString();

            string result = Helper.InvokeRpc("sendrawtransaction", rawdata);

            Console.WriteLine(result.ToString());
        }

        public static void Transfer()
        {
            byte[] prikey = Wallet.GetPrivateKeyFromWIF("L2byQYg4BCmV2Eu6eXs3fvwtgXZkjdAWRX4v7QryMPmN67dvapct");
            KeyPair keyPair = new KeyPair(prikey);

            var contract = Contract.CreateSignatureContract(keyPair.PublicKey);

            UInt160 contract_hash = UInt160.Parse("0x80720db6a0c9dc66c97b2676929905ddc8ad0b3e");//合约 hash

            //构建交易

            byte[] script;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitAppCall(contract_hash, "transfer", contract.ScriptHash, "AMjHjk2KFuLnrDhQCE9CvDtnuSTFADUbg8".ToScriptHash(), 1230122112);
                script = sb.ToArray();
            }

            //拼交易
            InvocationTransaction tx = Helper.MakeTran(null, contract.Address, 0, script);

            tx.Attributes = Helper.GetAttribute(contract.ScriptHash);

            var signature = tx.Sign(keyPair);

            tx.Witnesses = Helper.GetWitness(signature, keyPair.PublicKey);

            Console.WriteLine("txid: " + tx.Hash.ToString());

            byte[] data = tx.ToArray();
            string rawdata = data.ToHexString();

            string result = Helper.InvokeRpc("sendrawtransaction", rawdata);

            Console.WriteLine(result.ToString());
        }

        public static void InvokeScript()
        {           
            UInt160 contract_hash = UInt160.Parse("0x80720db6a0c9dc66c97b2676929905ddc8ad0b3e");//合约 hash

            //构建交易
            byte[] script;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitAppCall(contract_hash, "name");
                sb.EmitAppCall(contract_hash, "decimals");
                sb.EmitAppCall(contract_hash, "totalSupply");
                script = sb.ToArray();
            }

            //
            string result = Helper.InvokeRpc("invokescript", script?.ToHexString());
            
            Console.WriteLine(result);
        }
       
    }
}