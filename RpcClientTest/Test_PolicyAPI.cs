using Neo;
using Neo.Network.RPC;
using RpcClientTest;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestRpcClient
{
    class Test_PolicyAPI : TestBase
    {
        PolicyAPI policyAPI;

        public Test_PolicyAPI(RpcClient rpcClient)
        {
            policyAPI = new PolicyAPI(rpcClient);
            RpcClient = rpcClient;
        }

        public override void Run()
        {
            Console.WriteLine(" maxTransactionsPerBlock:" + policyAPI.GetMaxTransactionsPerBlock());
            Console.WriteLine(" maxBlockSize:" + policyAPI.GetMaxBlockSize());
            Console.WriteLine(" feePerByte:" + policyAPI.GetFeePerByte());
            Console.WriteLine(" blockedAccounts:" + policyAPI.GetBlockedAccounts());
        }                
        
    }
}
