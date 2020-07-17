using Neo;
using Neo.Network.RPC;
using Neo.SmartContract.Native;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Text;

namespace RpcClientTest
{
    abstract class TestBase
    {
        //address: NikMd2j2bgVr8HzYgoJjbnwUPyXWnzjDCM
        //pubkey: 0336df43a1a74b2d2cedb97f9919489621dd2e2b37276361474c7030ffc25e2aa7
        //ScriptHash: 0x20e22e16cfbcfdd29f347268427b76863b7679fa
        public KeyPair keyPair0 = Neo.Network.RPC.Utility.GetKeyPair("L5TNJrPhrvsKhQfjX7Uyc9eaDKSpdZZZXjFmmpDCdm1kQe5ntA25");


        //1-4 Multisig Addr: Nf3PBXyVc7vrL19egUUvJhWfHuBP2mLGPX
        //address: NYzbJ4vhBXmk9QBDtpCiVgF98QJxsJBYFC
        //pubkey: 0378c824e55a8b906adcc6981fdda1e1062e000aeee03fc74f4e9d9b07fd6062ab
        //ScriptHash: 0x50255c4a704b280d1009cacd275216e1f070798f
        public KeyPair keyPair1 = Neo.Network.RPC.Utility.GetKeyPair("L4N1Yh6NkjGqoGqNhCCotpkD1PpAHyYtmC7kE4vWRS9GBNr13bFM");

        //address:  
        //pubkey: 02bee9d67363d5d5834c5b307994e7e6b5411b4653ddb3c4fe74b270e1418f6a15
        //ScriptHash: 0x87e3b7c7007cc71f3482f29d2ceed612addb74f8
        public KeyPair keyPair2 = Neo.Network.RPC.Utility.GetKeyPair("KyXXWrNkTTZN9kC5SHKLpDkSocx5uzp7o4PdQpjM6hh5okzm891C");


        public UInt160 neo = NativeContract.NEO.Hash;

        public UInt160 gas = NativeContract.GAS.Hash;

        public RpcClient RpcClient;

        public abstract void Run();
        
    }
}
