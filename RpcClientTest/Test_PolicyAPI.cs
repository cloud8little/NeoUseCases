using Neo;
using Neo.Network.RPC;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestRpcClient
{
    class Test_PolicyAPI
    {
        PolicyAPI policyAPI;

        public void Test(RpcClient client)
        {
            policyAPI = new PolicyAPI(client);
            uint maxTransactionsPerBlock = policyAPI.GetMaxTransactionsPerBlock();
            uint maxBlockSize = policyAPI.GetMaxBlockSize();
            long feePerByte = policyAPI.GetFeePerByte();
            UInt160[] blockedAccounts = policyAPI.GetBlockedAccounts();
        }
        
    }
}
