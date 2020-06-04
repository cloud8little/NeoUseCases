using Neo;
using Neo.Cryptography.ECC;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Neo2_Test
{
    public static class Helper
    {
        public static string gas_hash = "602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
        public static string neo_hash = "c56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";
        public static string api = "http://127.0.0.1:20332/?jsonrpc=2.0&id=1&";


        public static TransactionAttribute[] GetAttribute(UInt160 scriptHash)
        {
            Random random = new Random();
            var nonce = new byte[32];
            random.NextBytes(nonce);
            TransactionAttribute[] attributes = new TransactionAttribute[]
            {
                new TransactionAttribute() { Usage = TransactionAttributeUsage.Script, Data = scriptHash.ToArray() },
                new TransactionAttribute() { Usage = TransactionAttributeUsage.Remark1, Data = nonce }
            };

            return attributes;
        }

        public static Witness[] GetWitness(byte[] signature, ECPoint publicKey)
        {
            var sb = new ScriptBuilder();
            sb = sb.EmitPush(signature);
            var invocationScript = sb.ToArray();

            var verificationScript = Contract.CreateSignatureRedeemScript(publicKey);
            Witness witness = new Witness() { InvocationScript = invocationScript, VerificationScript = verificationScript };

            return new[] { witness };
        }
       
        public static InvocationTransaction MakeTran(string targetAddr, string myAddr, decimal amount, byte[] script)
        {
            List<Utxo> gasList = GetGasBalanceByAddress(myAddr);

            var tx = new InvocationTransaction();
            tx.Attributes = new TransactionAttribute[] { };
            tx.Version = 1; //若花费 sys_fee, version 就是 1
            tx.Inputs = new CoinReference[] { };
            tx.Outputs = new TransactionOutput[] { };
            tx.Witnesses = new Witness[] { };
            tx.Script = script;

            //计算系统费
            string result = InvokeRpc("invokescript", script?.ToHexString());
            var consume = JObject.Parse(result)["result"]["gas_consumed"].ToString();
            decimal sys_fee = decimal.Parse(consume) - 10;

            if (sys_fee <= 0)
            {
                tx.Version = 0;
                sys_fee = 0;
            }

            //计算网络费
            decimal fee = 0;
            if (tx.Size > 1024)
            {
                fee += 0.001m;
                fee += tx.Size * 0.00001m;
            }

            //总费用
            decimal gas_consumed = sys_fee + fee + amount;

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

            //构造 UTXO 的 vin 和 vout
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
                    outputchange.ScriptHash = myAddr.ToScriptHash();
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

        internal static List<Utxo> GetNeoBalanceByAddress(string address)
        {
            JObject response = JObject.Parse(Helper.HttpGet(api + "method=getunspents&params=['" + address + "']"));
            JArray resJA = (JArray)response["result"]["balance"];

            List<Utxo> Utxos = new List<Utxo>();

            foreach (JObject jAsset in resJA)
            {
                var asset_hash = jAsset["asset_hash"].ToString();
                if (asset_hash != neo_hash)
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

        //get GAS UTXO
        public static List<Utxo> GetGasBalanceByAddress(string address)
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

        public static string HttpGet(string url)
        {
            WebClient wc = new WebClient();
            return wc.DownloadString(url);
        }

        public static string InvokeRpc(string method, string data)
        {
            string input = @"{
	            'jsonrpc': '2.0',
                'method': '&',
	            'params': ['#'],
	            'id': '1'
                }";

            input = input.Replace("&", method);
            input = input.Replace("#", data);

            string result = HttpPost(api, input);
            return result;
        }

        public static string HttpPost(string url, string data)
        {
            HttpWebRequest req = WebRequest.CreateHttp(new Uri(url));
            req.ContentType = "application/json;charset=utf-8";

            req.Method = "POST";
            //req.Accept = "text/xml,text/javascript";
            req.ContinueTimeout = 10000;

            byte[] postData = Encoding.UTF8.GetBytes(data);
            Stream reqStream = req.GetRequestStream();
            reqStream.Write(postData, 0, postData.Length);
            //reqStream.Dispose();

            HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();
            string result = GetResponseAsString(rsp);

            return result;
        }

        private static string GetResponseAsString(HttpWebResponse rsp)
        {
            Stream stream = null;
            StreamReader reader = null;

            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, Encoding.UTF8);

                return reader.ReadToEnd();
            }
            finally
            {
                // 释放资源
                if (reader != null)
                    reader.Close();
                if (stream != null)
                    stream.Close();

            }
        }
    }

    public class Utxo
    {
        public UInt256 txid;
        public int n;
        public decimal value;

        public Utxo(UInt256 _txid, decimal _value, int _n)
        {
            txid = _txid;
            value = _value;
            n = _n;
        }
    }

    public class TransactionInput
    {
        public UInt256 hash;
        public int index;
    }
}
