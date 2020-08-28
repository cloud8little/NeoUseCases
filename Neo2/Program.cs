using Neo.SmartContract;
using Neo.Wallets;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Neo2_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //Transfer.SendGasTrans();

            //Transfer.SendNeoTrans();

            //DeployContract.Deploy();

            //InvokeContract.Transfer();

            //InvokeContract.InvokeScript();

            CreateAccount();

            Console.ReadKey();
        }

        private static void CreateAccount()
        {
            byte[] privateKey = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(privateKey);
            }
            KeyPair key = new KeyPair(privateKey);

            var publicKey = key.PublicKey.ToString();
            var wif = key.Export();
            var contract = Contract.CreateSignatureContract(key.PublicKey);
            var address = contract.Address;
            var scriptHash = contract.ScriptHash;

        }
        
    }
}
