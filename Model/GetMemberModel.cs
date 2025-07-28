using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class GetMemberModel
    {
        public List<Member> data { get; set; }
    }
    public class Member
    {
        public int member_id { get; set; } = 0;
        public string member_name { get; set; }
        public string member_email { get; set; }
        public string member_phone_number { get; set; }
        public int member_points { get; set; } = 0;
        public string updated_at { get; set; }
    }
    public class MemberBonusSettings
    {
        public int point_percentage { get; set; }
    }
}
