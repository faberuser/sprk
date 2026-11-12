using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;
using NShared.NJit.NCommon;

namespace NShared.NJit;

public class JitStringDeserializer : IMessagePackFormatter<string>, IMessagePackFormatter, IMessagePackFormatter<string[]>
{
	private int _valueCount;

	private ByteSpan _offsetSpan;

	private ByteSpan _valueSpan;

	private Dictionary<int, string> _cacheDic;

	public void Reset()
	{
		_valueCount = 0;
		_offsetSpan = default(ByteSpan);
		_valueSpan = default(ByteSpan);
	}

	public void Init(ByteSpan span)
	{
		_valueCount = span.ReadInt(0);
		_offsetSpan = span.Slice(4);
		_valueSpan = _offsetSpan.Slice(4 * _valueCount);
		if (_valueCount > 0)
		{
			_cacheDic = new Dictionary<int, string>();
		}
	}

	public string Evaluate(ByteSpan span, out ByteSpan nextSpan)
	{
		int index = span.ReadByMsgPack_Int32(0, out nextSpan);
		return EvaluateByIndex(index);
	}

	public static void SkipEvaluate(ByteSpan span, out ByteSpan nextSpan)
	{
		span.ReadByMsgPack_Int32(0, out nextSpan);
	}

	void IMessagePackFormatter<string>.Serialize(ref MessagePackWriter writer, string value, MessagePackSerializerOptions options)
	{
		JitOption.JitLogError("Invalid Call!!!!");
		writer.Write(value);
	}

	string IMessagePackFormatter<string>.Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		int num = reader.ReadInt32();
		if (num >= _valueCount)
		{
			return string.Empty;
		}
		return EvaluateByIndex(num);
	}

	void IMessagePackFormatter<string[]>.Serialize(ref MessagePackWriter writer, string[] value, MessagePackSerializerOptions options)
	{
		JitOption.JitLogError("Invalid Call!!!!");
		writer.WriteNil();
	}

	string[] IMessagePackFormatter<string[]>.Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.IsNil)
		{
			reader.ReadNil();
			return null;
		}
		string[] array = new string[reader.ReadArrayHeader()];
		for (int i = 0; i < array.Length; i++)
		{
			int index = reader.ReadInt32();
			array[i] = EvaluateByIndex(index);
		}
		return array;
	}

	private string EvaluateByIndex(int index)
	{
		if (index >= _valueCount)
		{
			return string.Empty;
		}
		lock (_cacheDic)
		{
			if (_cacheDic.TryGetValue(index, out var value))
			{
				return value;
			}
			int index2 = _offsetSpan.ReadInt(4 * index);
			value = _valueSpan.Slice(index2).ReadByMsgPack_String(0, out var _);
			_cacheDic.Add(index, value);
			return value;
		}
	}
}
