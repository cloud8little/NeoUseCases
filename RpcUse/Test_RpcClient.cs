using Neo;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Network.RPC;
using Neo.Network.RPC.Models;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestRpcClient
{
    class Test_RpcClient
    {
        public void Test_Blockchain(RpcClient rpcClient)
        {            
            //GetBestBlockHash
            string bestblockhash = rpcClient.GetBestBlockHash().Result;
            //GetBlockCount
            double blockCount = rpcClient.GetBlockCount().Result;
            //GetBlockHash
            string blockHash = rpcClient.GetBlockHash((int)blockCount - 1).Result;
            //GetBlockHex
            string blockHex_fromHash = rpcClient.GetBlockHex(blockHash).Result;
            string blockHex_fromIndex = rpcClient.GetBlockHex(((int)blockCount - 1).ToString()).Result;
            //GetBlock
            RpcBlock block_fromHash = rpcClient.GetBlock(blockHash).Result;
            RpcBlock block_fromIndex = rpcClient.GetBlock(((int)blockCount - 1).ToString()).Result;
            //GetBlockHeaderHex
            string blockHeaderHex_fromHash = rpcClient.GetBlockHeaderHex(blockHash).Result;
            string blockHeaderHex_fromIndex = rpcClient.GetBlockHeaderHex(((int)blockCount - 1).ToString()).Result;
            //GetBlockHeader
            RpcBlockHeader blockHeader_fromHash = rpcClient.GetBlockHeader(blockHash).Result;
            RpcBlockHeader blockHeader_fromIndex = rpcClient.GetBlockHeader(((int)blockCount - 1).ToString()).Result;
            //GetBlockSysFee
            //BigInteger blockSysFee = rpcClient.GetBlockSysFee((int)blockCount - 1);
            //GetContractState
            ContractState contractState = rpcClient.GetContractState(NativeContract.GAS.Hash.ToString()).Result;

            //GetRawMempool + see Test_Wallet_and_Plugins()
            //GetRawMempoolBoth + see Test_Wallet_and_Plugins()

            //GetRawTransactionHex
            string rawTransactionHex = rpcClient.GetRawTransactionHex("0xd7cfe4a36a7e5d08b4855350fa1efae3c3009ab80f28a97025818309b3f8ccb7").Result;
            //GetRawTransaction
            RpcTransaction rawTransaction = rpcClient.GetRawTransaction("0xd7cfe4a36a7e5d08b4855350fa1efae3c3009ab80f28a97025818309b3f8ccb7").Result;
            //GetTransactionHeight
            uint transactionHeight = rpcClient.GetTransactionHeight("0xd7cfe4a36a7e5d08b4855350fa1efae3c3009ab80f28a97025818309b3f8ccb7").Result;

            //GetStorage+Needfix
            //var storage = rpcClient.GetStorage(NativeContract.NEO.Hash.ToString(), "746f74616c537570706c79");
            
            //GetValidators
            RpcValidator[] validators = rpcClient.GetValidators().Result;
        }

        public void Test_Node(RpcClient rpcClient)
        {
            //GetConnectionCount
            int connectionCount = rpcClient.GetConnectionCount().Result;
            //GetPeers
            RpcPeers peers = rpcClient.GetPeers().Result;
            //GetVersion
            RpcVersion version = rpcClient.GetVersion().Result;

            //SendRawTransaction + see Test_Nep5API.cs

            //SubmitBlock
            //UInt256 submitBlockResult = rpcClient.SubmitBlock(rpcClient.GetBlockHex(rpcClient.GetBestBlockHash()).HexToBytes());
        }

        public void Test_SmartContract(RpcClient rpcClient)
        {
            //When RpcStack[] is null
            RpcStack[] stack_totalSupply = new RpcStack[] { };

            UInt160 scriptHashesForVerifying = UInt160.Parse("0x20e22e16cfbcfdd29f347268427b76863b7679fa");
            Signer signer = new Signer { Account = scriptHashesForVerifying, Scopes = WitnessScope.CalledByEntry };
            //InvokeFunction
            RpcInvokeResult invokeResult_totalSupply = rpcClient.InvokeFunction(NativeContract.NEO.Hash.ToString(), "totalSupply", stack_totalSupply, signer).Result;
            //InvokeScript
            RpcInvokeResult invokeScriptResult_totalSupply = rpcClient.InvokeScript(invokeResult_totalSupply.Script.HexToBytes()).Result;

            //When RpcStack[] isn't null
            RpcStack stackItem = new RpcStack();
            stackItem.Type = "Hash160";
            stackItem.Value = "0xd168dc18e69ea17137ebbe4530002d5d7b564f79";
            RpcStack[] stack_balanceOf = new RpcStack[1] { stackItem };
            //InvokeFunction
            RpcInvokeResult invokeResult_balanceOf = rpcClient.InvokeFunction(NativeContract.GAS.Hash.ToString(), "balanceOf", stack_balanceOf).Result;
            //InvokeScript
            //RpcInvokeResult invokeScriptResult_balanceOf = rpcClient.InvokeScript(invokeResult_balanceOf.Script.HexToBytes());

            string script = invokeResult_balanceOf.Script;
            string engineState = invokeResult_balanceOf.State.ToString();
            long gasConsumed = long.Parse(invokeResult_balanceOf.GasConsumed);
          
            string transaction = invokeResult_balanceOf.Tx;
        }

        public void Test_Utilities(RpcClient rpcClient)
        {
            //ListPlugins
            RpcPlugin[] plugins = rpcClient.ListPlugins().Result;
            //ValidateAddress
            RpcValidateAddressResult validateAddressResult_null = rpcClient.ValidateAddress("").Result;
            RpcValidateAddressResult validateAddressResult_false = rpcClient.ValidateAddress("NYzbJ4vhBXmk9QBDtpCiVgF98QJxsJBYFD").Result;
            RpcValidateAddressResult validateAddressResult_true = rpcClient.ValidateAddress("NYzbJ4vhBXmk9QBDtpCiVgF98QJxsJBYFC").Result;
        }

        public void Test_Wallet_and_Plugins(RpcClient rpcClient)
        {
            //CloseWallet + when no wallet is opened
            Boolean closeResult = rpcClient.CloseWallet().Result;
            //OpenWallet
            Boolean openwallet = rpcClient.OpenWallet("1.json", "1").Result;
            //DumpPrivKey
            //string privkey_null = rpcClient.DumpPrivKey("");
            //string privkey_falseFormat = rpcClient.DumpPrivKey("AcDZPbtcK8djLk5ZXLkps9XL7ee45RHkBB");
            //string privkey_addrNotInTheWallet = rpcClient.DumpPrivKey("NWyPzTPZ8sZEMHa4WC6FfdJ1ieTamTvKiE");
            string privkey_true = rpcClient.DumpPrivKey("NNB8GKS7mdMXXGsAwvXYyEGonkEjDbqNkG").Result;
            //GetBalance
            BigDecimal walletBalance = rpcClient.GetWalletBalance(NativeContract.GAS.Hash.ToString()).Result;
            //GetNewAddress
            string newAddress = rpcClient.GetNewAddress().Result;
            //GetUnclaimedGas
            BigInteger unclaimedGas = rpcClient.GetWalletUnclaimedGas().Result;
            //ImportPrivKey
            byte[] privateKey = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(privateKey);
            }
            KeyPair keyPair = new KeyPair(privateKey);
            string wif = keyPair.Export();
            //RpcAccount account_null = rpcClient.ImportPrivKey("");
            //RpcAccount account_falseFormat = rpcClient.ImportPrivKey("L2F7F6HY3CtLZPUwpLeqFkfGVmTchjEBTozi9gB1NScv3aiRoEfb");
            RpcAccount account_alreadyExist = rpcClient.ImportPrivKey("Kx4G77Esuo9b8wXyDeagvJxWKPw7kK5GfVR1A5JDFqxtbNGRiWzu").Result;
            RpcAccount account_true = rpcClient.ImportPrivKey(keyPair.Export()).Result;
            //ListAddress
            List<RpcAccount> addresses = rpcClient.ListAddress().Result;
            //SendFrom
            JObject sendFromResult = rpcClient.SendFrom(NativeContract.GAS.Hash.ToString(), "NikMd2j2bgVr8HzYgoJjbnwUPyXWnzjDCM", "NWyPzTPZ8sZEMHa4WC6FfdJ1ieTamTvKiE", "0.0001").Result;
            WaitTransaction(rpcClient, sendFromResult["hash"].AsString())
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is confirmed in block {(await p).BlockHash}"));

            //SendMany
            RpcTransferOut rpcTransferOut1 = new RpcTransferOut();
            rpcTransferOut1.Asset = NativeContract.NEO.Hash;
            rpcTransferOut1.Value = "1";
            rpcTransferOut1.ScriptHash = "NWyPzTPZ8sZEMHa4WC6FfdJ1ieTamTvKiE".ToScriptHash();
            RpcTransferOut rpcTransferOut2 = new RpcTransferOut();
            rpcTransferOut2.Asset = NativeContract.GAS.Hash;
            rpcTransferOut2.Value = "1";
            rpcTransferOut2.ScriptHash = "NWyPzTPZ8sZEMHa4WC6FfdJ1ieTamTvKiE".ToScriptHash();
            IEnumerable<RpcTransferOut> rpcTransferOuts = new RpcTransferOut[2] { rpcTransferOut1, rpcTransferOut2 };
            JObject sendMany_FromIsSpecified = rpcClient.SendMany("NikMd2j2bgVr8HzYgoJjbnwUPyXWnzjDCM", rpcTransferOuts).Result;
            JObject sendMany_FromIsNull = rpcClient.SendMany("",rpcTransferOuts).Result;

            //SendToAddress
            JObject sendToAddressResult = rpcClient.SendToAddress(NativeContract.NEO.Hash.ToString(), "NWyPzTPZ8sZEMHa4WC6FfdJ1ieTamTvKiE", "1").Result;
            string sendToAddress_txHash = sendToAddressResult["hash"].AsString();
            WaitTransaction(rpcClient, sendToAddress_txHash)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is confirmed in block {(await p).BlockHash}"));
            //GetRawMempool
            string[] mempool = rpcClient.GetRawMempool().Result;
            //GetRawMempoolBoth
            RpcRawMemPool mempoolBoth = rpcClient.GetRawMempoolBoth().Result;
            //GetApplicationLog
            RpcApplicationLog applicationLog = rpcClient.GetApplicationLog("0x3e9a33142c42453301b8cd07540f543e6c9f0e23bcf88b37484b21b2d4cc822b").Result;
            //GetNep5Balances
            RpcNep5Balances nep5Balances = rpcClient.GetNep5Balances("NWyPzTPZ8sZEMHa4WC6FfdJ1ieTamTvKiE").Result;
            //GetNep5Transfers
            RpcNep5Transfers nep5Transfers = rpcClient.GetNep5Transfers("NWyPzTPZ8sZEMHa4WC6FfdJ1ieTamTvKiE", 1468595301000/*, 1578971382894*/).Result;
        }

        public async Task<RpcTransaction> WaitTransaction(RpcClient rpcClient, string txHash, int timeout = 60)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(timeout);
            RpcTransaction rpcTx = null;
            while (rpcTx == null || rpcTx.Confirmations == null)
            {
                if (deadline < DateTime.UtcNow)
                {
                    throw new TimeoutException();
                }

                try
                {
                    rpcTx = rpcClient.GetRawTransaction(txHash).Result;
                    if (rpcTx == null || rpcTx.Confirmations == null)
                    {
                        await Task.Delay((int)Blockchain.MillisecondsPerBlock / 2);
                    }
                }
                catch (Exception) { }
            }
            return rpcTx;
        }
    }
}
