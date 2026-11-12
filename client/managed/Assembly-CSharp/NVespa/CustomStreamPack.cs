using System;
using System.Text;
using UnityEngine;

namespace NVespa;

public class CustomStreamPack : CustomStreamBase
{
	public class Header
	{
		public string EntryName;

		public int DataLength;

		public Header(string entryName, int dataLength)
		{
			EntryName = entryName;
			DataLength = dataLength;
		}
	}

	public CustomStreamPack(long length)
		: base(length)
	{
	}

	public Header GetHeader()
	{
		string text = ReadUtf8String();
		int num = ReadInt();
		if (string.IsNullOrEmpty(text) || num <= 0)
		{
			return null;
		}
		return new Header(text, num);
	}

	private bool CanReadByte(int needByteCount)
	{
		if (_currentLength == 0L)
		{
			Debug.LogError("CanRead failed: Bytes is null");
			return false;
		}
		if (base.ReadableLength < needByteCount)
		{
			return false;
		}
		return true;
	}

	private int ReadInt()
	{
		int num = 4;
		if (!CanReadByte(num))
		{
			return 0;
		}
		byte[] array = new byte[num];
		if (Read(array, 0, array.Length) != array.Length)
		{
			VespaDebug.LogError("Buffer length not equal");
			return 0;
		}
		return BitConverter.ToInt32(array, 0);
	}

	private string ReadUtf8String()
	{
		int num = ReadInt();
		if (num == 0)
		{
			return string.Empty;
		}
		if (!CanReadByte(num))
		{
			return string.Empty;
		}
		byte[] array = new byte[num];
		if (Read(array, 0, array.Length) != array.Length)
		{
			VespaDebug.LogError("Buffer length not equal");
			return string.Empty;
		}
		return Encoding.UTF8.GetString(array, 0, array.Length);
	}
}
