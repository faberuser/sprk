namespace NVespa;

public class MainTimer
{
	public class LocalTimerForMain : ILocalTimer
	{
		int ILocalTimer.deltaTick => deltaTick;

		int ILocalTimer.playbackSpeed => 1000;

		bool ILocalTimer.pause => pause;

		ETimerEventFlag ILocalTimer.TotalEventFlag => EventFlag;

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

	private static int _tick;

	private static int _deltaTick;

	private static int _frameCount;

	private static int _prevGlobalTick;

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

	public static ETimerEventFlag EventFlag { get; private set; }

	public static bool pause
	{
		get
		{
			if (!GlobalTimer.pause)
			{
				return EventFlag.IsEventPause();
			}
			return true;
		}
	}

	static MainTimer()
	{
		DefaultLocal = new LocalTimerForMain();
		Reset();
	}

	public static void Reset()
	{
		_tick = 0;
		_deltaTick = 0;
		_frameCount = -1;
		_prevGlobalTick = 0;
		EventFlag = (ETimerEventFlag)0;
	}

	public static void Refresh()
	{
		if (_frameCount != GlobalTimer.frameCount)
		{
			_frameCount = GlobalTimer.frameCount;
			if (pause)
			{
				_deltaTick = 0;
			}
			else
			{
				_tick += GlobalTimer.tick - _prevGlobalTick;
				_deltaTick = GlobalTimer.deltaTick;
			}
			_prevGlobalTick = GlobalTimer.tick;
		}
	}

	public static void EventSet(ETimerEventFlag flag, bool isSet)
	{
		if (isSet)
		{
			EventFlag |= flag;
		}
		else
		{
			EventFlag &= ~flag;
		}
	}
}
