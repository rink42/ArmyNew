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

                string getHcSql = "SELECT * FROM hierarchical WHERE old_rank_code = @rankCode AND old_supply_rank = @supplyRank";
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
                }

                DataRow row = getHcTb.Rows[0];
                int Old = int.Parse(row["old_supply_rank"].ToString());
                int New = int.Parse(row["new_supply_rank"].ToString());
                string oldChNum = chineseNumber(Old);
                string newChNum = chineseNumber(New);

                string rankTitleSql = "SELECT rank_code, rank_title FROM rank WHERE rank_code in (@oldRankCode, @newRankCode) order by rank_code";
                SqlParameter[] rankTitleParameter = {
                    new SqlParameter("@oldRankCode",SqlDbType.VarChar){Value = row["old_rank_code"].ToString()},
                    new SqlParameter("@newRankCode",SqlDbType.VarChar){Value = row["new_rank_code"].ToString()},
                };
                DataTable rankTitleTb = _dbHelper.ArmyExecuteQuery(rankTitleSql, rankTitleParameter);
                string oldRankTitle = string.Empty;
                string newRankTitle = string.Empty;
                if (rankTitleTb != null && rankTitleTb.Rows.Count == 2)
                {
                    newRankTitle = rankTitleTb.Rows[0]["rank_title"].ToString();
                    oldRankTitle = rankTitleTb.Rows[1]["rank_title"].ToString();
                }
                
                HierarchicalRes newHierarchical = new HierarchicalRes
                {
                    OldRankTitle = oldRankTitle,
                    OldSupplyRank = oldChNum,
                    OldSupplyPoint = row["old_supply_point"].ToString(),
                    NewRankTitle = newRankTitle,
                    NewSupplyRank = newChNum,
                    NewSupplyPoint = row["new_supply_point"].ToString(),
                };

                return newHierarchical;
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