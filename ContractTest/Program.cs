using Akka.Actor;
using Neo;
using Neo.IO;
using Neo.IO.Json;
using Neo.Network.P2P;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.Wallets;
using Neo.Wallets.NEP6;
using Neo.Wallets.SQLite;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ContractTest
{
    class Program
    {
        private static Wallet currentWallet;

        public static bool ReadingPassword { get; private set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Signtest();
            //MintTokenTest();

            Console.ReadKey();
        }

        public static void Signtest()
        {
            string wif = "KxYgqToYbDwrtfuNC5AyCoXwNDSqFTwKyrQPm18RHotRA1AXcNVP";


            byte[] prikey = Wallet.GetPrivateKeyFromWIF(wif);
            KeyPair keyPair = new KeyPair(prikey);
            string message = "Thesekindofcontrolsaregoodweverecommendedbeforebuttheyarespecificto";

            byte[] byteMessage = Encoding.UTF8.GetBytes(message);
            var signData = Neo.Cryptography.Crypto.Sign(byteMessage, keyPair.PrivateKey, keyPair.PublicKey.EncodePoint(false)[1..]);

            Console.WriteLine("message:" + Convert.ToBase64String(byteMessage));
            Console.WriteLine("public key:" + Convert.ToBase64String(keyPair.PublicKey.ToArray()));
            Console.WriteLine("sign data:"+ Convert.ToBase64String(signData));
        }

        public static void MintTokenTest()
        {
            OpenWallet();

            string URL = "http://localhost:10332";
            //string wif = "KxYgqToYbDwrtfuNC5AyCoXwNDSqFTwKyrQPm18RHotRA1AXcNVP";
            //byte[] prikey = Wallet.GetPrivateKeyFromWIF(wif);
            //KeyPair keyPair = new KeyPair(prikey);
           
            WalletAccount account = currentWallet.GetAccounts().First();

            UInt160 contractHash = UInt160.Parse("0x6fc61f9de831ada4e2aafd65cb34f2e35a2e037d");
            string operation = "mint";
            UInt160 asset = UInt160.Parse("0x8c23f196d8a1bfd103a9dcb1f9ccf0c611377d3b");
            UInt160 to = UInt160.Parse("0x1f177332c467db9ba734d3ca85645fbadd7e13e3");
            string amount = "1000";

            Transaction tx;            
            if (!BigDecimal.TryParse(amount, 8, out BigDecimal decimalAmount) || decimalAmount.Sign <= 0)
            {
                Console.WriteLine("Incorrect Amount Format");
                return;
            }
            tx = currentWallet.MakeTransaction(new[]
            {
                new TransferOutput
                {
                    AssetId = asset,
                    Value = decimalAmount,
                    ScriptHash = to
                }
            });            

            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitAppCall(contractHash, operation);
                tx.Script = scriptBuilder.ToArray();
                Console.WriteLine($"Invoking script with: '{tx.Script.ToHexString()}'");
            }

            try
            {
                Random rand = new Random();
                tx.Version = 0;
                tx.Nonce = (uint)rand.Next();
                tx.Sender = account.ScriptHash;
                tx.Cosigners = new Cosigner[] { new Cosigner() { Account = account.ScriptHash } };
                //tx = currentWallet.MakeTransaction(tx.Script, null, tx.Attributes, tx.Cosigners);

            }

            catch (InvalidOperationException)
            {
                Console.WriteLine("Error: insufficient balance.");
                return;
            }

            using (ApplicationEngine engine = ApplicationEngine.Run(tx.Script, tx, testMode: true))
            {
                Console.WriteLine($"VM State: {engine.State}");
                Console.WriteLine($"Gas Consumed: {new BigDecimal(engine.GasConsumed, NativeContract.GAS.Decimals)}");
                Console.WriteLine($"Evaluation Stack: {new Neo.IO.Json.JArray(engine.ResultStack.Select(p => p.ToParameter().ToJson()))}");
                Console.WriteLine();
                if (engine.State.HasFlag(VMState.FAULT))
                {
                    Console.WriteLine("Engine faulted.");
                    return;
                }
            }

            ContractParametersContext context = new ContractParametersContext(tx);
            currentWallet.Sign(context);

            string msg;
            if (context.Completed)
            {
                tx.Witnesses = context.GetWitnesses();

                var data = GetRawData(tx);

                var hexStr = data.ToHexString();

                var result = Helper.RpcPost(URL, "sendrawtransaction", new JValue(hexStr));

                Console.WriteLine(result);

                msg = $"Signed and relayed transaction with hash={tx.Hash}";
                Console.WriteLine(msg);

            }
        }

        public static byte[] GetRawData(Transaction tx)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                Serialize(writer,tx);
                writer.Flush();
                return ms.ToArray();
            }
        }


        static void Serialize(BinaryWriter writer, Transaction tx)
        {
            SerializeUnsigned(writer,tx);
            writer.Write(tx.Witnesses);
        }

        static void SerializeUnsigned(BinaryWriter writer, Transaction tx)
        {
            writer.Write(tx.Version);
            writer.Write(tx.Nonce);
            writer.Write(tx.Sender.ToArray());
            writer.Write(tx.SystemFee);
            writer.Write(tx.NetworkFee);
            writer.Write(tx.ValidUntilBlock);
            writer.Write(tx.Attributes);
            writer.Write(tx.Cosigners);
            writer.WriteVarBytes(tx.Script);
        }       

        public static void OpenWallet()
        {
            Console.Write("wallet path:");
            string path = Console.ReadLine();
           
            string password = ReadUserInput("password", true);

            if (password.Length == 0)
            {
                Console.WriteLine("cancelled");
                return;
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            if (Path.GetExtension(path) == ".db3")
            {
                currentWallet = UserWallet.Open(path, password);
            }
            else
            {
                NEP6Wallet nep6wallet = new NEP6Wallet(path);
                nep6wallet.Unlock(password);
                currentWallet = nep6wallet;
            }
        }


        public static string ReadUserInput(string prompt, bool password = false)
        {
            const string t = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
            StringBuilder sb = new StringBuilder();
            ConsoleKeyInfo key;

            if (!string.IsNullOrEmpty(prompt))
            {
                Console.Write(prompt + ": ");
            }

            if (password) ReadingPassword = true;
            var prevForeground = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            if (Console.IsInputRedirected)
            {
                // neo-gui Console require it
                sb.Append(Console.ReadLine());
            }
            else
            {
                do
                {
                    key = Console.ReadKey(true);

                    if (t.IndexOf(key.KeyChar) != -1)
                    {
                        sb.Append(key.KeyChar);
                        if (password)
                        {
                            Console.Write('*');
                        }
                        else
                        {
                            Console.Write(key.KeyChar);
                        }
                    }
                    else if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                    {
                        sb.Length--;
                        Console.Write("\b \b");
                    }
                } while (key.Key != ConsoleKey.Enter);
            }

            Console.ForegroundColor = prevForeground;
            if (password) ReadingPassword = false;
            Console.WriteLine();
            return sb.ToString();
        }
    }
}
