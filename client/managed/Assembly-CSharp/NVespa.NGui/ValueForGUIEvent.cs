using UnityEngine;

namespace NVespa.NGui;

public class ValueForGUIEvent<T>
{
	private EventType _eventType;

	private T _value;

	private bool _dirty;

	private T _pending;

	public T Value
	{
		get
		{
			if (Event.current.type == _eventType && _dirty)
			{
				_value = _pending;
				_dirty = false;
			}
			return _value;
		}
	}

	public ValueForGUIEvent(EventType i_type)
	{
		_eventType = i_type;
	}

	public ValueForGUIEvent(EventType i_type, T i_init)
	{
		_eventType = i_type;
		_value = i_init;
	}

	public void Assign(T i_value)
	{
		_pending = i_value;
		_dirty = true;
	}
}
