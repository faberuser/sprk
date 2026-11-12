using System;
using System.Collections.Generic;
using JsonBin.NBinary.NRead;
using JsonBin.NBinary.NWrite;
using NVespa;

namespace JsonBin.NBinary;

public class KeyContainer
{
	private interface IEnumMapper
	{
	}

	private class EnumMapper<EnumType> : IEnumMapper
	{
		public Dictionary<int, EnumType> Map = new Dictionary<int, EnumType>();
	}

	private PackedDic_IntToString _keyDic = new PackedDic_IntToString();

	private PackedDic_IntToString _valueStringDic = new PackedDic_IntToString();

	private Dictionary<Type, IEnumMapper> _enumMapper = new Dictionary<Type, IEnumMapper>();

	private Dictionary<string, int> _indexMap;

	public int Size { get; private set; }

	public int Count => _keyDic.Count;

	public static List<string> Errors_on_WriteJsonBin { get; private set; }

	public IDictionary<int, string> GetAllKeys()
	{
		return _keyDic.GetAllData();
	}

	public IDictionary<int, string> GetAllValueStrings()
	{
		return _valueStringDic.GetAllData();
	}

	public string GetKey(int hash)
	{
		if (_keyDic.TryGetValue(hash, out var value))
		{
			return value;
		}
		return string.Empty;
	}

	public int GetHash(string key)
	{
		return StringToHashCode(key);
	}

	public string GetValueString(int index)
	{
		if (_valueStringDic.TryGetValue(index, out var value))
		{
			return value;
		}
		return string.Empty;
	}

	public bool TryGetEnumFromIndexedStr<EnumType>(int stringIndex, out EnumType value)
	{
		Type typeFromHandle = typeof(EnumType);
		EnumMapper<EnumType> enumMapper;
		if (!_enumMapper.TryGetValue(typeFromHandle, out var value2))
		{
			enumMapper = new EnumMapper<EnumType>();
			_enumMapper.Add(typeFromHandle, enumMapper);
		}
		else
		{
			enumMapper = value2 as EnumMapper<EnumType>;
			if (enumMapper == null)
			{
				value = default(EnumType);
				return false;
			}
		}
		if (enumMapper.Map.TryGetValue(stringIndex, out value))
		{
			return true;
		}
		if (!Vespa_String_Extension_Method.TryToEnum<EnumType>(GetValueString(stringIndex), out value))
		{
			value = default(EnumType);
		}
		enumMapper.Map.Add(stringIndex, value);
		return true;
	}

	public int AddKey(string value)
	{
		if (value == null)
		{
			value = string.Empty;
		}
		int num = StringToHashCode(value);
		if (_keyDic.Original.TryGetValue(num, out var value2))
		{
			if (value2 == value)
			{
				return num;
			}
			string text = $"Duplicate Hash Value: OldKey={value2}, NewKey={value}";
			VespaDebug.LogError(text);
			if (Errors_on_WriteJsonBin == null)
			{
				Errors_on_WriteJsonBin = new List<string>();
			}
			Errors_on_WriteJsonBin.Add(text);
			return 0;
		}
		_keyDic.Original.Add(num, value);
		return num;
	}

	public int AddValueString(string value)
	{
		if (_indexMap == null)
		{
			_indexMap = new Dictionary<string, int>();
		}
		if (value == null)
		{
			value = string.Empty;
		}
		if (_indexMap.TryGetValue(value, out var value2))
		{
			return value2;
		}
		value2 = _valueStringDic.Original.Count;
		_valueStringDic.Original.Add(value2, value);
		_indexMap.Add(value, value2);
		return value2;
	}

	public byte[] Serialize()
	{
		byte[] array = _keyDic.Serialize();
		byte[] array2 = _valueStringDic.Serialize();
		using SimpleBufferWriter simpleBufferWriter = new SimpleBufferWriter();
		JsonBinWriter jsonBinWriter = new JsonBinWriter(simpleBufferWriter);
		jsonBinWriter.Write_Native(array.Length);
		jsonBinWriter.CopyDirect(array);
		jsonBinWriter.Write_Native(array2.Length);
		jsonBinWriter.CopyDirect(array2);
		return simpleBufferWriter.ToArrayFinal();
	}

	public bool Deserialize(JsonBinReader reader, int keyContainerSize)
	{
		Size = keyContainerSize;
		if (!reader.TryRead_Native(out int value))
		{
			return false;
		}
		JsonBinSpan span = reader.Span(0);
		if (!_keyDic.Deserialize(span))
		{
			return false;
		}
		reader.Advance(value);
		if (!reader.TryRead_Native(out int _))
		{
			return false;
		}
		JsonBinSpan span2 = reader.Span(0);
		if (!_valueStringDic.Deserialize(span2))
		{
			return false;
		}
		return true;
	}

	public static int StringToHashCode(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return 0;
		}
		int num = 5381;
		int num2 = num;
		int length = str.Length;
		int num3 = 0;
		while (num3 < length)
		{
			int num4 = str[num3++];
			if (num4 == 0)
			{
				break;
			}
			num = ((num << 5) + num) ^ num4;
			if (num3 >= length)
			{
				break;
			}
			num4 = str[num3++];
			if (num4 == 0)
			{
				break;
			}
			num2 = ((num2 << 5) + num2) ^ num4;
		}
		return num + num2 * 1566083941;
	}
}
