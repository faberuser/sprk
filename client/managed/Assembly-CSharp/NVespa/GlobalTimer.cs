using UnityEngine;

namespace NVespa;

public class GlobalTimer
{
	public enum EPauseFlag
	{
		Tutorial = 1,
		CutScene = 2,
		Menu = 4,
		ScreenFilter = 8
	}

	private class LocalTimerForGlobal : ILocalTimer
	{
		int ILocalTimer.deltaTick => deltaTick;

		int ILocalTimer.playbackSpeed => 1000;

		bool ILocalTimer.pause => pause;

		ETimerEventFlag ILocalTimer.TotalEventFlag => (ETimerEventFlag)0;

		EAudioMixerFlag ILocalTimer.TotalAudioMixerFlag => EAudioMixerFlag.None;

		int ILocalTimer.GetPlaybackSpeedRecursively(int depth)
		{
			return 1000;
		}

		bool ILocalTimer.GetPauseRecursively(int depth)
		{
			return pause;
		}
	}

	private class UnscaledForGlobal : ILocalTimer
	{
		int ILocalTimer.deltaTick
		{
			get
			{
				if (!pause)
				{
					return UnitySystemTimer.unscaledDeltaTick;
				}
				return 0;
			}
		}

		int ILocalTimer.playbackSpeed => 1000;

		bool ILocalTimer.pause => pause;

		ETimerEventFlag ILocalTimer.TotalEventFlag => (ETimerEventFlag)0;

		EAudioMixerFlag ILocalTimer.TotalAudioMixerFlag => EAudioMixerFlag.None;

		int ILocalTimer.GetPlaybackSpeedRecursively(int depth)
		{
			return 1000;
		}

		bool ILocalTimer.GetPauseRecursively(int depth)
		{
			return pause;
		}
	}

	public static ILocalTimer DefaultLocal;

	public static ILocalTimer UnscaledLocal;

	private static int _tick;

	private static int _deltaTick;

	private static int _frameCount;

	private static float _time;

	private static float _deltaTime;

	private static int _unityTickBase;

	private static int _vespaTickBase;

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
			return _time;
		}
	}

	public static float deltaTime
	{
		get
		{
			Refresh();
			return _deltaTime;
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

	public static EPauseFlag pauseFlag { get; private set; }

	public static bool pause => pauseFlag != (EPauseFlag)0;

	static GlobalTimer()
	{
		DefaultLocal = new LocalTimerForGlobal();
		UnscaledLocal = new UnscaledForGlobal();
		Reset();
	}

	public static void Reset()
	{
		pauseFlag = (EPauseFlag)0;
		_tick = 0;
		_deltaTick = 0;
		_frameCount = -1;
		_unityTickBase = 0;
		_vespaTickBase = 0;
	}

	public static void PauseTimer(EPauseFlag flag)
	{
		ResetTimeOffset();
		pauseFlag |= flag;
	}

	public static void ResumeTimer(EPauseFlag flag)
	{
		ResetTimeOffset();
		pauseFlag &= ~flag;
	}

	public static void Refresh()
	{
		if (_frameCount == Time.frameCount)
		{
			return;
		}
		if (pause)
		{
			_tick = _vespaTickBase;
			_deltaTick = 0;
		}
		else
		{
			int num = (int)(Time.time * 1000f) + (_vespaTickBase - _unityTickBase);
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
		_frameCount = Time.frameCount;
		_time = (float)_tick * 0.001f;
		_deltaTime = (float)_deltaTick * 0.001f;
	}

	public static void ResetTimeOffset()
	{
		Refresh();
		_unityTickBase = (int)(Time.time * 1000f);
		_vespaTickBase = _tick;
	}
}
