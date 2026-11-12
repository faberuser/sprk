using System.Collections.Generic;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit;

public class JitDictionaryWithKey<K, V> : IJitContainer, IJitDictionaryRead<K, V> where V : IDataKeySelector<K>
{
	private Dictionary<K, V> _cache;

	private JitPacker _packer;

	private bool _jitLoaded;

	private int _countFromJit;

	private int _modSize;

	private ByteSpan _hashedOffsetSpan;

	private ByteSpan _hashedDataSpan;

	public IKeyComparerT<K> KeyComparer { get; private set; }

	public IDataResolverT<V> ValueResolver { get; private set; }

	public int Count => GetCount();

	public JitDictionaryWithKey()
	{
		KeyComparer = ResolverContainer.GetComparer<K>();
		ValueResolver = ResolverContainer.Get<V>();
		_cache = new Dictionary<K, V>(KeyComparer);
	}

	void IJitContainer.Bind(JitPacker packer)
	{
		_packer = packer;
	}

	void IJitContainer.Reset()
	{
		_cache.Clear();
		_jitLoaded = false;
		_countFromJit = 0;
		_modSize = 0;
		_hashedOffsetSpan = default(ByteSpan);
		_hashedDataSpan = default(ByteSpan);
	}

	public Dictionary<K, V> JsonLoad_Get()
	{
		return _cache;
	}

	public int GetCount()
	{
		if (_jitLoaded)
		{
			return _countFromJit;
		}
		lock (_cache)
		{
			return _cache.Count;
		}
	}

	public bool TryGetValue(K key, out V value)
	{
		lock (_cache)
		{
			if (_cache.TryGetValue(key, out value))
			{
				return true;
			}
		}
		if (TryGetFromRawData(key, out value))
		{
			lock (_cache)
			{
				if (!_cache.ContainsKey(key))
				{
					_cache.Add(key, value);
				}
			}
			return true;
		}
		value = default(V);
		return false;
	}

	public List<K> DumpAllKeyForTest()
	{
		List<K> list = new List<K>();
		if (!_jitLoaded)
		{
			return list;
		}
		for (int i = 0; i < _modSize; i++)
		{
			int index = _hashedOffsetSpan.ReadInt(4 * i);
			ByteSpan byteSpan = _hashedDataSpan.Slice(index);
			int num = byteSpan.ReadInt(0);
			if (num != 0)
			{
				ByteSpan nextSpan = byteSpan.Slice(4);
				for (int j = 0; j < num; j++)
				{
					list.Add(ValueResolver.EvaluateReference(nextSpan, _packer, out nextSpan).Key);
				}
			}
		}
		return list;
	}

	public void DevOnly_ForceLoadAllRawData()
	{
		lock (_cache)
		{
			if (!_jitLoaded)
			{
				return;
			}
			for (int i = 0; i < _modSize; i++)
			{
				int index = _hashedOffsetSpan.ReadInt(4 * i);
				ByteSpan byteSpan = _hashedDataSpan.Slice(index);
				int num = byteSpan.ReadInt(0);
				if (num == 0)
				{
					continue;
				}
				ByteSpan nextSpan = byteSpan.Slice(4);
				for (int j = 0; j < num; j++)
				{
					V value = ValueResolver.EvaluateReference(nextSpan, _packer, out nextSpan);
					if (!_cache.ContainsKey(value.Key))
					{
						_cache.Add(value.Key, value);
					}
				}
			}
			_jitLoaded = false;
			_countFromJit = 0;
			_modSize = 0;
			_hashedOffsetSpan = default(ByteSpan);
			_hashedDataSpan = default(ByteSpan);
		}
	}

	private bool TryGetFromRawData(K key, out V value)
	{
		if (!_jitLoaded)
		{
			value = default(V);
			return false;
		}
		if (_modSize == 0)
		{
			value = default(V);
			return false;
		}
		int num = KeyComparer.GetHashCode(key) % _modSize;
		if (num < 0)
		{
			num += _modSize;
		}
		int index = _hashedOffsetSpan.ReadInt(4 * num);
		ByteSpan byteSpan = _hashedDataSpan.Slice(index);
		int num2 = byteSpan.ReadInt(0);
		if (num2 == 0)
		{
			value = default(V);
			return false;
		}
		ByteSpan nextSpan = byteSpan.Slice(4);
		for (int i = 0; i < num2; i++)
		{
			value = ValueResolver.EvaluateReference(nextSpan, _packer, out nextSpan);
			if (KeyComparer.Equals(key, value.Key))
			{
				return true;
			}
		}
		value = default(V);
		return false;
	}

	void IJitContainer.Deserialize(ByteSpan span)
	{
		lock (_cache)
		{
			_cache.Clear();
			_jitLoaded = true;
			_countFromJit = span.ReadInt(0);
			span = span.Slice(4);
			_modSize = span.ReadInt(0);
			span = span.Slice(4);
			_hashedOffsetSpan = span;
			_hashedDataSpan = _hashedOffsetSpan.Slice(4 * _modSize);
		}
	}

	bool IJitContainer.IsPackerValid()
	{
		return _packer != null;
	}
}
