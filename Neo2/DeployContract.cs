﻿using Neo;
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
        public static void Deploy()
        {
            byte[] prikey = Wallet.GetPrivateKeyFromWIF("L5VAXN9odUJgCKQ3CB8ebH6EyU3Puoh85wN687XVGHTzwymachFp");
            KeyPair keyPair = new KeyPair(prikey);

            var contract = Contract.CreateSignatureContract(keyPair.PublicKey);

            //从文件中读取合约
            byte[] contractData = System.IO.File.ReadAllBytes("nep5.avm"); //这里填你的合约 avm 所在地址

            UInt160 contract_hash = new UInt160(Crypto.Default.Hash160(contractData));//合约 hash

            Console.WriteLine("合约脚本hash：" + contract_hash.ToString());

            //构建交易
            InvocationTransaction tx = MakeTransaction(contract.Address, contractData);

            tx.Attributes = Helper.GetAttribute(contract.ScriptHash);

            var signature = tx.Sign(keyPair);

            tx.Witnesses = Helper.GetWitness(signature, keyPair.PublicKey);

            Console.WriteLine("txid: " + tx.Hash.ToString());

            byte[] data = tx.ToArray();
            string rawdata = data.ToHexString();

            string result = Helper.InvokeRpc("sendrawtransaction", rawdata);

            Console.WriteLine(result.ToString());
        }        

        private static InvocationTransaction MakeTransaction(string myAddress, byte[] contractData)
        {
            byte[] parameter_list = "0710".HexToBytes();  //合约入参类型  例：0610 代表（string，[]）参考：http://docs.neo.org/zh-cn/sc/Parameter.html            
           
            ContractParameterType return_type = "05".HexToBytes().Select(p => (ContractParameterType?)p).FirstOrDefault() ?? ContractParameterType.Void;  //合约返回值类型 05 代表 ByteArray
            ContractPropertyState properties = ContractPropertyState.NoProperty;
            //properties |= ContractPropertyState.HasStorage; //是否需要使用存储 
            //properties |= ContractPropertyState.Payable; //是否支持收款  
            //properties |= ContractPropertyState.HasDynamicInvoke;//支持动态调用

            byte[] script;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitSysCall("Neo.Contract.Create", contractData, parameter_list, return_type, properties, "name", "version", "author", "email", "description");
                script = sb.ToArray();
            }

            //拼交易
            InvocationTransaction tx = Helper.MakeTran(null, myAddress, 0, script);

            return tx;
        }

       
    }
}
