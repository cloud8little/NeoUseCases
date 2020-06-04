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
        public static string api = "http://127.0.0.1:20332/?jsonrpc=2.0&id=1&";

        public static void SendGasTrans()
        {
            byte[] prikey = Wallet.GetPrivateKeyFromWIF("L2byQYg4BCmV2Eu6eXs3fvwtgXZkjdAWRX4v7QryMPmN67dvapct");
            KeyPair keyPair = new KeyPair(prikey);

            var contract = Contract.CreateSignatureContract(keyPair.PublicKey);           

            string toAddress = "ANX5ewXRuKmxpKxqgjVqA6JYdm7kW88LBx";

            //构建交易
            ContractTransaction tx = MakeTran(toAddress, contract.Address, 2.22m);

            tx.Attributes = Helper.GetAttribute(contract.ScriptHash);

            var signature = tx.Sign(keyPair);

            tx.Witnesses = Helper.GetWitness(signature, keyPair.PublicKey);

            Console.WriteLine("txid: " + tx.Hash.ToString());

            byte[] data = tx.ToArray();
            string rawdata = data.ToHexString();

            string result = Helper.InvokeRpc("sendrawtransaction", rawdata);

            Console.WriteLine(result.ToString());
        }

        internal static void SendNeoTrans()
        {
            byte[] prikey = Wallet.GetPrivateKeyFromWIF("L2byQYg4BCmV2Eu6eXs3fvwtgXZkjdAWRX4v7QryMPmN67dvapct");
            KeyPair keyPair = new KeyPair(prikey);

            var contract = Contract.CreateSignatureContract(keyPair.PublicKey);

            string toAddress = "ANX5ewXRuKmxpKxqgjVqA6JYdm7kW88LBx";

            //构建交易
            ContractTransaction tx = MakeNeoTran(toAddress, contract.Address, 3);

            tx.Attributes = Helper.GetAttribute(contract.ScriptHash);

            var signature = tx.Sign(keyPair);

            tx.Witnesses = Helper.GetWitness(signature, keyPair.PublicKey);

            Console.WriteLine("txid: " + tx.Hash.ToString());

            byte[] data = tx.ToArray();
            string rawdata = data.ToHexString();

            string result = Helper.InvokeRpc("sendrawtransaction", rawdata);

            Console.WriteLine(result.ToString());
        }

        private static ContractTransaction MakeNeoTran(string toAddress, string address, int amount)
        {            
            List<Utxo> neoList = Helper.GetNeoBalanceByAddress(address);

            var tx = new ContractTransaction();
            tx.Attributes = new TransactionAttribute[] { };
            tx.Version = 0; //
            tx.Inputs = new CoinReference[] { };
            tx.Outputs = new TransactionOutput[] { };
            tx.Witnesses = new Witness[] { };

            neoList.Sort((a, b) =>
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
            for (int i = 0; i < neoList.Count; i++)
            {
                CoinReference coin = new CoinReference();
                coin.PrevHash = neoList[i].txid;
                coin.PrevIndex = (ushort)neoList[i].n;
                coinList.Add(coin);
                count += neoList[i].value;
                if (count >= amount)
                    break;
            }

            tx.Inputs = coinList.ToArray();

            if (count >= amount)
            {
                List<TransactionOutput> list_outputs = new List<TransactionOutput>();
                if (amount > decimal.Zero && toAddress != null)
                {
                    TransactionOutput output = new TransactionOutput();
                    output.AssetId = UInt256.Parse(Helper.neo_hash);
                    output.Value = Fixed8.FromDecimal(amount);
                    output.ScriptHash = toAddress.ToScriptHash();
                    list_outputs.Add(output);
                }

                //找零
                var change = count - amount;
                if (change > decimal.Zero)
                {
                    TransactionOutput outputchange = new TransactionOutput();
                    outputchange.AssetId = UInt256.Parse(Helper.neo_hash);
                    outputchange.ScriptHash = address.ToScriptHash();
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
                    output.AssetId = UInt256.Parse(Helper.gas_hash);
                    output.Value = Fixed8.FromDecimal(gas_consumed);
                    output.ScriptHash = targetAddr.ToScriptHash();
                    list_outputs.Add(output);
                }

                //找零
                var change = count - gas_consumed;
                if (change > decimal.Zero)
                {
                    TransactionOutput outputchange = new TransactionOutput();
                    outputchange.AssetId = UInt256.Parse(Helper.gas_hash);
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

    }
}
