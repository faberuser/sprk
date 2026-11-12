using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;
using NShared.NJit.NCommon;

namespace NShared.NJit;

public class JitStringSerializer : IMessagePackFormatter<string>, IMessagePackFormatter, IMessagePackFormatter<string[]>
{
	private List<string> _valueList = new List<string>();

	private Dictionary<string, int> _indexDic = new Dictionary<string, int>();

	public int StringCount => _valueList.Count;

	public int IndexingCount => _indexDic.Count;

	public JitStringSerializer()
	{
		_valueList.Add(string.Empty);
		_indexDic.Add(string.Empty, 0);
	}

	public byte[] DoSerialize()
	{
		List<byte[]> list = new List<byte[]>();
		for (int i = 0; i < _valueList.Count; i++)
		{
			byte[] item = MessagePackSerializer.Serialize(_valueList[i], MessagePackInitializer.Options_with_BuiltinResolver);
			list.Add(item);
		}
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		simpleBufferWriter.WriteInt(list.Count);
		int num = 0;
		for (int j = 0; j < list.Count; j++)
		{
			simpleBufferWriter.WriteInt(num);
			num += list[j].Length;
		}
		for (int k = 0; k < list.Count; k++)
		{
			simpleBufferWriter.WriteBytes(list[k]);
		}
		return simpleBufferWriter.ToArray();
	}

	public byte[] SerializeValue(string value)
	{
		return SimpleBufferWriter.DirectWriteByMsgPack(GetValueIndex(value));
	}

	void IMessagePackFormatter<string>.Serialize(ref MessagePackWriter writer, string value, MessagePackSerializerOptions options)
	{
		int valueIndex = GetValueIndex(value);
		writer.Write(valueIndex);
	}

	string IMessagePackFormatter<string>.Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		int num = reader.ReadInt32();
		if (num >= _valueList.Count)
		{
			return string.Empty;
		}
		return _valueList[num];
	}

	void IMessagePackFormatter<string[]>.Serialize(ref MessagePackWriter writer, string[] value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(value.Length);
		for (int i = 0; i < value.Length; i++)
		{
			int valueIndex = GetValueIndex(value[i]);
			writer.Write(valueIndex);
		}
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
			int num = reader.ReadInt32();
			if (num < _valueList.Count)
			{
				array[i] = _valueList[num];
			}
			else
			{
				array[i] = string.Empty;
			}
		}
		return array;
	}

	private int GetValueIndex(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return 0;
		}
		if (_indexDic.TryGetValue(value, out var value2))
		{
			return value2;
		}
		value2 = _valueList.Count;
		_valueList.Add(value);
		_indexDic.Add(value, value2);
		return value2;
	}
}
