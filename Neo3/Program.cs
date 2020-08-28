using Neo;
using Neo.IO;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using System;
using System.Linq;
using System.Text;
using Neo.Network.RPC;
using Utility = Neo.Network.RPC.Utility;
using System.Security.Cryptography;

namespace NeoTest
{
    class Program
    {
        private static Wallet currentWallet;

        public static bool ReadingPassword { get; private set; }

        public static RpcClient rpcClient;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            rpcClient = new RpcClient("http://localhost:10332");

            ContractTest();

            //Signtest();
            //MintTokenTest();

            Console.ReadKey();
        }

        private static void ContractTest()
        {
            ContractClient contractClient = new ContractClient(rpcClient);

            UInt160 contractHash = UInt160.Parse("0x1f86b7327fce941efc789fc257d351dcfc9bd0cf");

            var res = contractClient.TestInvoke(contractHash, "name").Stack.Single().ToStackItem().GetString();

            Console.WriteLine(res);


            Console.Write("wif:");
            string wif = Console.ReadLine();

            KeyPair sendKey = Utility.GetKeyPair(wif);
            UInt160 sender = Contract.CreateSignatureContract(sendKey.PublicKey).ScriptHash;

            // add Cosigners, which is a collection of scripthashs that need to be signed
            Cosigner[] cosigners = new[] { new Cosigner { Scopes = WitnessScope.CalledByEntry, Account = sender } };

            UInt160 receiver = UInt160.Parse("0x0ae69efc8c2a3a858238e5434e7a873ead6e8b84");
            byte[] script = contractHash.MakeScript("transfer", sender, receiver, 20032);

            Transaction tx = new TransactionManager(rpcClient, sender).MakeTransaction(script, null, cosigners).AddSignature(sendKey).Sign().Tx;

            var hash = rpcClient.SendRawTransaction(tx);

            Console.WriteLine($"Transaction {hash.ToString()} is broadcasted!");

            // print a message after the transaction is on chain
            WalletAPI neoAPI = new WalletAPI(rpcClient);
            neoAPI.WaitTransaction(tx)
               .ContinueWith(async (p) => Console.WriteLine($"Transaction vm state is {(await p).VMState}"));

            Console.WriteLine(tx.Hash);
        }

        public static void Signtest()
        {
            Console.Write("wif:");
            string wif = Console.ReadLine();

            byte[] prikey = Wallet.GetPrivateKeyFromWIF(wif);
            KeyPair keyPair = new KeyPair(prikey);
            string message = "Thesekindofcontrolsaregoodweverecommendedbeforebuttheyarespecificto";

            byte[] byteMessage = Encoding.UTF8.GetBytes(message);
            var signData = Neo.Cryptography.Crypto.Sign(byteMessage, keyPair.PrivateKey, keyPair.PublicKey.EncodePoint(false)[1..]);

            Console.WriteLine("message:" + Convert.ToBase64String(byteMessage));
            Console.WriteLine("public key:" + Convert.ToBase64String(keyPair.PublicKey.ToArray()));
            Console.WriteLine("sign data:"+ Convert.ToBase64String(signData));
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
