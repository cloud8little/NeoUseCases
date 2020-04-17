using Neo;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.SmartContract.Manifest;
using Neo.Wallets;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Neo.IO;

namespace Converter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string address = "NfxTs1w2q9PqkQmExYgsqQzpXDTg4QcuqX";
            UInt160 scriptHash = address.ToScriptHash();
            Console.WriteLine(scriptHash.ToString());
            string addrHash = scriptHash.ToString();

            ////big-endian 2 little-endian
            //string littleEndianHash = addrHash.Remove(0, 2).HexToBytes().Reverse().ToArray().ToHexString();
            //Console.WriteLine(littleEndianHash);
            ////little-endian 2 big-endian
            //Console.WriteLine("0x" + littleEndianHash.HexToBytes().Reverse().ToArray().ToHexString());

            //var bytes = addrHash.Remove(0, 2).HexToBytes();

            //string addr = new UInt160(bytes.Reverse().ToArray()).ToAddress();


            //Base64ByteArrayToBigInteger("OJAN");

            //Base64HexStringToString("VIQFA");

            //Base64ByteArrayToAddress("iXcg2M129PAKv6N8Dt2InCCP3ps7fTcRxvDM+bHcqQPRv6HYlvEjjPp5djuGdntCaHI0n9L9vM8WLuIg");

            //Console.WriteLine(Encoding.Default.GetString(Convert.FromBase64String("Z25haHpwaXJn").ToArray()));

            //Console.WriteLine(BytesAsString("a3dcf73fe594e5ecd52746e1cfab2da18387d2a2".HexToBytes().Reverse().ToArray()));

            Console.WriteLine(ScriptHashAsByteArray("0x20e22e16cfbcfdd29f347268427b76863b7679fa"));



           //Console.WriteLine(Encoding.Default.GetString(Convert.FromBase64String("iXcg2M129PAKv6N8Dt2InCCP3ps7fTcRxvDM+bHcqQPRv6HYlvEjjPp5djuGdntCaHI0n9L9vM8WLuIg").ToArray()));
            //var aa = Convert.FromBase64String("EiMy").ToArray();

            //Console.WriteLine(Convert.FromBase64String("VvwAK/RVX3xIoY/dnK3hANtiYHU=").ToArray().ToHexString());
            //Console.WriteLine(Convert.FromBase64String("VvwAK/RVX3xIoY/dnK3hANtiYHU=").ToArray().ToHexString());

            //BigAndLittleEndExchange("9bde8f209c88dd0e7ca3bf0af0f476cdd8207789");

            //LoadScript("E:\\neo_code\\neo-devpack-dotnet-nep5-template\\templates\\Template.NEP5.CSharp\\bin\\Debug\\netstandard2.1\\Template.NEP5.CSharp.nef");

            Console.ReadKey();
        }

        private static string ScriptHashAsByteArray(string scriptHash)
        {
            if (scriptHash.StartsWith("0x"))
                scriptHash = scriptHash.Substring(2);

            string str = "new byte[] { ";

            while (scriptHash.Length > 0)
            {
                str += "0x" + scriptHash.Substring(0, 2);
                if (scriptHash.Length > 2)
                    str += ", ";
                scriptHash = scriptHash.Substring(2);
            }

            str += " }";

            return str;
        }

        private static string BytesAsString(byte[] bytes)
        {
            string str = "new byte[] { ";
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i < bytes.Length - 1)
                    str += bytes[i] + ", ";
                else
                    str += bytes[i] + " }";
            }            

            return str;
        }

        private static void BigAndLittleEndExchange(string str)
        {
            //String args[1] = "c97e324bac15a4ea589f423e4b29a7210b8fad09";
            try
            {
                String reverse = str.HexToBytes().Reverse().ToArray().ToHexString();
                Console.WriteLine("LitteleEnd <=> BigEnd: " + reverse);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {Environment.NewLine}{e.Message}");
            }
            Console.WriteLine();
        }

        static void Base64ByteArrayToBigInteger(string str)
        {
            String littleEnd = Convert.FromBase64String(str).ToArray().ToHexString();
            String bigEnd = Convert.FromBase64String(str).Reverse().ToArray().ToHexString();
            var number = System.Numerics.BigInteger.Parse(bigEnd, System.Globalization.NumberStyles.AllowHexSpecifier);
            Console.WriteLine("LittleEnd: " + littleEnd);
            Console.WriteLine("BigEnd: " + bigEnd);
            Console.WriteLine("BigInteger number:" + number);
        }

        static void Base64HexStringToString(string str)
        {
            Console.WriteLine(System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(str)));
        }

        private static void Base64ByteArrayToAddress(string str)
        {
            byte[] result = Convert.FromBase64String(str).Reverse().ToArray();
            String hex = result.ToHexString();
            var scripthash = UInt160.Parse(hex);
            String address = Neo.Wallets.Helper.ToAddress(scripthash);
            Console.WriteLine("Hex: " + hex);
            Console.WriteLine("Standard Address: " + address);
        }

        private static void LoadScript(string nefFilePath)
        {
            var manifestFilePath = Path.ChangeExtension(nefFilePath, ".manifest.json");
            var info = new FileInfo(manifestFilePath);
            if (!info.Exists || info.Length >= Transaction.MaxTransactionSize)
            {
                throw new ArgumentException(nameof(manifestFilePath));
            }

            var manifest = ContractManifest.Parse(File.ReadAllBytes(manifestFilePath));

            info = new FileInfo(nefFilePath);
            if (!info.Exists || info.Length >= Transaction.MaxTransactionSize)
            {
                throw new ArgumentException(nameof(nefFilePath));
            }

            NefFile file;
            using (var stream = new BinaryReader(File.OpenRead(nefFilePath), Encoding.UTF8, false))
            {
                file = stream.ReadSerializable<NefFile>();
            }

            //var script = file.Script.ToArray().ToHexString();
            Console.WriteLine("script:" + Convert.ToBase64String(file.Script));

            Console.WriteLine("manifest:" + manifest.ToString().Replace("\"","\\\""));
        }
    }
}
