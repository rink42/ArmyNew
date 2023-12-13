using ArmyAPI.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using Humanizer;

namespace ArmyAPI.Services
{
    public class ChangeHierarchical
    {
        private readonly DbHelper _dbHelper;

        public ChangeHierarchical()
        {
            _dbHelper = new DbHelper();
        }

        public HierarchicalRes getNewHierarchical(string rankCode, string supplyRank)
        {
            HierarchicalRes res = new HierarchicalRes();
            try
            {
                //Step 1. 確認日期是否為18日前 -> 是 晉級/否 晉升
                string newSupplyRank;
                string Date = DateTime.Now.ToString("dd");
                int eighteenDate = int.Parse(Date);
                if (eighteenDate < 18)
                {
                    int SupplyRankNum = int.Parse(supplyRank) + 1;
                    newSupplyRank = SupplyRankNum.ToString();
                }
                else
                {
                    newSupplyRank = supplyRank;
                }

                string getHcSql = @"SELECT 
                                        adh.old_supply_point, adh.old_supply_rank, LTRIM(RTRIM(adr1.rank_title)) as 'old_rank_title', adh.new_supply_point, adh.new_supply_rank, LTRIM(RTRIM(adr2.rank_title)) as 'new_rank_title'
                                    FROM 
                                        ArmyWeb.dbo.hierarchical as adh
                                    LEFT JOIN
                                        Army.dbo.rank as adr1 on adr1.rank_code = adh.old_rank_code
                                    LEFT JOIN 
                                        Army.dbo.rank as adr2 on adr2.rank_code = adh.new_rank_code
                                    WHERE 
                                        adh.old_rank_code = @rankCode AND adh.old_supply_rank = @supplyRank";
                SqlParameter[] getHcParameter =
                {
                    new SqlParameter("@rankCode",SqlDbType.VarChar){Value = rankCode},
                    new SqlParameter("@supplyRank",SqlDbType.VarChar){Value = newSupplyRank}
                };

                DataTable getHcTb = _dbHelper.ArmyWebExecuteQuery(getHcSql, getHcParameter);
                if (getHcTb == null || getHcTb.Rows.Count == 0) 
                {
                    getHcParameter = new SqlParameter[]
                    {
                        new SqlParameter("@rankCode",SqlDbType.VarChar){Value = rankCode},
                        new SqlParameter("@supplyRank",SqlDbType.VarChar){Value = supplyRank}
                    };

                    getHcTb = _dbHelper.ArmyWebExecuteQuery(getHcSql, getHcParameter);

                    if(getHcTb == null || getHcTb.Rows.Count == 0)
                    {
                        return res;
                    }
                }

                DataRow row = getHcTb.Rows[0];
                int Old = int.Parse(row["old_supply_rank"].ToString());
                int New = int.Parse(row["new_supply_rank"].ToString());
                string oldChNum = chineseNumber(Old);
                string newChNum = chineseNumber(New);
                
                res = new HierarchicalRes
                {
                    OldRankTitle = row["old_rank_title"].ToString(),
                    OldSupplyRank = oldChNum,
                    OldSupplyPoint = row["old_supply_point"].ToString(),
                    NewRankTitle = row["new_rank_title"].ToString(),
                    NewSupplyRank = newChNum,
                    NewSupplyPoint = row["new_supply_point"].ToString(),
                };

                return res;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("getNewHierarchical Error. {0}", ex.ToString()));
            }
            
        }

        public string chineseNumber(int number)
        {
            string chineseNumber = number.ToWords(new System.Globalization.CultureInfo("zh-CN"));
            return chineseNumber;
        }
    }
}