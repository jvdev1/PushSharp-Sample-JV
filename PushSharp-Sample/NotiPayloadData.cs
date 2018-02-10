using System;
using System.Collections.Generic;
using System.Text;

namespace PushSharp_Sample
{
    public class NotiPayloadData
    {
        public Aps aps { get; set; }
    }

    public class Alert
    {
        public string title { get; set; }
        public string body { get; set; }
    }

    public class Aps
    {
        public Alert alert { get; set; }
        public string sound { get; set; }
    }
}
