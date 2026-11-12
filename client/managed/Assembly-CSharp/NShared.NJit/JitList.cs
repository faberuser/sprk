using System.Collections.Generic;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit;

public class JitList<V> : IJitContainer
{
	private List<V> _original_in_JsonLoad;

	private JitPacker _packer;

	private bool _jitLoaded;

	private int _valueCount;

	private ByteSpan _offsetSpan;

	private ByteSpan _valueSpan;

	public int Count
	{
		get
		{
			if (_original_in_JsonLoad != null)
			{
				return _original_in_JsonLoad.Count;
			}
			return _valueCount;
		}
	}

	public V this[int index] => Get(index);

	public IDataResolverT<V> ValueResolver { get; private set; }

	public JitList()
	{
		ValueResolver = ResolverContainer.Get<V>();
	}

	void IJitContainer.Bind(JitPacker packer)
	{
		_packer = packer;
	}

	void IJitContainer.Reset()
	{
		_original_in_JsonLoad = null;
		_jitLoaded = false;
		_offsetSpan = default(ByteSpan);
		_valueSpan = default(ByteSpan);
	}

	public List<V> JsonLoad_GetOrNew()
	{
		_original_in_JsonLoad = _original_in_JsonLoad ?? new List<V>();
		return _original_in_JsonLoad;
	}

	public V Get(int index)
	{
		if (_original_in_JsonLoad != null)
		{
			return _original_in_JsonLoad[index];
		}
		if (TryGetFromRawData(index, out var value))
		{
			return value;
		}
		return default(V);
	}

	private bool TryGetFromRawData(int index, out V value)
	{
		if (!_jitLoaded)
		{
			value = default(V);
			return false;
		}
		if (index >= _valueCount)
		{
			value = default(V);
			return false;
		}
		int index2 = _offsetSpan.ReadInt(4 * index);
		ByteSpan span = _valueSpan.Slice(index2);
		value = ValueResolver.EvaluateReference(span, _packer, out var _);
		return true;
	}

	void IJitContainer.Deserialize(ByteSpan span)
	{
		_jitLoaded = true;
		_valueCount = span.ReadInt(0);
		_offsetSpan = span.Slice(4);
		_valueSpan = _offsetSpan.Slice(4 * _valueCount);
	}

	bool IJitContainer.IsPackerValid()
	{
		return _packer != null;
	}
}
