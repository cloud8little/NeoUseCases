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
using RpcClientTest;

namespace TestRpcClient
{
    class Test_ContractClient : TestBase
    {
        ContractClient contractClient;
        public Test_ContractClient(RpcClient rpcClient)
        {
            contractClient = new ContractClient(rpcClient);

            RpcClient = rpcClient;
        }

        public override void Run()
        {
            //Test_CreateDeployContractTx();

            //ContractState contractState = RpcClient.GetContractState("0xf621168b1fce3a89c33a5f6bcf7e774b4657031c");

            //Test_Migrate();

            //Test_Destroy();
        }

        public void Test_CreateDeployContractTx()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo rootDir = Directory.GetParent(Directory.GetParent(Directory.GetParent(currentDirectory).ToString()).ToString());

            string nefFilePath = rootDir.ToString() + "\\Template.NEP5.CSharp.nef";
            string manifestFilePath = rootDir.ToString() + "\\Template.NEP5.CSharp.manifest.json";

            //read nefFile & manifestFile
            NefFile nefFile;
            using (var stream = new BinaryReader(File.OpenRead(nefFilePath), Encoding.UTF8, false))
            {
                nefFile = stream.ReadSerializable<NefFile>();
            }
            var mani = File.ReadAllBytes(manifestFilePath);
            ContractManifest manifest = ContractManifest.Parse(mani);

            Console.WriteLine("contract hash:" + nefFile.ScriptHash);

            //deploy contract
            var tx = contractClient.CreateDeployContractTx(nefFile.Script, manifest, keyPair1).Result;

            //broadcast
            RpcClient.SendRawTransaction(tx);

            Console.WriteLine($"Transaction {tx.Hash} is broadcasted!");

            //print a message after the transaction is on chain
            WalletAPI walletAPI = new WalletAPI(RpcClient);
            walletAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));

            //getContractState + here add wait a block
            //ContractState contractState = RpcClient.GetContractState(nefFile.ScriptHash.ToString());

            ////InvokeDeploy
            //UInt160 witnessAddress = Contract.CreateSignatureContract(keyPair1.PublicKey).ScriptHash;
            //byte[] script = nefFile.ScriptHash.MakeScript("deploy"/*, "[]"*/);
            //Cosigner[] cosigners = new[] { new Cosigner { Scopes = WitnessScope.CalledByEntry, Account = witnessAddress } };
            //Transaction invokeTx = new TransactionManager(RpcClient, witnessAddress)
            //    .MakeTransaction(script, null, cosigners)
            //    .AddSignature(keyPair1)
            //    .Sign()
            //    .Tx;
            //RpcClient.SendRawTransaction(invokeTx);
            //Console.WriteLine($"Transaction {invokeTx.Hash} is broadcasted!");
            //WalletAPI neoAPI = new WalletAPI(RpcClient);
            //neoAPI.WaitTransaction(invokeTx)
            //   .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));


            //var addr1 = Contract.CreateSignatureRedeemScript(keyPair0.PublicKey).ToScriptHash();
            ////InvokeTransfer         
            //byte[] script = nefFile.ScriptHash.MakeScript("transfer", witnessAddress, addr1, 8888);
            //Cosigner[] cosigners = new[] { new Cosigner { Scopes = WitnessScope.CalledByEntry, Account = witnessAddress } };
            //Transaction invokeTx = new TransactionManager(RpcClient, witnessAddress)
            //    .MakeTransaction(script, null, cosigners)
            //    .AddSignature(keyPair1)
            //    .Sign()
            //    .Tx;
            //RpcClient.SendRawTransaction(invokeTx);
            //Console.WriteLine($"Transaction {invokeTx.Hash} is broadcasted!");
            //WalletAPI neoAPI = new WalletAPI(RpcClient);
            //neoAPI.WaitTransaction(invokeTx)
            //   .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));

            ////TestInvoke

            //RpcInvokeResult invokeResult_name = contractClient.TestInvoke(nefFile.ScriptHash, "name", "[]");
            //string name = invokeResult_name.Stack[0].ToStackItem().GetString();
            //RpcInvokeResult invokeResult_totalSupply = contractClient.TestInvoke(nefFile.ScriptHash, "totalSupply", "[]");
            //BigInteger totalSupply = invokeResult_totalSupply.Stack[0].ToStackItem().GetBigInteger();

            //Console.WriteLine("name:" + name);
            //Console.WriteLine("total:" + totalSupply);
        }

        public void Test_Migrate()
        {            
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
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.CalledByEntry, Account = witnessAddress } };
            Transaction invokeTx = new TransactionManager(RpcClient)
                .MakeTransaction(script, signers)
                .AddSignature(keyPair1)
                .SignAsync().Result;
            RpcClient.SendRawTransaction(invokeTx);
            Console.WriteLine($"Transaction {invokeTx.Hash.ToString()} is broadcasted!");
            WalletAPI neoAPI = new WalletAPI(RpcClient);
            neoAPI.WaitTransaction(invokeTx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));

            //getContractState + here add wait a block
            //get old contract, return unknown contract
            ContractState contractState_old = RpcClient.GetContractState(nefFile_old.ScriptHash.ToString()).Result;
            //get new contract, id should be the same as before
            ContractState contractState_new = RpcClient.GetContractState(nefFile.ScriptHash.ToString()).Result;

            //getstorage
        }

        public void Test_Destroy()
        {
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
            Signer[] signers = new[] { new Signer { Scopes = WitnessScope.CalledByEntry, Account = witnessAddress } };
            Transaction invokeTx = new TransactionManager(RpcClient)
                .MakeTransaction(script, signers)
                .AddSignature(keyPair1)
                .SignAsync().Result;
            RpcClient.SendRawTransaction(invokeTx);
            Console.WriteLine($"Transaction {invokeTx.Hash.ToString()} is broadcasted!");
            WalletAPI neoAPI = new WalletAPI(RpcClient);
            neoAPI.WaitTransaction(invokeTx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));

            //getstorage
        }
    }
}
