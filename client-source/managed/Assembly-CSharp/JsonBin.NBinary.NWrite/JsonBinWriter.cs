using System;
using System.Collections.Generic;
using JsonBin.NBinary.NRead;

namespace JsonBin.NBinary.NWrite;

public class JsonBinWriter
{
	private SimpleBufferWriter writer;

	public JsonBinWriter(SimpleBufferWriter writer)
	{
		this.writer = writer;
	}

	public void Write_Native(int value)
	{
		JsonBinSpan span = writer.GetSpan(4);
		Write_Native_Static(value, span);
		writer.Advance(4);
	}

	public void WritePacked_Nil()
	{
		JsonBinSpan span = writer.GetSpan(1);
		span[0] = 192;
		writer.Advance(1);
	}

	public void WriteRaw_Nil()
	{
		WritePacked_Nil();
	}

	public void WritePacked(byte value)
	{
		if (value <= 127)
		{
			JsonBinSpan span = writer.GetSpan(1);
			span[0] = value;
			writer.Advance(1);
		}
		else
		{
			WriteRaw_UInt8(value);
		}
	}

	public void WriteRaw_UInt8(byte value)
	{
		JsonBinSpan span = writer.GetSpan(2);
		span[0] = 201;
		span[1] = value;
		writer.Advance(2);
	}

	public void WritePacked(sbyte value)
	{
		if (value < -32)
		{
			WriteRaw_Int8(value);
			return;
		}
		JsonBinSpan span = writer.GetSpan(1);
		span[0] = (byte)value;
		writer.Advance(1);
	}

	public void WriteRaw_Int8(sbyte value)
	{
		JsonBinSpan span = writer.GetSpan(2);
		span[0] = 208;
		span[1] = (byte)value;
		writer.Advance(2);
	}

	public void WritePacked(ushort value)
	{
		if (value <= 127)
		{
			JsonBinSpan span = writer.GetSpan(1);
			span[0] = (byte)value;
			writer.Advance(1);
		}
		else if (value <= 255)
		{
			JsonBinSpan span2 = writer.GetSpan(2);
			span2[0] = 201;
			span2[1] = (byte)value;
			writer.Advance(2);
		}
		else
		{
			WriteRaw_UInt16(value);
		}
	}

	public void WriteRaw_UInt16(ushort value)
	{
		JsonBinSpan span = writer.GetSpan(3);
		span[0] = 202;
		Write_Native_Static(value, span.Slice(1));
		writer.Advance(3);
	}

	public void WritePacked(short value)
	{
		if (value >= 0)
		{
			WritePacked((ushort)value);
		}
		else if (value >= -32)
		{
			JsonBinSpan span = writer.GetSpan(1);
			span[0] = (byte)value;
			writer.Advance(1);
		}
		else if (value >= -128)
		{
			JsonBinSpan span2 = writer.GetSpan(2);
			span2[0] = 208;
			span2[1] = (byte)value;
			writer.Advance(2);
		}
		else
		{
			WriteRaw_Int16(value);
		}
	}

	public void WriteRaw_Int16(short value)
	{
		JsonBinSpan span = writer.GetSpan(3);
		span[0] = 209;
		Write_Native_Static(value, span.Slice(1));
		writer.Advance(3);
	}

	public void WritePacked(uint value)
	{
		if (value <= 127)
		{
			JsonBinSpan span = writer.GetSpan(1);
			span[0] = (byte)value;
			writer.Advance(1);
		}
		else if (value <= 255)
		{
			JsonBinSpan span2 = writer.GetSpan(2);
			span2[0] = 201;
			span2[1] = (byte)value;
			writer.Advance(2);
		}
		else if (value <= 65535)
		{
			JsonBinSpan span3 = writer.GetSpan(3);
			span3[0] = 202;
			Write_Native_Static((ushort)value, span3.Slice(1));
			writer.Advance(3);
		}
		else
		{
			WriteRaw_UInt32(value);
		}
	}

	public void WriteRaw_UInt32(uint value)
	{
		JsonBinSpan span = writer.GetSpan(5);
		span[0] = 203;
		Write_Native_Static(value, span.Slice(1));
		writer.Advance(5);
	}

	public void WritePacked(int value)
	{
		if (value >= 0)
		{
			WritePacked((uint)value);
		}
		else if (value >= -32)
		{
			JsonBinSpan span = writer.GetSpan(1);
			span[0] = (byte)value;
			writer.Advance(1);
		}
		else if (value >= -128)
		{
			JsonBinSpan span2 = writer.GetSpan(2);
			span2[0] = 208;
			span2[1] = (byte)value;
			writer.Advance(2);
		}
		else if (value >= -32768)
		{
			JsonBinSpan span3 = writer.GetSpan(3);
			span3[0] = 209;
			Write_Native_Static((short)value, span3.Slice(1));
			writer.Advance(3);
		}
		else
		{
			WriteRaw_Int32(value);
		}
	}

	public void WriteRaw_Int32(int value)
	{
		JsonBinSpan span = writer.GetSpan(5);
		span[0] = 210;
		Write_Native_Static(value, span.Slice(1));
		writer.Advance(5);
	}

	public void WritePacked(ulong value)
	{
		if (value <= 127)
		{
			JsonBinSpan span = writer.GetSpan(1);
			span[0] = (byte)value;
			writer.Advance(1);
		}
		else if (value <= 255)
		{
			JsonBinSpan span2 = writer.GetSpan(2);
			span2[0] = 201;
			span2[1] = (byte)value;
			writer.Advance(2);
		}
		else if (value <= 65535)
		{
			JsonBinSpan span3 = writer.GetSpan(3);
			span3[0] = 202;
			Write_Native_Static((ushort)value, span3.Slice(1));
			writer.Advance(3);
		}
		else if (value <= uint.MaxValue)
		{
			JsonBinSpan span4 = writer.GetSpan(5);
			span4[0] = 203;
			Write_Native_Static((uint)value, span4.Slice(1));
			writer.Advance(5);
		}
		else
		{
			WriteRaw_UInt64(value);
		}
	}

	public void WriteRaw_UInt64(ulong value)
	{
		JsonBinSpan span = writer.GetSpan(9);
		span[0] = 204;
		Write_Native_Static(value, span.Slice(1));
		writer.Advance(9);
	}

	public void WritePacked(long value)
	{
		if (value >= 0)
		{
			WritePacked((ulong)value);
		}
		else if (value >= -32)
		{
			JsonBinSpan span = writer.GetSpan(1);
			span[0] = (byte)value;
			writer.Advance(1);
		}
		else if (value >= -128)
		{
			JsonBinSpan span2 = writer.GetSpan(2);
			span2[0] = 208;
			span2[1] = (byte)value;
			writer.Advance(2);
		}
		else if (value >= -32768)
		{
			JsonBinSpan span3 = writer.GetSpan(3);
			span3[0] = 209;
			Write_Native_Static((short)value, span3.Slice(1));
			writer.Advance(3);
		}
		else if (value >= int.MinValue)
		{
			JsonBinSpan span4 = writer.GetSpan(5);
			span4[0] = 210;
			Write_Native_Static((int)value, span4.Slice(1));
			writer.Advance(5);
		}
		else
		{
			WriteRaw_Int64(value);
		}
	}

	public void WriteRaw_Int64(long value)
	{
		JsonBinSpan span = writer.GetSpan(9);
		span[0] = 211;
		Write_Native_Static(value, span.Slice(1));
		writer.Advance(9);
	}

	public void WritePacked(bool value)
	{
		JsonBinSpan span = writer.GetSpan(1);
		span[0] = (byte)(value ? 195 : 194);
		writer.Advance(1);
	}

	public void WritePacked(char value)
	{
		WritePacked((ushort)value);
	}

	public void WritePacked(float value)
	{
		if (value == 0f)
		{
			WritePacked(0);
			return;
		}
		JsonBinSpan span = writer.GetSpan(5);
		span[0] = 199;
		Write_Native_Static(value, span.Slice(1));
		writer.Advance(5);
	}

	public void WritePacked(double value)
	{
		if (value == 0.0)
		{
			WritePacked(0);
			return;
		}
		JsonBinSpan span = writer.GetSpan(9);
		span[0] = 200;
		Write_Native_Static(value, span.Slice(1));
		writer.Advance(9);
	}

	public void WritePacked_BinHeader(int length)
	{
		if (length <= 255)
		{
			int sizeHint = length + 2;
			JsonBinSpan span = writer.GetSpan(sizeHint);
			span[0] = 196;
			span[1] = (byte)length;
			writer.Advance(2);
		}
		else if (length <= 65535)
		{
			int sizeHint2 = length + 3;
			JsonBinSpan span2 = writer.GetSpan(sizeHint2);
			span2[0] = 197;
			Write_Native_Static((ushort)length, span2.Slice(1));
			writer.Advance(3);
		}
		else
		{
			int sizeHint3 = length + 5;
			JsonBinSpan span3 = writer.GetSpan(sizeHint3);
			span3[0] = 198;
			Write_Native_Static(length, span3.Slice(1));
			writer.Advance(5);
		}
	}

	public void WritePacked_ArrayHeader(List<int> offsetList, bool isMap = false)
	{
		int num = __Extension_Method_ICollection__.COUNT(offsetList);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(offsetList); i++)
		{
			num = Math.Max(offsetList[i], num);
		}
		if (num <= 255)
		{
			int num2 = 2 + __Extension_Method_ICollection__.COUNT(offsetList);
			JsonBinSpan span = writer.GetSpan(num2);
			span[0] = (byte)(isMap ? 218 : 215);
			span[1] = (byte)__Extension_Method_ICollection__.COUNT(offsetList);
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(offsetList); j++)
			{
				span[2 + j] = (byte)offsetList[j];
			}
			writer.Advance(num2);
		}
		else if (num <= 65535)
		{
			int num3 = 1 + 2 * (1 + __Extension_Method_ICollection__.COUNT(offsetList));
			JsonBinSpan span2 = writer.GetSpan(num3);
			span2[0] = (byte)(isMap ? 219 : 216);
			Write_Native_Static((ushort)__Extension_Method_ICollection__.COUNT(offsetList), span2.Slice(1));
			for (int k = 0; k < __Extension_Method_ICollection__.COUNT(offsetList); k++)
			{
				int num4 = offsetList[k];
				JsonBinSpan span3 = span2.Slice(1 + 2 * (1 + k));
				Write_Native_Static((ushort)num4, span3);
			}
			writer.Advance(num3);
		}
		else
		{
			int num5 = 1 + 4 * (1 + __Extension_Method_ICollection__.COUNT(offsetList));
			JsonBinSpan span4 = writer.GetSpan(num5);
			span4[0] = (byte)(isMap ? 220 : 217);
			Write_Native_Static(__Extension_Method_ICollection__.COUNT(offsetList), span4.Slice(1));
			for (int l = 0; l < __Extension_Method_ICollection__.COUNT(offsetList); l++)
			{
				int value = offsetList[l];
				JsonBinSpan span5 = span4.Slice(1 + 4 * (1 + l));
				Write_Native_Static(value, span5);
			}
			writer.Advance(num5);
		}
	}

	public void WritePacked_IndexedString(int stringIndex)
	{
		JsonBinSpan span = writer.GetSpan(5);
		span[0] = 221;
		Write_Native_Static(stringIndex, span.Slice(1));
		writer.Advance(5);
	}

	public void WritePacked(string value)
	{
		if (value == null)
		{
			WritePacked_Nil();
			return;
		}
		byte[] bytes = JsonBinCode.String_Encoding.GetBytes(value);
		WritePacked_String(bytes);
	}

	public void WritePacked_String(byte[] utf8stringBytes)
	{
		int num = utf8stringBytes.Length;
		WritePacked_StringHeader(num);
		writer.GetSpan(num).CopyFrom(utf8stringBytes);
		writer.Advance(num);
	}

	public void WritePacked_StringHeader(int byteCount)
	{
		if (byteCount <= 255)
		{
			JsonBinSpan span = writer.GetSpan(byteCount + 2);
			span[0] = 212;
			span[1] = (byte)byteCount;
			writer.Advance(2);
		}
		else if (byteCount <= 65535)
		{
			JsonBinSpan span2 = writer.GetSpan(byteCount + 3);
			span2[0] = 213;
			Write_Native_Static((ushort)byteCount, span2.Slice(1));
			writer.Advance(3);
		}
		else
		{
			JsonBinSpan span3 = writer.GetSpan(byteCount + 5);
			span3[0] = 214;
			Write_Native_Static(byteCount, span3.Slice(1));
			writer.Advance(5);
		}
	}

	public JsonBinSpan GetSpan(int length)
	{
		return writer.GetSpan(length);
	}

	public void Advance(int length)
	{
		writer.Advance(length);
	}

	public void CopyDirect(byte[] bytes)
	{
		writer.GetSpan(bytes.Length).CopyFrom(bytes);
		writer.Advance(bytes.Length);
	}

	public void CopyDirect(ReadonlyBuffer buffer)
	{
		writer.GetSpan(buffer.Length).CopyFrom(buffer.GetRawBytes(), buffer.Length);
		writer.Advance(buffer.Length);
	}

	private static void Write_Native_Static(short value, JsonBinSpan span)
	{
		Write_Native_Static((ushort)value, span);
	}

	private static void Write_Native_Static(int value, JsonBinSpan span)
	{
		Write_Native_Static((uint)value, span);
	}

	private static void Write_Native_Static(long value, JsonBinSpan span)
	{
		Write_Native_Static((ulong)value, span);
	}

	private static void Write_Native_Static(ushort value, JsonBinSpan span)
	{
		span[0] = (byte)value;
		span[1] = (byte)(value >> 8);
	}

	private static void Write_Native_Static(uint value, JsonBinSpan span)
	{
		span[0] = (byte)value;
		span[1] = (byte)(value >> 8);
		span[2] = (byte)(value >> 16);
		span[3] = (byte)(value >> 24);
	}

	private static void Write_Native_Static(ulong value, JsonBinSpan span)
	{
		span[0] = (byte)value;
		span[1] = (byte)(value >> 8);
		span[2] = (byte)(value >> 16);
		span[3] = (byte)(value >> 24);
		span[4] = (byte)(value >> 32);
		span[5] = (byte)(value >> 40);
		span[6] = (byte)(value >> 48);
		span[7] = (byte)(value >> 56);
	}

	private unsafe static void Write_Native_Static(float value, JsonBinSpan span)
	{
		Write_Native_Static(*(int*)(&value), span);
	}

	private unsafe static void Write_Native_Static(double value, JsonBinSpan span)
	{
		Write_Native_Static(*(long*)(&value), span);
	}
}
