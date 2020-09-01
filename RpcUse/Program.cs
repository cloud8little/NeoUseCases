using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Neo;
using Neo.IO;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.Network.RPC;
using Neo.Network.RPC.Models;
using Neo.SmartContract;
using Neo.SmartContract.Manifest;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.Wallets;

namespace TestRpcClient
{
    class Program
    {
        static void Main(string[] args)
        {
            RpcClient rpcClient = new RpcClient("http://localhost:10332");

            UInt160 scriptHash = NativeContract.NEO.Hash; 
            byte[] script = scriptHash.MakeScript("name"); 
            var name = rpcClient.InvokeScript(script).Result.Stack.Single().GetString();

            Console.WriteLine("name:" + name);

            Test_WalletAPI test_WalletAPI = new Test_WalletAPI(rpcClient);
            test_WalletAPI.Run();

            //Test_ContractClient test_ContractClient = new Test_ContractClient(rpcClient);
            //test_ContractClient.Run();

            //Test_Nep5API test_Nep5API = new Test_Nep5API(rpcClient);
            //test_Nep5API.Run();

            //Test_PolicyAPI test_PolicyAPI = new Test_PolicyAPI(rpcClient);
            //test_PolicyAPI.Run();

            //Test_RpcClient test_RpcClient = new Test_RpcClient();
            //test_RpcClient.Test_Blockchain(rpcClient);
            //test_RpcClient.Test_Node(rpcClient);
            //test_RpcClient.Test_SmartContract(rpcClient);
            //test_RpcClient.Test_Utilities(rpcClient);
            //test_RpcClient.Test_Wallet_and_Plugins(rpcClient);



            //Test_Nep5API test_Nep5API = new Test_Nep5API();
            //test_Nep5API.Test_Nep5TokenInfo(rpcClient);
            //test_Nep5API.Test_CreateTransferTx_SingleToSingle(rpcClient);
            //test_Nep5API.Test_CreateTransferTx_MultiSig(rpcClient);

            //Test_ContractClient test_ContractClient = new Test_ContractClient();
            //test_ContractClient.Test_CreateDeployContractTx_and_TestInvoke(rpcClient);
            //test_ContractClient.Test_Migrate(rpcClient);
            //test_ContractClient.Test_Destroy(rpcClient);

            //Test_TransactionManager test_TransactionManager = new Test_TransactionManager();
            //test_TransactionManager.Test_CreateTransferTx_SingletoMulti(rpcClient);
            //test_TransactionManager.Test_CreateTransferTx_MultitoSingle(rpcClient);


            Console.ReadKey();
        }
    }
}
