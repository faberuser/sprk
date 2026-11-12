using JsonBin.NBinary.NRead;
using JsonBin.NBinary.NWrite;

namespace JsonBin.NBinary;

public sealed class PackedDic_IntToString : PackedDictionary<int, string>
{
	public PackedDic_IntToString()
		: base(string.Empty)
	{
	}

	protected override int KeyToHash(int key)
	{
		return key;
	}

	protected override int ReadKeyFromSpan(JsonBinSpan span, out JsonBinSpan nextSpan)
	{
		if (!span.TryReadPacked_Int(out var value, out nextSpan))
		{
			return 0;
		}
		return value;
	}

	protected override string ReadValueFromSpan(JsonBinSpan span, out JsonBinSpan nextSpan)
	{
		if (!span.TryReadPacked_String(out var value, out nextSpan))
		{
			return string.Empty;
		}
		return value;
	}

	protected override void WriteKey(JsonBinWriter writer, int key)
	{
		writer.WritePacked(key);
	}

	protected override void WriteValue(JsonBinWriter writer, string value)
	{
		writer.WritePacked(value);
	}
}
