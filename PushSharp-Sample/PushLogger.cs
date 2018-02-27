using PushSharp.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PushSharp_Sample
{
    public class PushLog : ILogger
    {
        public static void Write(object obj)
        {
            Debug.WriteLine("<PushLog> " + obj?.ToString() ?? "?");
        }

        public void Write(LogLevel level, string msg, params object[] args)
        {
            Debug.WriteLine("<PushLog> " + $"[{level}] : {msg}");

            if (args?.Length > 0)
            {
                foreach (object obj in args)
                {
                    if (obj is int) continue;
                    Debug.WriteLine("<PushLog> " + $"{obj.ToString()}");
                }
            }
        }
    }
}
