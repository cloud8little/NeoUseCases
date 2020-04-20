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
        public KeyPair keyPair0 = Neo.Network.RPC.Utility.GetKeyPair("KxYgqToYbDwrtfuNC5AyCoXwNDSqFTwKyrQPm18RHotRA1AXcNVP");


        //1-4 Multisig Addr: Nf3PBXyVc7vrL19egUUvJhWfHuBP2mLGPX
        //address: NYzbJ4vhBXmk9QBDtpCiVgF98QJxsJBYFC
        //pubkey: 0378c824e55a8b906adcc6981fdda1e1062e000aeee03fc74f4e9d9b07fd6062ab
        //ScriptHash: 0x50255c4a704b280d1009cacd275216e1f070798f
        public KeyPair keyPair1 = Neo.Network.RPC.Utility.GetKeyPair("Kx4G77Esuo9b8wXyDeagvJxWKPw7kK5GfVR1A5JDFqxtbNGRiWzu");

        //address:  
        //pubkey: 02bee9d67363d5d5834c5b307994e7e6b5411b4653ddb3c4fe74b270e1418f6a15
        //ScriptHash: 0x87e3b7c7007cc71f3482f29d2ceed612addb74f8
        public KeyPair keyPair2 = Neo.Network.RPC.Utility.GetKeyPair("L2F7F6HY3CtLZPUwpLeqFkfGVmTchjEBTozi9gB1NScv3aiRoEfa");

        //address: Ndc3N6vyczYrqDwRgdYWPnV5hgNbK3GmqC
        //pubkey: 03598066d0df7afaa51524ae76c8e2eb63c35e96f8d826cde21aeadf3e0e1667b6
        //ScriptHash: 0x313afcd9a4456df0a6233b017692657ac6430ec2
        public KeyPair keyPair3 = Neo.Network.RPC.Utility.GetKeyPair("L2hVUr3bV9t2YtF2gcBdtCTPLDhXojFYFwiNKU6LN3G3KJkzGeJd");

        //address: NYhF4c1KxeRgJXpaAoeiz5BGf4hQazjBpc
        //pubkey: 032d562db5cad9862483205be6b231bf4d12521eeac8e77a56c540d41d255d94c3
        //ScriptHash: 0x6c6ae0bd6be2c567c8254af44f0566451b7b318c
        public KeyPair keyPair4 = Neo.Network.RPC.Utility.GetKeyPair("L149sPD3K46UA1uVcqM5m9xm74bLaLPP34coqLqEvwd2WAvEeFa2");

        //address: NQxFrTi8dNX1SoeGU8s4hu7ZyMFNwBXyNw
        //pubkey: 03d49395361e5692959f237457ebdf3c68d1b2a748632ca7cc43b96949581baeee
        //ScriptHash: 0x16962562fc3c5e0f7f8ece375edfc792c05f4737
        public KeyPair keyPair5 = Neo.Network.RPC.Utility.GetKeyPair("L3Ke1RSBycXmRukv27L6o7sQWzDwDbFcbfR9oBBwXbCKHdBvb4ZM");


        public UInt160 neo = NativeContract.NEO.Hash;

        public UInt160 gas = NativeContract.GAS.Hash;

        public RpcClient RpcClient;

        public abstract void Run();
        
    }
}
