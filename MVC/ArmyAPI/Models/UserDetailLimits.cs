using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Newtonsoft.Json;

namespace ArmyAPI.Models
{
	public class UserDetailLimits
	{
		public string Key { get; set; }

		public List<string> Values { get; set; }
//		[JsonIgnore]
		public List<string> Texts { get; set; }
		public List<string> Wheres { get; set; }

		[JsonIgnore]
		/// <summary>
		/// 依據權限組成 Where 條件
		/// </summary>
		public StringBuilder WhereString = new StringBuilder();

		public UnitTypes HaveLimits { get; set; } = UnitTypes.None;

		public enum UnitTypes : uint
		{
			[Description("")]
			None = 0,
			[Description("其他")]
			官科 = 1,
			[Description("單位")]
			全軍 = 官科 * 2,
			[Description("單位")]
			陸軍 = 全軍 * 2,
			[Description("單位")]
			陸階 = 陸軍 * 2,
			[Description("單位")]
			資電軍 = 陸階 * 2,
			[Description("單位")]
			後備 = 資電軍 * 2,
			[Description("單位")]
			業管 = 後備 * 2,
			[Description("階級")]
			軍官_含將官 = 業管 * 2,
			[Description("階級")]
			軍官_不含將官 = 軍官_含將官 * 2,
			[Description("階級")]
			士官 = 軍官_不含將官 * 2,
			[Description("階級")]
			士兵 = 士官 * 2,
			[Description("階級")]
			聘僱 = 士兵 * 2,
			[Description("網站2")]
			人事查詢_現員 = 聘僱 * 2,
			[Description("網站2")]
			人事查詢_退員 = 人事查詢_現員 * 2,
			[Description("網站2")]
			現員年籍冊 = 人事查詢_退員 * 2,
			[Description("網站2")]
			退員年籍冊 = 現員年籍冊 * 2
		}

		public UserDetailLimits()
		{
		}

		public UserDetailLimits(string key, List<string> values, List<string> texts, List<string> wheres)
		{
			Key = key;
			Values = values;
			Texts = texts;
			Wheres = wheres;

			foreach (var t in texts)
			{
				foreach (UnitTypes ut in Enum.GetValues(typeof(UnitTypes)))
				{
					if (t.Replace("(", "_").Replace(")", "") == ut.ToString())
					{
						HaveLimits |= ut;
						break;
					}
				}
			}
		}

		public bool HasLimit(UnitTypes type)
		{
			bool result = false;

			if ((uint)(HaveLimits & type) > 0)
				result = true;

			return result;
		}
	}
}