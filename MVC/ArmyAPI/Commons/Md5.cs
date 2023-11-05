using System.Text;

namespace ArmyAPI.Commons
{
	public class Md5
	{
		public static string Encode(string input)
		{
			return Encode(Encoding.Default.GetBytes(input));
		}

		public static string Encode(byte[] input)
		{
			// Create a new instance of the MD5CryptoServiceProvider object.
			System.Security.Cryptography.MD5 s1 = System.Security.Cryptography.MD5.Create();

			// Create a new Stringbuilder to collect the bytes
			// and create a string.
			StringBuilder sBuilder = new StringBuilder();

			// Convert the input string to a byte array and compute the hash.
			byte[] data = s1.ComputeHash(input);

			// Loop through each byte of the hashed data 
			// and format each one as a hexadecimal string.
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("X2"));
			}

			// Return the hexadecimal string.
			return sBuilder.ToString();
		}
	}
}