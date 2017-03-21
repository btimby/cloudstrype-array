using System;

namespace CloudstrypeArray.Lib
{
	public static class Util
	{
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
	}
}
	