using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KASIR.Model
{
    public class OutletData
    {
        public DataOutlet Data { get; set; }
    }

    public class DataOutlet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Pin { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsKitchenBarMerged { get; set; }
        public string Footer { get; set; }
    }
    


 }
