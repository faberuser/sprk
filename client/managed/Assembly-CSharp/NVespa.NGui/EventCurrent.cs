using UnityEngine;

namespace NVespa.NGui;

public static class EventCurrent
{
	private static bool _isUsed;

	private static EventType _currentType;

	private static float _prevTime;

	public static bool isUsed
	{
		get
		{
			updateCurrent();
			return _isUsed;
		}
	}

	public static bool Use()
	{
		if (isUsed)
		{
			return false;
		}
		_isUsed = true;
		Event.current.Use();
		return true;
	}

	private static void updateCurrent()
	{
		if (Event.current.rawType != _currentType || _prevTime != Time.realtimeSinceStartup)
		{
			_currentType = Event.current.rawType;
			_prevTime = Time.realtimeSinceStartup;
			_isUsed = false;
		}
	}
}
