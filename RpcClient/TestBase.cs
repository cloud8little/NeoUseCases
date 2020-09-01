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
        //   Address: NNU67Fvdy3LEQTM374EJ9iMbCRxVExgM8Y
        //ScriptHash: 0xf621168b1fce3a89c33a5f6bcf7e774b4657031c
        // PublicKey: 0222d8515184c7d62ffa99b829aeb4938c4704ecb0dd7e340e842e9df121826343
        public KeyPair keyPair0 = Neo.Network.RPC.Utility.GetKeyPair("L5TNJrPhrvsKhQfjX7Uyc9eaDKSpdZZZXjFmmpDCdm1kQe5ntA25");

        //   Address: NNB8GKS7mdMXXGsAwvXYyEGonkEjDbqNkG
        //ScriptHash: 0xb120f50f804d3a203c43475212894ab1c911ce18
        // PublicKey: 028bd1902c4d1419f002b821e6de653ddfd5358063208e426756398be6ffa3aac8
        public KeyPair keyPair1 = Neo.Network.RPC.Utility.GetKeyPair("L4N1Yh6NkjGqoGqNhCCotpkD1PpAHyYtmC7kE4vWRS9GBNr13bFM");

        //   Address: NiAWtLyWRfVWkUf7WwooiedTaGZ76yPqCu
        //ScriptHash: 0x648a20b684e66833f38769b7b0b993d4680d13f4
        // PublicKey: 02967610b1b4f7b84a861d6e0b79047712c1a4b9638156ce1f3891189fb9e41690
        public KeyPair keyPair2 = Neo.Network.RPC.Utility.GetKeyPair("KyXXWrNkTTZN9kC5SHKLpDkSocx5uzp7o4PdQpjM6hh5okzm891C");


        public UInt160 neo = NativeContract.NEO.Hash;

        public UInt160 gas = NativeContract.GAS.Hash;

        public RpcClient RpcClient;

        public abstract void Run();
        
    }
}
