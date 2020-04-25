using System;
using System.Collections.Generic;
using System.Text;

namespace FingerprintCore
{
    public struct FpMatch
    {
        public bool matched { get; set; }
        public int ID { get; set; }
        public int MatchScore { get; set; }
    }
}
