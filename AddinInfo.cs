using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFilesToPostgres
{
    class AddinInfo
    {
        public string Name { get; set; }
        public string Manifest { get; set; }
        public string Assembly { get; set; }
        public string VendorId { get; set; }
        public string RevitVersion { get; set; }
        public bool AddinEnabled { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
