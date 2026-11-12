using System.Collections.Generic;
using JsonBin.NBinary.NRead;
using JsonBin.NBinary.NWrite;
using NVespa;

namespace JsonBin.NBinary;

public abstract class PackedDictionary<K, V>
{
	private class HashedData
	{
		public List<KeyValuePair<K, V>> pairList = new List<KeyValuePair<K, V>>();

		public byte[] Bytes;

		public int offset;
	}

	public const int SIZE_INT = 4;

	private V _defaultV;

	private int _modSize;

	private JsonBinSpan _hashedOffsetSpan;

	private JsonBinSpan _hashedDataSpan;

	public Dictionary<K, V> Original { get; private set; }

	public int Count => _modSize;

	protected abstract int KeyToHash(K key);

	protected abstract K ReadKeyFromSpan(JsonBinSpan span, out JsonBinSpan nextSpan);

	protected abstract V ReadValueFromSpan(JsonBinSpan span, out JsonBinSpan nextSpan);

	protected abstract void WriteKey(JsonBinWriter writer, K key);

	protected abstract void WriteValue(JsonBinWriter writer, V value);

	public PackedDictionary(V defaultV)
	{
		_defaultV = defaultV;
		Original = new Dictionary<K, V>();
	}

	public bool TryGetValue(K key, out V value)
	{
		lock (Original)
		{
			if (Original.TryGetValue(key, out value))
			{
				return true;
			}
		}
		if (LoadByKey(key))
		{
			lock (Original)
			{
				if (Original.TryGetValue(key, out value))
				{
					return true;
				}
			}
		}
		value = _defaultV;
		return false;
	}

	public IDictionary<K, V> GetAllData()
	{
		if (_modSize == 0)
		{
			return Original;
		}
		for (int i = 0; i < _modSize; i++)
		{
			if (!_hashedOffsetSpan.TryRead_Native(out int value, 4 * i))
			{
				continue;
			}
			JsonBinSpan span = _hashedDataSpan.Slice(value);
			JsonBinSpan nextSpan;
			int num = ReadIntFromSpan(span, out nextSpan);
			if (num == 0)
			{
				continue;
			}
			lock (Original)
			{
				for (int j = 0; j < num; j++)
				{
					JsonBinSpan nextSpan2;
					K key = ReadKeyFromSpan(nextSpan, out nextSpan2);
					V value2 = ReadValueFromSpan(nextSpan2, out nextSpan);
					if (!Original.ContainsKey(key))
					{
						Original.Add(key, value2);
					}
				}
			}
		}
		_modSize = 0;
		_hashedOffsetSpan = default(JsonBinSpan);
		_hashedDataSpan = default(JsonBinSpan);
		return Original;
	}

	protected int ReadIntFromSpan(JsonBinSpan span, out JsonBinSpan nextSpan)
	{
		if (!span.TryReadPacked_Int(out var value, out nextSpan))
		{
			return 0;
		}
		return value;
	}

	private bool LoadByKey(K key)
	{
		if (_modSize == 0)
		{
			return false;
		}
		int num = KeyToHash(key) % _modSize;
		if (num < 0)
		{
			num += _modSize;
		}
		if (!_hashedOffsetSpan.TryRead_Native(out int value, 4 * num))
		{
			return false;
		}
		JsonBinSpan span = _hashedDataSpan.Slice(value);
		JsonBinSpan nextSpan;
		int num2 = ReadIntFromSpan(span, out nextSpan);
		if (num2 == 0)
		{
			return false;
		}
		lock (Original)
		{
			for (int i = 0; i < num2; i++)
			{
				JsonBinSpan nextSpan2;
				K val = ReadKeyFromSpan(nextSpan, out nextSpan2);
				V val2 = ReadValueFromSpan(nextSpan2, out nextSpan);
				if (Original.ContainsKey(val))
				{
					V val3 = Original[val];
					string obj = $"Duplicate Key={val}, Value={val2}, Old Value={val3}";
					VespaDebug.LogError(obj);
				}
				else
				{
					Original.Add(val, val2);
				}
			}
		}
		return true;
	}

	public bool Deserialize(JsonBinSpan span)
	{
		if (!span.TryRead_Native(out _modSize, 0))
		{
			_modSize = 0;
			return false;
		}
		_hashedOffsetSpan = span.Slice(4);
		_hashedDataSpan = _hashedOffsetSpan.Slice(4 * _modSize);
		return true;
	}

	public byte[] Serialize()
	{
		Dictionary<K, V> original = Original;
		int count = original.Count;
		HashedData[] hashedArray = GetHashedArray(original, original.Count);
		for (int i = 0; i < hashedArray.Length; i++)
		{
			HashedData hashedData = hashedArray[i];
			hashedData.offset = 0;
			if (i > 0)
			{
				HashedData hashedData2 = hashedArray[i - 1];
				hashedData.offset = hashedData2.offset + hashedData2.Bytes.Length;
			}
			using SimpleBufferWriter simpleBufferWriter = new SimpleBufferWriter();
			JsonBinWriter jsonBinWriter = new JsonBinWriter(simpleBufferWriter);
			int count2 = hashedData.pairList.Count;
			jsonBinWriter.WritePacked(count2);
			for (int j = 0; j < count2; j++)
			{
				KeyValuePair<K, V> keyValuePair = hashedData.pairList[j];
				WriteKey(jsonBinWriter, keyValuePair.Key);
				WriteValue(jsonBinWriter, keyValuePair.Value);
			}
			hashedData.Bytes = simpleBufferWriter.ToArrayFinal();
		}
		using SimpleBufferWriter simpleBufferWriter2 = new SimpleBufferWriter();
		JsonBinWriter jsonBinWriter2 = new JsonBinWriter(simpleBufferWriter2);
		jsonBinWriter2.Write_Native(count);
		foreach (HashedData hashedData3 in hashedArray)
		{
			jsonBinWriter2.Write_Native(hashedData3.offset);
		}
		foreach (HashedData hashedData4 in hashedArray)
		{
			jsonBinWriter2.CopyDirect(hashedData4.Bytes);
		}
		return simpleBufferWriter2.ToArrayFinal();
	}

	private HashedData[] GetHashedArray(Dictionary<K, V> dic, int modCount)
	{
		HashedData[] array = new HashedData[modCount];
		for (int i = 0; i < modCount; i++)
		{
			array[i] = new HashedData();
		}
		foreach (KeyValuePair<K, V> item in dic)
		{
			int num = KeyToHash(item.Key) % modCount;
			if (num < 0)
			{
				num += modCount;
			}
			array[num].pairList.Add(item);
		}
		return array;
	}
}
