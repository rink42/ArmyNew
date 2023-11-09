using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ArmyAPI.Commons
{
	public class Zip
	{
		public static string Compress(string input)
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(input);

			StringBuilder sb = new StringBuilder();
			using (MemoryStream ms = new MemoryStream())
			{
				using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress))
				{
					zip.Write(byteArray, 0, byteArray.Length);
				}

				byte[] gzipped = ms.ToArray();

				foreach (byte b in gzipped)
				{
					sb.Append(b.ToString("X2"));
				}
			}

			return sb.ToString();
		}

		public static string UnCompress(string hexString)
		{
			int length = hexString.Length / 2;

			// 創建新的 byte array
			byte[] bytes = new byte[length];

			// 轉換每兩個字符為一個byte
			for (int i = 0; i < length; i++)
			{
				bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
			}
			string decompressed = "";
			// 解壓縮
			using (MemoryStream ms = new MemoryStream(bytes))
			{
				using (GZipStream unzip = new GZipStream(ms, CompressionMode.Decompress))
				{
					using (StreamReader sr = new StreamReader(unzip))
					{
						decompressed = sr.ReadToEnd();
					}
				}
			}

			return decompressed;
		}
	}
}