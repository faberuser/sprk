using System.IO;
using System.Text;
using NVespa;

namespace Vespa_System_MemoryStream_ExtensionMethod;

public static class MemoryStream_ExtensionMethod
{
	public static void WRITEBOOL(this MemoryStream This, bool value, BitConverter2 converter)
	{
		if (This != null)
		{
			converter.SetBool(value);
			if (converter.BytesLength > 0)
			{
				This.Write(converter.Bytes, 0, converter.BytesLength);
			}
		}
	}

	public static void WRITESHORT(this MemoryStream This, short value, BitConverter2 converter)
	{
		if (This != null)
		{
			converter.SetShort(value);
			if (converter.BytesLength > 0)
			{
				This.Write(converter.Bytes, 0, converter.BytesLength);
			}
		}
	}

	public static void WRITEINT(this MemoryStream This, int value, BitConverter2 converter)
	{
		if (This != null)
		{
			converter.SetInt(value);
			if (converter.BytesLength > 0)
			{
				This.Write(converter.Bytes, 0, converter.BytesLength);
			}
		}
	}

	public static void WRITELONG(this MemoryStream This, long value, BitConverter2 converter)
	{
		if (This != null)
		{
			converter.SetLong(value);
			if (converter.BytesLength > 0)
			{
				This.Write(converter.Bytes, 0, converter.BytesLength);
			}
		}
	}

	public static void WRITEDOUBLE(this MemoryStream This, double value, BitConverter2 converter)
	{
		if (This != null)
		{
			converter.SetDouble(value);
			if (converter.BytesLength > 0)
			{
				This.Write(converter.Bytes, 0, converter.BytesLength);
			}
		}
	}

	public static void WRITESTRING(this MemoryStream This, string value, BitConverter2 converter)
	{
		if (This == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(value))
		{
			This.WRITEINT(0, converter);
			return;
		}
		byte[] bytes = Encoding.Unicode.GetBytes(value);
		int num = ((bytes != null) ? bytes.Length : 0);
		This.WRITEINT(num, converter);
		if (num > 0)
		{
			This.Write(bytes, 0, num);
		}
	}
}
