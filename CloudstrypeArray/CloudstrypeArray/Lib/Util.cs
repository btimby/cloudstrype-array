using System;
using System.Text.RegularExpressions;

namespace CloudstrypeArray.Lib
{
	public static class Util
	{
		private static string[] SUFFIXES = new string[] {"BYTES", "KB", "MB", "GB", "TB"};

		public static void SwapArray<T>(ref T[] data, int left, int right)
		{
			T t = data[left];
			data[left] = data[right];
			data[right] = t;
		}

		public static void FixGuid(ref byte[] guid)
		{
			SwapArray<byte> (ref guid, 0, 3);
			SwapArray<byte> (ref guid, 1, 2);
			SwapArray<byte> (ref guid, 4, 5);
			SwapArray<byte> (ref guid, 6, 7);
		}

		public static long ParseFileSize(string size)
		{
			// Use regex to split numeric/suffix portions.
			Match m =  Regex.Match(size, @"([\.\d]+)(\w*)");
			string num = m.Groups[1].ToString();
			string suff = m.Groups[2].ToString();
			// Parse double
			double numSize;
			if (!Double.TryParse(num, out numSize))
			{
				return 0;
			}
			// lookup multiplier.
			int pos = Array.IndexOf(SUFFIXES, suff.ToUpper());
			if (pos == -1)
			{
				// Assume bytes
				pos = 0;
			}
			return (long)(numSize * Math.Pow(1024, pos));
		}

		public static string GetHomeDirectory()
		{
			return Environment.GetEnvironmentVariable("HOME");
		}
	}
}
	