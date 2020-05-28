using Neo;
using Neo.Cryptography;
using Neo.Network.P2P;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Neo2_Test
{
    public static class DeployContract
    {
        public static string gas_hash = "602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
        public static string api = "http://127.0.0.1:20332/?jsonrpc=2.0&id=1&";

        public static void Deploy()
        {
            byte[] prikey = Wallet.GetPrivateKeyFromWIF("L2byQYg4BCmV2Eu6eXs3fvwtgXZkjdAWRX4v7QryMPmN67dvapct");
            KeyPair keyPair = new KeyPair(prikey);
            var contract = Contract.CreateSignatureContract(keyPair.PublicKey);
            string address = contract.Address;
           
            List<Utxo> gasList = GetGasBalanceByAddress(api, address);

            //从文件中读取合约脚本
            byte[] contractData = System.IO.File.ReadAllBytes("PEG.avm"); //这里填你的合约所在地址

            UInt160 contract_hash = new UInt160(Crypto.Default.Hash160(contractData));

            Console.WriteLine("合约脚本hash：" + contract_hash.ToString()); //合约 hash

            var tx = MakeTransaction(address, gasList, contractData);
            
            ContractParametersContext context = new ContractParametersContext(tx);
            byte[] signature = tx.Sign(keyPair);
            context.AddSignature(contract, keyPair.PublicKey, signature);

            if (context.Completed)
            {
                tx.Witnesses = context.GetWitnesses();
            }

            Console.WriteLine("txid: " + tx.Hash.ToString());

            byte[] data = tx.GetHashData();
            string rawdata = data.ToHexString();

            string input = @"{
	            'jsonrpc': '2.0',
                'method': 'sendrawtransaction',
	            'params': ['#'],
	            'id': '1'
                }";
            input = input.Replace("#", rawdata);

            string result = Helper.HttpPost(api, input);

            Console.WriteLine(result.ToString());
        }


        private static InvocationTransaction MakeTransaction(string address, List<Utxo> gasList, byte[] contractData)
        {
            byte[] parameter__list = "0710".HexToBytes();  //合约入参类型  例：0610 代表（string，[]）参考：http://docs.neo.org/zh-cn/sc/Parameter.html
            byte[] return_type = "05".HexToBytes();  //合约返回值类型 05 代表 ByteArray
            int need_storage = 1; //是否需要使用存储 0 false 1 true
            int need_nep4 = 0; //是否需要动态调用 0 false 2 true
            int need_canCharge = 4; //是否支持收款 4 true

            byte[] script;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                //倒序插入数据
                sb.EmitPush("test"); //description
                sb.EmitPush("xxx@neo.com"); //email
                sb.EmitPush("test"); //auther
                sb.EmitPush("1.0");  //version
                sb.EmitPush("ABC Coin"); //name
                sb.EmitPush(need_storage | need_nep4 | need_canCharge);
                sb.EmitPush(return_type);
                sb.EmitPush(parameter__list);
                sb.EmitPush(contractData);
                sb.EmitSysCall("Neo.Contract.Create");

                script = sb.ToArray();
            }
            string deployData = script.ToHexString();

            string input = @"{
	            'jsonrpc': '2.0',
                'method': 'invokescript',
	            'params': ['#'],
	            'id': '1'
                }";

            input = input.Replace("#", deployData);

            //用 ivokescript 试运行得到 gas 消耗
            string result = Helper.HttpPost(api, input);
            var consume = JObject.Parse(result)["result"]["gas_consumed"].ToString();
            decimal gas_consumed = decimal.Parse(consume);

            //拼交易
            var tx = MakeTran(gasList, null, gas_consumed - 10, address);
            tx.Script = script;

            return tx;
        }


        private static List<Utxo> GetGasBalanceByAddress(string api, string address)
        {
            JObject response = JObject.Parse(Helper.HttpGet(api + "method=getunspents&params=['" + address + "']"));
            JArray resJA = (JArray)response["result"]["balance"];

            List<Utxo> Utxos = new List<Utxo>();

            foreach (JObject jAsset in resJA)
            {
                var asset_hash = jAsset["asset_hash"].ToString();
                if (asset_hash != gas_hash)
                    continue;
                var jUnspent = jAsset["unspent"] as JArray;

                foreach (JObject j in jUnspent)
                {
                    Utxo utxo = new Utxo(UInt256.Parse(j["txid"].ToString()), decimal.Parse(j["value"].ToString()), int.Parse(j["n"].ToString()));

                    Utxos.Add(utxo);                    
                }
            }
            return Utxos;
        }


        public static InvocationTransaction MakeTran(List<Utxo> gasList, string targetAddr, decimal sendCount, string changeAddr)
        {            
            var tx = new InvocationTransaction();
            tx.Attributes = new TransactionAttribute[0];
            tx.Version = 0;           

            gasList.Sort((a, b) =>
            {
                if (a.value > b.value)
                    return 1;
                else if (a.value < b.value)
                    return -1;
                else
                    return 0;
            });

            decimal count = decimal.Zero;

            List<CoinReference> coinList = new List<CoinReference>();
            for (int i = 0; i < gasList.Count; i++)
            {
                CoinReference coin = new CoinReference();
                coin.PrevHash = gasList[i].txid;
                coin.PrevIndex = (ushort)gasList[i].n;
                coinList.Add(coin);
                count += gasList[i].value;
                if (count >= sendCount)
                    break;
            }

            tx.Inputs = coinList.ToArray();

            if (count >= sendCount)
            {
                List<TransactionOutput> list_outputs = new List<TransactionOutput>();
                if (sendCount > decimal.Zero && targetAddr != null)
                {
                    TransactionOutput output = new TransactionOutput();
                    output.AssetId = UInt256.Parse(gas_hash);
                    output.Value = Fixed8.FromDecimal(sendCount);
                    output.ScriptHash = targetAddr.ToScriptHash();
                    list_outputs.Add(output);
                }

                var change = count - sendCount;
                if (change > decimal.Zero)
                {
                    TransactionOutput outputchange = new TransactionOutput();
                    outputchange.AssetId = UInt256.Parse(gas_hash);
                    outputchange.ScriptHash = changeAddr.ToScriptHash();
                    outputchange.Value = Fixed8.FromDecimal(change);
                    list_outputs.Add(outputchange);
                }

                tx.Outputs = list_outputs.ToArray();
            }
            else
            {
                throw new Exception("no enough money!");
            }

            return tx;
        }

    }
}
