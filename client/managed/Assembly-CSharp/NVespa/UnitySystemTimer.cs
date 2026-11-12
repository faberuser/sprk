using UnityEngine;

namespace NVespa;

public class UnitySystemTimer
{
	private static int _tick;

	private static int _deltaTick;

	private static int _unscaledTick;

	private static int _unscaledDeltaTick;

	private static int _systemTick;

	private static int _systemDeltaTick;

	private static int _frameCount;

	public static int tick
	{
		get
		{
			Refresh();
			return _tick;
		}
	}

	public static int deltaTick
	{
		get
		{
			Refresh();
			return _deltaTick;
		}
	}

	public static float time
	{
		get
		{
			Refresh();
			return (float)_tick * 0.001f;
		}
	}

	public static float deltaTime
	{
		get
		{
			Refresh();
			return (float)_deltaTick * 0.001f;
		}
	}

	public static int unscaledTick
	{
		get
		{
			Refresh();
			return _unscaledTick;
		}
	}

	public static int unscaledDeltaTick
	{
		get
		{
			Refresh();
			return _unscaledDeltaTick;
		}
	}

	public static float unscaledTime => (float)unscaledTick * 0.001f;

	public static float unscaledDeltaTime => (float)unscaledDeltaTick * 0.001f;

	public static int SystemTick
	{
		get
		{
			Refresh();
			return _systemTick;
		}
	}

	public static int SystemDeltaTick
	{
		get
		{
			Refresh();
			return _systemDeltaTick;
		}
	}

	public static int frameCount
	{
		get
		{
			Refresh();
			return _frameCount;
		}
	}

	static UnitySystemTimer()
	{
		_tick = 0;
		_deltaTick = 0;
		_unscaledTick = 0;
		_unscaledDeltaTick = 0;
		_frameCount = -1;
	}

	public static void Refresh()
	{
		if (VespaDebug.IsUnityMainThread() && _frameCount != Time.frameCount)
		{
			RefreshTick();
			RefreshUnscaledTick();
			RefreshSystemTick();
			_frameCount = Time.frameCount;
		}
	}

	public static void RefreshTick()
	{
		int num = (int)(Time.time * 1000f);
		if (_frameCount + 1 == Time.frameCount)
		{
			_deltaTick = num - _tick;
			_tick = num;
		}
		else
		{
			_deltaTick = (int)(Time.deltaTime * 1000f);
			_tick = num;
		}
	}

	public static void RefreshUnscaledTick()
	{
		int num = (int)(Time.unscaledTime * 1000f);
		if (_frameCount + 1 == Time.frameCount)
		{
			_unscaledDeltaTick = num - _unscaledTick;
			_unscaledTick = num;
		}
		else
		{
			_unscaledDeltaTick = (int)(Time.unscaledDeltaTime * 1000f);
			_unscaledTick = num;
		}
	}

	public static void RefreshSystemTick()
	{
		int num = (int)(Time.realtimeSinceStartup * 1000f);
		if (_frameCount + 1 == Time.frameCount)
		{
			_systemDeltaTick = num - _systemTick;
		}
		else
		{
			_systemDeltaTick = (int)(Time.unscaledDeltaTime * 1000f);
		}
		_systemTick = num;
	}
}
