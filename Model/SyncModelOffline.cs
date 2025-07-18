using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace KASIR.Model
{
    public class DataStructShiftOffline
    {
        public List<ShiftReportData> data { get; set; }
    }
    public class ShiftReportData
    {
        public string outlet_id { get; set; }
        public int actual_ending_cash { get; set; }
        public int cash_difference { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public int expected_ending_cash { get; set; }
        public int total_amount { get; set; }
        public int total_discount { get; set; }
        public int shift_number { get; set; }
        public string casher_name { get; set; }

        // Additional properties from the target model
        public string id { get; set; }
        public string start_at { get; set; }
        public string end_at { get; set; }
        public int is_sync { get; set; }
    }

    public class DataStructMemberPoint
    {
        public List<MemberPointModel> data { get; set; }
    }
    public class MemberPointModel
    {
        public string id { get; set; }
        public int points { get; set; }
        public string outlet_id { get; set; }
        public string updated_at { get; set; }
        public string transaction_ref { get; set; }
        public int is_sync { get; set; }
    }
}
