using System;

namespace msgPckDecoder.Common
{
    public class DecodeResult
    {
        // Model type key results that can be possible in this proj
        public string Json { get; set; }
        public string ModelHash { get; set; }
        public DateTime RequestDate { get; set; }
        public int? ItemCount { get; set; }
    }
}