using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shoprite.Debug
{
    class OrderResend
    {
        public string OriginalXml { get; set; }
        public int VendorId { get; internal set; }
        public int BatchId { get; internal set; }
        public string VendorName { get; internal set; }
        public string VendorPassword { get; internal set; }
    }
}
