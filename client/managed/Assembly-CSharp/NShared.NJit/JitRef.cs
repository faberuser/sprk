using System;
using NShared.NJit.NCommon;
using NShared.NJit.NResolver;

namespace NShared.NJit;

public class JitRef<T> : IJitContainer where T : class
{
	private T _original_in_JsonLoad;

	private bool _jitLoaded;

	private JitPacker _packer;

	private ByteSpan _span;

	void IJitContainer.Bind(JitPacker packer)
	{
		_packer = packer;
	}

	void IJitContainer.Reset()
	{
		_original_in_JsonLoad = null;
		_jitLoaded = false;
		_span = default(ByteSpan);
	}

	public bool JsonLoad_IsNull()
	{
		return _original_in_JsonLoad == null;
	}

	public T JsonLoad_GetOrNew()
	{
		_original_in_JsonLoad = _original_in_JsonLoad ?? Activator.CreateInstance<T>();
		return _original_in_JsonLoad;
	}

	public void JsonLoad_Set(T value)
	{
		_original_in_JsonLoad = value;
	}

	public T Eval()
	{
		if (_original_in_JsonLoad != null)
		{
			return _original_in_JsonLoad;
		}
		if (!_jitLoaded)
		{
			return null;
		}
		ByteSpan nextSpan;
		return Proxy<T>.DataResolver.EvaluateReference(_span, _packer, out nextSpan);
	}

	void IJitContainer.Deserialize(ByteSpan span)
	{
		_jitLoaded = true;
		_span = span;
	}

	bool IJitContainer.IsPackerValid()
	{
		return _packer != null;
	}
}
