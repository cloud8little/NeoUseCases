using Neo;
using Neo.IO;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Network.RPC;
using Neo.Network.RPC.Models;
using Neo.SmartContract;
using Neo.SmartContract.Manifest;
using Neo.VM;
using Neo.Wallets;
using System;
using System.IO;
using System.Text;
using System.Numerics;

namespace TestRpcClient
{
    class Test_ContractClient
    {
        public void Test_CreateDeployContractTx_and_TestInvoke(RpcClient rpcClient)
        {
            ContractClient contractClient = new ContractClient(rpcClient);

            string currentDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo rootDir = Directory.GetParent(Directory.GetParent(Directory.GetParent(currentDirectory).ToString()).ToString());

            string nefFilePath = rootDir.ToString() + "\\Template.NEP5.CSharp.nef";
            string manifestFilePath = rootDir.ToString() + "\\Template.NEP5.CSharp.manifest.json";
            KeyPair keyPair1 = Neo.Network.RPC.Utility.GetKeyPair("Kx4G77Esuo9b8wXyDeagvJxWKPw7kK5GfVR1A5JDFqxtbNGRiWzu");

            //read nefFile & manifestFile
            NefFile nefFile;
            using (var stream = new BinaryReader(File.OpenRead(nefFilePath), Encoding.UTF8, false))
            {
                nefFile = stream.ReadSerializable<NefFile>();
            }
            ContractManifest manifest = ContractManifest.Parse(File.ReadAllBytes(manifestFilePath));

            //deploy contract
            var tx = contractClient.CreateDeployContractTx(nefFile.Script, manifest, keyPair1);

            //broadcast
            rpcClient.SendRawTransaction(tx);
            Console.WriteLine($"Transaction {tx.Hash.ToString()} is broadcasted!");

            //print a message after the transaction is on chain
            WalletAPI walletAPI = new WalletAPI(rpcClient);
            walletAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));

            //getContractState + here add wait a block
            ContractState contractState = rpcClient.GetContractState(nefFile.ScriptHash.ToString());

            //InvokeDeploy
            UInt160 witnessAddress = Contract.CreateSignatureContract(keyPair1.PublicKey).ScriptHash;
            byte[] script = nefFile.ScriptHash.MakeScript("deploy"/*, "[]"*/);
            Cosigner[] cosigners = new[] { new Cosigner { Scopes = WitnessScope.CalledByEntry, Account = witnessAddress } };
            Transaction invokeTx = new TransactionManager(rpcClient, witnessAddress)
                .MakeTransaction(script, null, cosigners)
                .AddSignature(keyPair1)
                .Sign()
                .Tx;
            rpcClient.SendRawTransaction(invokeTx);
            Console.WriteLine($"Transaction {invokeTx.Hash.ToString()} is broadcasted!");
            WalletAPI neoAPI = new WalletAPI(rpcClient);
            neoAPI.WaitTransaction(invokeTx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));

            //TestInvoke

            RpcInvokeResult invokeResult_name = contractClient.TestInvoke(nefFile.ScriptHash, "name", "[]");
            string name = invokeResult_name.Stack[0].ToStackItem().GetString();
            RpcInvokeResult invokeResult_totalSupply = contractClient.TestInvoke(nefFile.ScriptHash, "totalSupply", "[]");
            BigInteger totalSupply = invokeResult_totalSupply.Stack[0].ToStackItem().GetBigInteger();

            //getstorage
        }

        public void Test_Migrate(RpcClient rpcClient)
        {
            ContractClient contractClient = new ContractClient(rpcClient);
                        
            string currentDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo rootDir = Directory.GetParent(Directory.GetParent(Directory.GetParent(currentDirectory).ToString()).ToString());

            //old nefFile
            string nefFilePath_old = rootDir.ToString() + "\\Template.NEP5.CSharp.nef";

            NefFile nefFile_old;
            using (var stream = new BinaryReader(File.OpenRead(nefFilePath_old), Encoding.UTF8, false))
            {
                nefFile_old = stream.ReadSerializable<NefFile>();
            }

            //new nefFile&manifestFile
            string nefFilePath = rootDir.ToString() + "\\Template.NEP5.CSharp_migrate.nef";
            string manifestFilePath = rootDir.ToString() + "\\Template.NEP5.CSharp_migrate.manifest.json";
            KeyPair keyPair1 = Neo.Network.RPC.Utility.GetKeyPair("Kx4G77Esuo9b8wXyDeagvJxWKPw7kK5GfVR1A5JDFqxtbNGRiWzu");

            //read nefFile & manifestFile
            NefFile nefFile;
            using (var stream = new BinaryReader(File.OpenRead(nefFilePath), Encoding.UTF8, false))
            {
                nefFile = stream.ReadSerializable<NefFile>();
            }
            ContractManifest manifest = ContractManifest.Parse(File.ReadAllBytes(manifestFilePath));
            string manifest_str = manifest.ToString();
            
            UInt160 witnessAddress = Contract.CreateSignatureContract(keyPair1.PublicKey).ScriptHash;
            byte[] script = nefFile_old.ScriptHash.MakeScript("migrate", nefFile.Script, manifest.ToString());
            Cosigner[] cosigners = new[] { new Cosigner { Scopes = WitnessScope.CalledByEntry, Account = witnessAddress } };
            Transaction invokeTx = new TransactionManager(rpcClient, witnessAddress)
                .MakeTransaction(script, null, cosigners)
                .AddSignature(keyPair1)
                .Sign()
                .Tx;
            rpcClient.SendRawTransaction(invokeTx);
            Console.WriteLine($"Transaction {invokeTx.Hash.ToString()} is broadcasted!");
            WalletAPI neoAPI = new WalletAPI(rpcClient);
            neoAPI.WaitTransaction(invokeTx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));

            //getContractState + here add wait a block
            //get old contract, return unknown contract
            ContractState contractState_old = rpcClient.GetContractState(nefFile_old.ScriptHash.ToString());
            //get new contract, id should be the same as before
            ContractState contractState_new = rpcClient.GetContractState(nefFile.ScriptHash.ToString());

            //getstorage
        }

        public void Test_Destroy(RpcClient rpcClient)
        {
            ContractClient contractClient = new ContractClient(rpcClient);

            string currentDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo rootDir = Directory.GetParent(Directory.GetParent(Directory.GetParent(currentDirectory).ToString()).ToString());

            string nefFilePath = rootDir.ToString() + "\\Template.NEP5.CSharp_migrate.nef";
            string manifestFilePath = rootDir.ToString() + "\\Template.NEP5.CSharp_migrate.manifest.json";
            KeyPair keyPair1 = Neo.Network.RPC.Utility.GetKeyPair("Kx4G77Esuo9b8wXyDeagvJxWKPw7kK5GfVR1A5JDFqxtbNGRiWzu");

            //read nefFile & manifestFile
            NefFile nefFile;
            using (var stream = new BinaryReader(File.OpenRead(nefFilePath), Encoding.UTF8, false))
            {
                nefFile = stream.ReadSerializable<NefFile>();
            }
            ContractManifest manifest = ContractManifest.Parse(File.ReadAllBytes(manifestFilePath));

            UInt160 witnessAddress = Contract.CreateSignatureContract(keyPair1.PublicKey).ScriptHash;
            byte[] script = nefFile.ScriptHash.MakeScript("destroy");
            Cosigner[] cosigners = new[] { new Cosigner { Scopes = WitnessScope.CalledByEntry, Account = witnessAddress } };
            Transaction invokeTx = new TransactionManager(rpcClient, witnessAddress)
                .MakeTransaction(script, null, cosigners)
                .AddSignature(keyPair1)
                .Sign()
                .Tx;
            rpcClient.SendRawTransaction(invokeTx);
            Console.WriteLine($"Transaction {invokeTx.Hash.ToString()} is broadcasted!");
            WalletAPI neoAPI = new WalletAPI(rpcClient);
            neoAPI.WaitTransaction(invokeTx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));

            //getstorage
        }
    }
}
