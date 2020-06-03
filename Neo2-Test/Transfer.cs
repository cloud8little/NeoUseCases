using Neo;
using Neo.Cryptography;
using Neo.IO;
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
    public static class Transfer
    {
        public static string gas_hash = "602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
        public static string api = "http://127.0.0.1:20332/?jsonrpc=2.0&id=1&";

        public static void SendTrans()
        {
            byte[] prikey = Wallet.GetPrivateKeyFromWIF("L2byQYg4BCmV2Eu6eXs3fvwtgXZkjdAWRX4v7QryMPmN67dvapct");
            KeyPair keyPair = new KeyPair(prikey);

            var contract = Contract.CreateSignatureContract(keyPair.PublicKey);           

            string toAddress = "ANX5ewXRuKmxpKxqgjVqA6JYdm7kW88LBx";

            //构建交易
            ContractTransaction tx = MakeTran(toAddress, contract.Address, 2.22m);

            tx = GetWitness(keyPair, tx, contract.ScriptHash);

            Console.WriteLine("txid: " + tx.Hash.ToString());

            byte[] data = tx.ToArray();
            string rawdata = data.ToHexString();

            string result = Helper.InvokeRpc("sendrawtransaction", rawdata);

            Console.WriteLine(result.ToString());
        }

        public static ContractTransaction MakeTran(string targetAddr, string myAddr, decimal amount)
        {
            List<Utxo> gasList = Helper.GetGasBalanceByAddress(myAddr);

            var tx = new ContractTransaction();
            tx.Attributes = new TransactionAttribute[] { };
            tx.Version = 0; //
            tx.Inputs = new CoinReference[] { };
            tx.Outputs = new TransactionOutput[] { };
            tx.Witnesses = new Witness[] { };
           
            decimal sys_fee = 0;

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

        public static ContractTransaction GetWitness(KeyPair keyPair, ContractTransaction tx, UInt160 scriptHash)
        {
            Random random = new Random();
            var nonce = new byte[32];
            random.NextBytes(nonce);
            TransactionAttribute[] attributes = new TransactionAttribute[]
            {
                new TransactionAttribute() { Usage = TransactionAttributeUsage.Script, Data = scriptHash.ToArray() },
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

            return tx;
        }

    }
}
