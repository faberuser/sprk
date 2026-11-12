using System.Collections.Generic;
using System.Text;

namespace NVespa;

internal class LdicReader
{
	private static readonly Encoding UTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

	public const uint LDIC_FOURCC = 1128875084u;

	private byte[] bytes;

	private int offset;

	public LdicReader(byte[] bytes)
		: this(bytes, 0)
	{
	}

	public LdicReader(byte[] bytes, int offset)
	{
		this.bytes = bytes;
		this.offset = offset;
	}

	public static uint ReadUInt32(ref byte[] bytes, int offset)
	{
		return (uint)(bytes[offset] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24));
	}

	public static int ReadInt32(ref byte[] bytes, int offset)
	{
		return bytes[offset] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24);
	}

	public static string ReadString(ref byte[] bytes, int offset, int count)
	{
		return UTF8.GetString(bytes, offset, count);
	}

	private static bool EnsureBufferSize(ref byte[] bytes, int offset, int size)
	{
		return bytes.Length >= offset + size;
	}

	public bool ReadAll(ref Dictionary<uint, string> dict)
	{
		int num = 0;
		int num2 = 0;
		if (bytes.Length <= num + 16)
		{
			return false;
		}
		uint num3 = ReadUInt32(ref bytes, num);
		num += 4;
		if (num3 != 1128875084)
		{
			return false;
		}
		uint num4 = ReadUInt32(ref bytes, num);
		num += 4;
		if (num4 < 3)
		{
			return false;
		}
		num += 4;
		int num5 = ReadInt32(ref bytes, num);
		num += 4;
		num += 4 * num5;
		num += 4;
		int size = ReadInt32(ref bytes, num);
		num += 4;
		if (!EnsureBufferSize(ref bytes, num - 4, size))
		{
			return false;
		}
		ReadInt32(ref bytes, num);
		num += 4;
		num += 4 * num5;
		for (int i = 0; i < num5; i++)
		{
			num += 8;
			uint key = ReadUInt32(ref bytes, num);
			num += 4;
			num2 = ReadInt32(ref bytes, num);
			num += 4;
			dict[key] = ReadString(ref bytes, num, num2);
			num += num2;
		}
		return true;
	}
}
