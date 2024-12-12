using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class CicilRefundModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public DataCicil data { get; set; }
    }

    public class DataCicil
    {
        public int total_cart { get; set; }
        public int unpaid_balance { get; set; }
        public int paid_balance { get; set; }
        public List<DetailPaid> detail_paid { get; set; }
    }

    public class DetailPaid
    {
        public int id { get; set; }
        public int total { get; set; }
        public string created_at { get; set; }
    }
}
