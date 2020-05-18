﻿using System;
using System.Collections;

namespace OtherTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Example example = new Example();
            example.Test();

            //StackTest();

            Console.ReadKey();
        }

        private static void StackTest()
        {
            Stack stack = new Stack();
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);
            stack.Push(4);

            foreach (int num in stack)
            {
                Console.WriteLine(num); //4321
            }

            while (stack.Count > 0)
            {
                Console.WriteLine(stack.Pop()); // 4321
            }
        }
    }


    public class Example
    {
        // Define an Enum without FlagsAttribute.
        enum SingleHue : short
        {
            None = 0,
            Black = 1,
            Red = 2,
            Green = 4,
            Blue = 8
        };

        // Define an Enum with FlagsAttribute.
        [Flags]
        enum MultiHue : short
        {
            None = 0,
            Black = 1,
            Red = 2,
            Green = 4,
            Blue = 8
        };

        public void Test()
        {
            // Display all possible combinations of values.
            Console.WriteLine(
                 "All possible combinations of values without FlagsAttribute:");
            for (int val = 0; val <= 16; val++)
                Console.WriteLine("{0,3} - {1:G}", val, (SingleHue)val);

            // Display all combinations of values, and invalid values.
            Console.WriteLine(
                 "\nAll possible combinations of values with FlagsAttribute:");
            for (int val = 0; val <= 16; val++)
                Console.WriteLine("{0,3} - {1:G}", val, (MultiHue)val);
        }
    }
}

