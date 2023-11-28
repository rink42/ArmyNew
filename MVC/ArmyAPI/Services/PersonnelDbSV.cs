using ArmyAPI.Models;
using System;
using System.Data;
using System.Data.SqlClient;


namespace ArmyAPI.Services
{
    public class personnelDbSV
    {
        private readonly DbHelper _dbHelper;

        public personnelDbSV()
        {
            _dbHelper = new DbHelper();
        }

        public MemRes checkMember(string memberId, string memberName)
        {
            MemRes result = new MemRes();
            try
            {
                string sql = "SELECT member_name, member_id FROM Army.dbo.v_member_data WHERE member_id = @member_id";
                SqlParameter[] parameters = { new SqlParameter("@member_id", memberId) };

                DataTable memberInDb = _dbHelper.ArmyExecuteQuery(sql, parameters); 

                if (memberInDb != null && memberInDb.Rows.Count > 0)
                {
                    result = new MemRes
                    {
                        Result = "True",
                        MemberId = memberInDb.Rows[0]["member_id"].ToString(),
                        MemberName = memberInDb.Rows[0]["member_name"].ToString()
                    };

                    if (memberInDb.Rows[0]["member_name"].ToString() == memberName)
                    {
                        return result;
                    }
                    else
                    {
                        result.Result = "False";
                        return result;
                    }
                }
                else
                {
                    result.Result = "Member Not Found";
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Result = "【Fail】" + ex.ToString();
                return result;
            }
        }




    }
}