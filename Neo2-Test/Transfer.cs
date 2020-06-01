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
using System.Text;

namespace Neo2_Test
{
    public static class Transfer
    {
        public static string gas_hash = "602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
        public static string api = "http://127.0.0.1:20332/?jsonrpc=2.0&id=1&";

        public static void SendTrans()
        {
            byte[] prikey = Wallet.GetPrivateKeyFromWIF("L2byQYg4BCmV2Eu6eXs3fvwtgXZkjdAWRX4v7QryMPmN67dvapct");
            KeyPair keyPair = new KeyPair(prikey);

            var contract = Contract.CreateSignatureContract(keyPair.PublicKey);
            string address = contract.Address;

            string toAddress = "ANX5ewXRuKmxpKxqgjVqA6JYdm7kW88LBx";

            List<Utxo> gasList = GetGasBalanceByAddress(api, address);

            //构建交易
            InvocationTransaction tx = MakeTran(gasList, toAddress, 2.2m, address);

            tx.Script = new byte[] { };
            tx.Gas = Fixed8.FromDecimal(0.1m);

            Random random = new Random();
            var nonce = new byte[32];
            random.NextBytes(nonce);
            TransactionAttribute[] attributes = new TransactionAttribute[]
            {
                new TransactionAttribute() { Usage = TransactionAttributeUsage.Script, Data = contract.Address.ToScriptHash().ToArray() },
                new TransactionAttribute() { Usage = TransactionAttributeUsage.Remark1, Data = nonce } // if a transaction has no inputs and outputs, need to add nonce for duplication
            };

            tx.Attributes = attributes;

            var signature = tx.Sign(keyPair);
            var sb = new ScriptBuilder();
            sb = sb.EmitPush(signature);
            var invocationScript = sb.ToArray();

            var verificationScript = Contract.CreateSignatureRedeemScript(keyPair.PublicKey);
            Witness witness = new Witness() { InvocationScript = invocationScript};

            tx.Witnesses = new[] { witness };

            Console.WriteLine("txid: " + tx.Hash.ToString());

            byte[] data = tx.GetHashData();
            string rawdata = data.ToHexString();

            string result = Helper.InvokeRpc(api, "sendrawtransaction", rawdata);

            Console.WriteLine(result.ToString());
        }

        public static InvocationTransaction MakeTran(List<Utxo> gasList, string targetAddr, decimal sendCount, string changeAddr)
        {
            var tx = new InvocationTransaction();
            tx.Attributes = new TransactionAttribute[0];
            tx.Version = 0;
            tx.Outputs = new TransactionOutput[] { };
            tx.Witnesses = new Witness[] { };

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
