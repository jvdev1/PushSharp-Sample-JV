using PushSharp_Sample;
using System;

namespace PushSharp_Sample_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            PushSharp.Core.Log.AddLogger(new PushLog());

            Console.WriteLine("type anything and press Enter to call Sample Push");

            string input = Console.ReadLine();

            while (input != "quit")
            {
                if (!string.IsNullOrEmpty(input))
                    Process(input);
                input = Console.ReadLine();
            }
        }

        private static void Process(string input)
        {

            PushSharpSample.Send_APNS(
                new string[] { "clfVLKu3HO4:APA91bGs156tsZrp5NX_opibc-phtLtHHK4KMaQ_r9R4pxflCcDZQ61TN-tdoJMkl2lHsD1mvzOnzVk7PoGWWjf3hVCztX0HBrk-zwyBdX0F8Ye7M7Rff9EbHhp6RCHhwfWd18759wju" }
                , "Title 2", "Message 2");
            
            PushSharpSample.Send_GCM(
                new string[] { "clfVLKu3HO4:APA91bGs156tsZrp5NX_opibc-phtLtHHK4KMaQ_r9R4pxflCcDZQ61TN-tdoJMkl2lHsD1mvzOnzVk7PoGWWjf3hVCztX0HBrk-zwyBdX0F8Ye7M7Rff9EbHhp6RCHhwfWd18759wju" }
                , "Title 2", "Message 2");


            Console.WriteLine("that's all");

        }
    }
}
