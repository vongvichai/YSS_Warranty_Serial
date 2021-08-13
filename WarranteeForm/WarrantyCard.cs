using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarranteeForm
{
    public class WarrantyCard
    {
        public string product_code { get; set; }
        public string serial_numbers { get; set; }
        public byte[] qr_code { get; set; }
    }
}
