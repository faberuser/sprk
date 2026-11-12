using UnityEngine;

namespace NVespa;

public class FrameCacheValue<T>
{
	private int _frame;

	private T _value;

	public bool NeedUpdate => _frame + 5 < Time.renderedFrameCount;

	public T Value
	{
		get
		{
			return _value;
		}
		set
		{
			_frame = Time.renderedFrameCount;
			_value = value;
		}
	}

	public FrameCacheValue()
	{
		_frame = -1;
		_value = default(T);
	}

	public FrameCacheValue(T value)
	{
		Value = value;
	}
}
