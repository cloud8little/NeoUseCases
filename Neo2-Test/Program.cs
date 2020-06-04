using System;

namespace Neo2_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //Transfer.SendGasTrans();

            Transfer.SendNeoTrans();

            //DeployContract.Deploy();

            //InvokeContract.Transfer();

            //InvokeContract.InvokeScript();

            Console.ReadKey();
        }


    }
}
