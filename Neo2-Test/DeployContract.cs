using Neo;
using Neo.Cryptography;
using Neo.IO;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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

            //从文件中读取合约
            byte[] contractData = System.IO.File.ReadAllBytes("PEG.avm"); //这里填你的合约 avm 所在地址

            UInt160 contract_hash = new UInt160(Crypto.Default.Hash160(contractData));//合约 hash

            Console.WriteLine("合约脚本hash：" + contract_hash.ToString());

            //构建交易
            InvocationTransaction tx = MakeTransaction(address, gasList, contractData);

            Random random = new Random();
            var nonce = new byte[32];
            random.NextBytes(nonce);
            TransactionAttribute[] attributes = new TransactionAttribute[]
            {
                new TransactionAttribute() { Usage = TransactionAttributeUsage.Script, Data = contract.Address.ToScriptHash().ToArray() },
                new TransactionAttribute() { Usage = TransactionAttributeUsage.Remark1, Data = nonce }
            };

            tx.Attributes = attributes;

            //添加签名
            var signature = tx.Sign(keyPair);
            var sb = new ScriptBuilder();
            sb = sb.EmitPush(signature);
            var invocationScript = sb.ToArray();

            var verificationScript = Contract.CreateSignatureRedeemScript(keyPair.PublicKey);
            Witness witness = new Witness() { InvocationScript = invocationScript, VerificationScript = verificationScript };

            tx.Witnesses = new[] { witness };

            Console.WriteLine("txid: " + tx.Hash.ToString());

            byte[] data = tx.ToArray();
            string rawdata = data.ToHexString();

            string result = Helper.InvokeRpc(api, "sendrawtransaction", rawdata);

            Console.WriteLine(result.ToString());
        }

        private static InvocationTransaction MakeTransaction(string address, List<Utxo> gasList, byte[] contractData)
        {
            byte[] parameter__list = "0710".HexToBytes();  //合约入参类型  例：0610 代表（string，[]）参考：http://docs.neo.org/zh-cn/sc/Parameter.html            
           
            ContractParameterType return_type = "05".HexToBytes().Select(p => (ContractParameterType?)p).FirstOrDefault() ?? ContractParameterType.Void;  //合约返回值类型 05 代表 ByteArray
            ContractPropertyState properties = ContractPropertyState.NoProperty;
            properties |= ContractPropertyState.HasStorage; //是否需要使用存储 
            properties |= ContractPropertyState.Payable; //是否支持收款  
            //properties |= ContractPropertyState.HasDynamicInvoke;//支持动态调用

            byte[] script;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitSysCall("Neo.Contract.Create", contractData, parameter__list, return_type, properties, "name", "version", "author", "email", "description");
                script = sb.ToArray();
            }

            //拼交易
            InvocationTransaction tx = MakeTran(gasList, null, address, script);

            return tx;
        }

        public static InvocationTransaction MakeTran(List<Utxo> gasList, string targetAddr, string changeAddr, byte[] script)
        {
            var tx = new InvocationTransaction();
            tx.Attributes = new TransactionAttribute[] { };
            tx.Version = 1; //目前 neo2 的 Version 是 1
            tx.Inputs = new CoinReference[] { };
            tx.Outputs = new TransactionOutput[] { };
            tx.Witnesses = new Witness[] { };          
            tx.Script = script;

            //计算系统费
            string result = Helper.InvokeRpc(api, "invokescript", script.ToHexString());
            var consume = JObject.Parse(result)["result"]["gas_consumed"].ToString();
            decimal sys_fee = decimal.Parse(consume) - 10;

            //计算网络费
            decimal fee = 0;
            if (tx.Size > 1024)
            {
                fee += 0.001m;
                fee += tx.Size * 0.00001m;
            }

            //总费用
            decimal gas_consumed = sys_fee + fee;

            gasList.Sort((a, b) =>
            {
                if (a.value > b.value)
                    return 1;
                else if (a.value < b.value)
                    return -1;
                else
                    return 0;
            });

            tx.Gas = Fixed8.FromDecimal(sys_fee);

            decimal count = decimal.Zero;

            //构造UTXO 的 vin 和 vout
            List<CoinReference> coinList = new List<CoinReference>();
            for (int i = 0; i < gasList.Count; i++)
            {
                CoinReference coin = new CoinReference();
                coin.PrevHash = gasList[i].txid;
                coin.PrevIndex = (ushort)gasList[i].n;
                coinList.Add(coin);
                count += gasList[i].value;
                if (count >= gas_consumed)
                    break;
            }

            tx.Inputs = coinList.ToArray();

            if (count >= gas_consumed)
            {
                List<TransactionOutput> list_outputs = new List<TransactionOutput>();
                if (gas_consumed > decimal.Zero && targetAddr != null)
                {
                    TransactionOutput output = new TransactionOutput();
                    output.AssetId = UInt256.Parse(gas_hash);
                    output.Value = Fixed8.FromDecimal(gas_consumed);
                    output.ScriptHash = targetAddr.ToScriptHash();
                    list_outputs.Add(output);
                }

                //找零
                var change = count - gas_consumed;
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

        //get GAS UTXO
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

    }
}
