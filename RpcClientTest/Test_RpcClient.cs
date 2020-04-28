using Neo;
using Neo.IO.Json;
using Neo.Ledger;
using Neo.Network.RPC;
using Neo.Network.RPC.Models;
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
            string bestblockhash = rpcClient.GetBestBlockHash();
            //GetBlockCount
            double blockCount = rpcClient.GetBlockCount();
            //GetBlockHash
            string blockHash = rpcClient.GetBlockHash((int)blockCount - 1);
            //GetBlockHex
            string blockHex_fromHash = rpcClient.GetBlockHex(blockHash);
            string blockHex_fromIndex = rpcClient.GetBlockHex(((int)blockCount - 1).ToString());
            //GetBlock
            RpcBlock block_fromHash = rpcClient.GetBlock(blockHash);
            RpcBlock block_fromIndex = rpcClient.GetBlock(((int)blockCount - 1).ToString());
            //GetBlockHeaderHex
            string blockHeaderHex_fromHash = rpcClient.GetBlockHeaderHex(blockHash);
            string blockHeaderHex_fromIndex = rpcClient.GetBlockHeaderHex(((int)blockCount - 1).ToString());
            //GetBlockHeader
            RpcBlockHeader blockHeader_fromHash = rpcClient.GetBlockHeader(blockHash);
            RpcBlockHeader blockHeader_fromIndex = rpcClient.GetBlockHeader(((int)blockCount - 1).ToString());
            //GetBlockSysFee
            //BigInteger blockSysFee = rpcClient.GetBlockSysFee((int)blockCount - 1);
            //GetContractState
            ContractState contractState = rpcClient.GetContractState(NativeContract.GAS.Hash.ToString());

            //GetRawMempool + see Test_Wallet_and_Plugins()
            //GetRawMempoolBoth + see Test_Wallet_and_Plugins()

            //GetRawTransactionHex
            string rawTransactionHex = rpcClient.GetRawTransactionHex("0xb8b7ee5108f50fd75faa6d36acb65f666b9841e619d05b4402cf156ca0864821");
            //GetRawTransaction
            RpcTransaction rawTransaction = rpcClient.GetRawTransaction("0xb8b7ee5108f50fd75faa6d36acb65f666b9841e619d05b4402cf156ca0864821");
            //GetTransactionHeight
            uint transactionHeight = rpcClient.GetTransactionHeight("0xb8b7ee5108f50fd75faa6d36acb65f666b9841e619d05b4402cf156ca0864821");

            //GetStorage+Needfix
            //var storage = rpcClient.GetStorage(NativeContract.NEO.Hash.ToString(), "746f74616c537570706c79");
            
            //GetValidators
            RpcValidator[] validators = rpcClient.GetValidators();
        }

        public void Test_Node(RpcClient rpcClient)
        {
            //GetConnectionCount
            int connectionCount = rpcClient.GetConnectionCount();
            //GetPeers
            RpcPeers peers = rpcClient.GetPeers();
            //GetVersion
            RpcVersion version = rpcClient.GetVersion();

            //SendRawTransaction + see Test_Nep5API.cs

            //SubmitBlock
            //UInt256 submitBlockResult = rpcClient.SubmitBlock(rpcClient.GetBlockHex(rpcClient.GetBestBlockHash()).HexToBytes());
        }

        public void Test_SmartContract(RpcClient rpcClient)
        {
            //When RpcStack[] is null
            RpcStack[] stack_totalSupply = new RpcStack[] { };

            UInt160 scriptHashesForVerifying = UInt160.Parse("0x20e22e16cfbcfdd29f347268427b76863b7679fa");
            //InvokeFunction
            RpcInvokeResult invokeResult_totalSupply = rpcClient.InvokeFunction(NativeContract.NEO.Hash.ToString(), "totalSupply", stack_totalSupply, scriptHashesForVerifying);
            //InvokeScript
            RpcInvokeResult invokeScriptResult_totalSupply = rpcClient.InvokeScript(invokeResult_totalSupply.Script.HexToBytes());

            //When RpcStack[] isn't null
            RpcStack stackItem = new RpcStack();
            stackItem.Type = "Hash160";
            stackItem.Value = "0xd168dc18e69ea17137ebbe4530002d5d7b564f79";
            RpcStack[] stack_balanceOf = new RpcStack[1] { stackItem };
            //InvokeFunction
            RpcInvokeResult invokeResult_balanceOf = rpcClient.InvokeFunction(NativeContract.GAS.Hash.ToString(), "balanceOf", stack_balanceOf);
            //InvokeScript
            RpcInvokeResult invokeScriptResult_balanceOf = rpcClient.InvokeScript(invokeResult_balanceOf.Script.HexToBytes());
        }

        public void Test_Utilities(RpcClient rpcClient)
        {
            //ListPlugins
            RpcPlugin[] plugins = rpcClient.ListPlugins();
            //ValidateAddress
            RpcValidateAddressResult validateAddressResult_null = rpcClient.ValidateAddress("");
            RpcValidateAddressResult validateAddressResult_false = rpcClient.ValidateAddress("NYzbJ4vhBXmk9QBDtpCiVgF98QJxsJBYFD");
            RpcValidateAddressResult validateAddressResult_true = rpcClient.ValidateAddress("NYzbJ4vhBXmk9QBDtpCiVgF98QJxsJBYFC");
        }

        public void Test_Wallet_and_Plugins(RpcClient rpcClient)
        {
            //CloseWallet + when no wallet is opened
            Boolean closeResult = rpcClient.CloseWallet();
            //OpenWallet
            Boolean openwallet = rpcClient.OpenWallet("test.json", "123");
            //DumpPrivKey
            //string privkey_null = rpcClient.DumpPrivKey("");
            //string privkey_falseFormat = rpcClient.DumpPrivKey("AcDZPbtcK8djLk5ZXLkps9XL7ee45RHkBB");
            //string privkey_addrNotInTheWallet = rpcClient.DumpPrivKey("NWyPzTPZ8sZEMHa4WC6FfdJ1ieTamTvKiE");
            string privkey_true = rpcClient.DumpPrivKey("NikMd2j2bgVr8HzYgoJjbnwUPyXWnzjDCM");
            //GetBalance
            BigDecimal walletBalance = rpcClient.GetBalance(NativeContract.GAS.Hash.ToString());
            //GetNewAddress
            string newAddress = rpcClient.GetNewAddress();
            //GetUnclaimedGas
            BigInteger unclaimedGas = rpcClient.GetUnclaimedGas();
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
            RpcAccount account_alreadyExist = rpcClient.ImportPrivKey("Kx4G77Esuo9b8wXyDeagvJxWKPw7kK5GfVR1A5JDFqxtbNGRiWzu");
            RpcAccount account_true = rpcClient.ImportPrivKey(keyPair.Export());
            //ListAddress
            List<RpcAccount> addresses = rpcClient.ListAddress();
            //SendFrom
            JObject sendFromResult = rpcClient.SendFrom(NativeContract.GAS.Hash.ToString(), "NikMd2j2bgVr8HzYgoJjbnwUPyXWnzjDCM", "NWyPzTPZ8sZEMHa4WC6FfdJ1ieTamTvKiE", "0.0001");
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
            JObject sendMany_FromIsSpecified = rpcClient.SendMany("NikMd2j2bgVr8HzYgoJjbnwUPyXWnzjDCM", rpcTransferOuts);
            JObject sendMany_FromIsNull = rpcClient.SendMany("",rpcTransferOuts);

            //SendToAddress
            JObject sendToAddressResult = rpcClient.SendToAddress(NativeContract.NEO.Hash.ToString(), "NWyPzTPZ8sZEMHa4WC6FfdJ1ieTamTvKiE", "1");
            string sendToAddress_txHash = sendToAddressResult["hash"].AsString();
            WaitTransaction(rpcClient, sendToAddress_txHash)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction is confirmed in block {(await p).BlockHash}"));
            //GetRawMempool
            string[] mempool = rpcClient.GetRawMempool();
            //GetRawMempoolBoth
            RpcRawMemPool mempoolBoth = rpcClient.GetRawMempoolBoth();
            //GetApplicationLog
            RpcApplicationLog applicationLog = rpcClient.GetApplicationLog("0x3e9a33142c42453301b8cd07540f543e6c9f0e23bcf88b37484b21b2d4cc822b");
            //GetNep5Balances
            RpcNep5Balances nep5Balances = rpcClient.GetNep5Balances("NWyPzTPZ8sZEMHa4WC6FfdJ1ieTamTvKiE");
            //GetNep5Transfers
            RpcNep5Transfers nep5Transfers = rpcClient.GetNep5Transfers("NWyPzTPZ8sZEMHa4WC6FfdJ1ieTamTvKiE", 1468595301000/*, 1578971382894*/);
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
                    rpcTx = rpcClient.GetRawTransaction(txHash);
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
