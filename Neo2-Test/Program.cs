using System;

namespace Neo2_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Transfer.SendTrans();

            //DeployContract.Deploy();

            //InvokeContract.Nep5Transfer();

            Console.ReadKey();
        }


    }
}
