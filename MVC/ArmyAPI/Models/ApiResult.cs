using System;
using System.Collections.Generic;

namespace ArmyAPI.Models
{
	/// <summary>
	/// API呼叫時，傳回的統一物件
	/// </summary>
	public class ApiResult
	{
		/// <summary>
		/// 執行成功與否
		/// </summary>
		public bool Succ { get; set; }
		/// <summary>
		/// 結果代碼(0000=成功，其餘為錯誤代號)
		/// </summary>
		public string Code { get; set; }
		/// <summary>
		/// 錯誤訊息
		/// </summary>
		public string Message { get; set; }
		/// <summary>
		/// 資料時間
		/// </summary>
		public DateTime DataTime { get; set; }
		/// <summary>
		/// 資料本體
		/// </summary>
		public object Data { get; set; }

		public ApiResult()
		{
		}

		/// <summary>
		/// 建立成功結果
		/// </summary>
		/// <param name="data"></param>
		public ApiResult(object data)
		{
			Code = "0000";
			Succ = true;
			DataTime = DateTime.Now;
			Data = data;
		}

		/// <summary>
		/// 建立失敗結果
		/// </summary>
		/// <param name="code"></param>
		/// <param name="message"></param>
		public ApiResult(string code, string message)
		{
			Code = code;
			Succ = false;
			this.DataTime = DateTime.Now;
			Data = null;
			Message = message;
		}

		/// <summary>
		/// 建立失敗結果
		/// </summary>
		/// <param name="code"></param>
		public ApiResult(string code, object o = null)
		{
			Code = code;
			Succ = false;
			this.DataTime = DateTime.Now;
			Data = null;

			var messages = new Dictionary<string, string>
			{
				{ "001", "基本查詢失敗" },
				{ "002", "退伍人員查詢失敗" },
				{ "003", "第 {0} 筆 類別錯誤" },
				{ "004", "第 {0} 筆 新增/ 更新 失敗" },
				{ "005", "刪除失敗" },
				{ "006", "刪除 Index 錯誤" },
				{ "007", "新增 / 更新 失敗" },
				{ "008", "非管理者" },
				{ "009", "檔案大小為0，請確保檔案不為空" },
				{ "010", "請選擇檔案" },
				{ "011", "欄位數不對" },
				{ "012", "讀取列表失敗" },
				{ "013", "匯入失敗，請檢查內容是否有誤" }

			};

			if (messages.TryGetValue(code, out var message))
			{
				// Code exists in the dictionary, set the message
				Message = message;
			}
		}
	}
}