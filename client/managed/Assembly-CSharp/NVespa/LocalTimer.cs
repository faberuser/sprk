using System;

namespace NVespa;

public class LocalTimer : ILocalTimer
{
	public Func<int> playbackSpeedFunc;

	private ILocalTimer _parent;

	private int _tick;

	private int _playbackSpeed;

	private int _deltaTick;

	private int _frameCount;

	public int tick
	{
		get
		{
			refresh();
			return _tick;
		}
	}

	public int playbackSpeed => GetPlaybackSpeedRecursively(0);

	public int deltaTick
	{
		get
		{
			refresh();
			return _deltaTick;
		}
	}

	public float time
	{
		get
		{
			refresh();
			return (float)_tick * 0.001f;
		}
	}

	public float deltaTime
	{
		get
		{
			refresh();
			return (float)_deltaTick * 0.001f;
		}
	}

	public ETimerEventFlag EventFlag { get; private set; }

	public EAudioMixerFlag AudioMixerFlag { get; set; }

	public bool pause => GetPauseRecursively(0);

	ETimerEventFlag ILocalTimer.TotalEventFlag => GetTotalEventFlag();

	EAudioMixerFlag ILocalTimer.TotalAudioMixerFlag => GetAudioMixerFlag();

	public LocalTimer(ILocalTimer parent)
	{
		_parent = parent;
		_frameCount = -1;
	}

	public void Reset(ILocalTimer parent)
	{
		_parent = parent;
		_tick = 0;
		_playbackSpeed = 1000;
		_deltaTick = 0;
		_frameCount = -1;
		playbackSpeedFunc = null;
		EventFlag = (ETimerEventFlag)0;
	}

	public void EventSet(ETimerEventFlag flag, bool isSet)
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

	public int GetPlaybackSpeedRecursively(int depth)
	{
		refresh();
		if (depth > 30)
		{
			VespaDebug.LogError("Infinite Loop");
			return _playbackSpeed;
		}
		if (_parent != null)
		{
			return _parent.GetPlaybackSpeedRecursively(depth + 1) * _playbackSpeed / 1000;
		}
		return _playbackSpeed;
	}

	public bool GetPauseRecursively(int depth)
	{
		if (GlobalTimer.pause)
		{
			return true;
		}
		if (depth > 30)
		{
			VespaDebug.LogError("Infinite Loop");
			return MainTimer.pause;
		}
		return TimerEventAttribute.GetAttribute(GetTotalEventFlag().PauseFlagMax()).PauseType == TimerEventAttribute.EPauseType.Pause;
	}

	internal ETimerEventFlag GetTotalEventFlag()
	{
		return EventFlag | _parent.TOTALEVENTFLAG();
	}

	internal EAudioMixerFlag GetAudioMixerFlag()
	{
		return AudioMixerFlag | _parent.AUDIOMIXERFLAG();
	}

	private int CalcPlaybackSpeed()
	{
		if (playbackSpeedFunc != null)
		{
			return playbackSpeedFunc();
		}
		return 1000;
	}

	private void refresh()
	{
		if (_frameCount != GlobalTimer.frameCount)
		{
			_frameCount = GlobalTimer.frameCount;
			if (pause)
			{
				_playbackSpeed = 0;
				_deltaTick = 0;
				return;
			}
			int num = ((EventFlag != 0) ? GlobalTimer.deltaTick : ((_parent == null) ? MainTimer.deltaTick : _parent.deltaTick));
			_playbackSpeed = CalcPlaybackSpeed();
			_tick += num * _playbackSpeed / 1000;
			_deltaTick = num * _playbackSpeed / 1000;
		}
	}
}
