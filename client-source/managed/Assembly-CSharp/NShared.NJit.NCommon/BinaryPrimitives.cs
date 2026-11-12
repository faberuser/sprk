namespace NShared.NJit.NCommon;

public static class BinaryPrimitives
{
	public static sbyte ReverseEndianness(sbyte value)
	{
		return value;
	}

	public static short ReverseEndianness(short value)
	{
		return (short)ReverseEndianness((ushort)value);
	}

	public static int ReverseEndianness(int value)
	{
		return (int)ReverseEndianness((uint)value);
	}

	public static long ReverseEndianness(long value)
	{
		return (long)ReverseEndianness((ulong)value);
	}

	public static byte ReverseEndianness(byte value)
	{
		return value;
	}

	public static ushort ReverseEndianness(ushort value)
	{
		return (ushort)((value >> 8) + (value << 8));
	}

	public static uint ReverseEndianness(uint value)
	{
		return RotateRight(value & 0xFF00FF, 8) + RotateLeft(value & 0xFF00FF00u, 8);
	}

	public static ulong ReverseEndianness(ulong value)
	{
		return ((ulong)ReverseEndianness((uint)value) << 32) + ReverseEndianness((uint)(value >> 32));
	}

	public static uint RotateRight(uint value, int offset)
	{
		return (value >> offset) | (value << 32 - offset);
	}

	public static uint RotateLeft(uint value, int offset)
	{
		return (value << offset) | (value >> 32 - offset);
	}
}
