using System;

namespace MiniJSON;

public class BitConverter2
{
	public int BytesLength { get; private set; }

	public byte[] Bytes { get; private set; }

	public BitConverter2()
	{
		Bytes = new byte[8];
		BytesLength = 0;
	}

	public void SetBool(bool value)
	{
		BoolToBytes(value, Bytes, 0);
		BytesLength = 1;
	}

	public void SetShort(short value)
	{
		ShortToBytes(value, Bytes, 0);
		BytesLength = 2;
	}

	public void SetInt(int value)
	{
		IntToBytes(value, Bytes, 0);
		BytesLength = 4;
	}

	public void SetLong(long value)
	{
		LongToBytes(value, Bytes, 0);
		BytesLength = 8;
	}

	public void SetDouble(double value)
	{
		DoubleToBytes(value, Bytes, 0);
		BytesLength = 8;
	}

	public static byte[] GetBytes(bool value)
	{
		byte[] array = new byte[1];
		BoolToBytes(value, array, 0);
		return array;
	}

	public static byte[] GetBytes(short value)
	{
		byte[] array = new byte[2];
		ShortToBytes(value, array, 0);
		return array;
	}

	public static byte[] GetBytes(int value)
	{
		byte[] array = new byte[4];
		IntToBytes(value, array, 0);
		return array;
	}

	public static byte[] GetBytes(long value)
	{
		byte[] array = new byte[8];
		LongToBytes(value, array, 0);
		return array;
	}

	public static byte[] GetBytes(double value)
	{
		byte[] array = new byte[8];
		DoubleToBytes(value, array, 0);
		return array;
	}

	public static void BoolToBytes(bool value, byte[] dest1, int offset)
	{
		if (value)
		{
			dest1[offset] = 1;
		}
		else
		{
			dest1[offset] = 0;
		}
	}

	public static void ShortToBytes(short value, byte[] dest2, int offset)
	{
		if (BitConverter.IsLittleEndian)
		{
			dest2[offset] = (byte)(value & 0xFF);
			dest2[offset + 1] = (byte)((value >> 8) & 0xFF);
		}
		else
		{
			dest2[offset + 1] = (byte)(value & 0xFF);
			dest2[offset] = (byte)((value >> 8) & 0xFF);
		}
	}

	public static void IntToBytes(int value, byte[] dest4, int offset)
	{
		if (BitConverter.IsLittleEndian)
		{
			dest4[offset] = (byte)(value & 0xFF);
			dest4[offset + 1] = (byte)((value >> 8) & 0xFF);
			dest4[offset + 2] = (byte)((value >> 16) & 0xFF);
			dest4[offset + 3] = (byte)(value >> 24);
		}
		else
		{
			dest4[offset + 3] = (byte)(value & 0xFF);
			dest4[offset + 2] = (byte)((value >> 8) & 0xFF);
			dest4[offset + 1] = (byte)((value >> 16) & 0xFF);
			dest4[offset] = (byte)(value >> 24);
		}
	}

	public static void LongToBytes(long value, byte[] dest8, int offset)
	{
		if (BitConverter.IsLittleEndian)
		{
			dest8[offset] = (byte)(value & 0xFF);
			dest8[offset + 1] = (byte)((value >> 8) & 0xFF);
			dest8[offset + 2] = (byte)((value >> 16) & 0xFF);
			dest8[offset + 3] = (byte)((value >> 24) & 0xFF);
			dest8[offset + 4] = (byte)((value >> 32) & 0xFF);
			dest8[offset + 5] = (byte)((value >> 40) & 0xFF);
			dest8[offset + 6] = (byte)((value >> 48) & 0xFF);
			dest8[offset + 7] = (byte)((value >> 56) & 0xFF);
		}
		else
		{
			dest8[offset + 7] = (byte)(value & 0xFF);
			dest8[offset + 6] = (byte)((value >> 8) & 0xFF);
			dest8[offset + 5] = (byte)((value >> 16) & 0xFF);
			dest8[offset + 4] = (byte)((value >> 24) & 0xFF);
			dest8[offset + 3] = (byte)((value >> 32) & 0xFF);
			dest8[offset + 2] = (byte)((value >> 40) & 0xFF);
			dest8[offset + 1] = (byte)((value >> 48) & 0xFF);
			dest8[offset] = (byte)((value >> 56) & 0xFF);
		}
	}

	public static void DoubleToBytes(double value, byte[] dest8, int offset)
	{
		LongToBytes(BitConverter.DoubleToInt64Bits(value), dest8, offset);
	}
}
