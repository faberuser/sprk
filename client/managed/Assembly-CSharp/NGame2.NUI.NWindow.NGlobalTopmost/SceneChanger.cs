using NGame2.NUI.NGame3DWidget;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow.NGlobalTopmost;

public class SceneChanger : GlobalTopmostWindow.IPrivateCaller
{
	private class ChangerTimer : ILocalTimer
	{
		public float TimeScale = 1f;

		public int Tick => Mathf.RoundToInt((float)UnitySystemTimer.unscaledTick * TimeScale);

		int ILocalTimer.deltaTick => Mathf.RoundToInt(TimeScale * (float)UnitySystemTimer.unscaledDeltaTick);

		int ILocalTimer.playbackSpeed => Mathf.RoundToInt(TimeScale * 1000f);

		bool ILocalTimer.pause => false;

		ETimerEventFlag ILocalTimer.TotalEventFlag => (ETimerEventFlag)0;

		EAudioMixerFlag ILocalTimer.TotalAudioMixerFlag => EAudioMixerFlag.None;

		int ILocalTimer.GetPlaybackSpeedRecursively(int depth)
		{
			return Mathf.RoundToInt(TimeScale * 1000f);
		}

		bool ILocalTimer.GetPauseRecursively(int depth)
		{
			return false;
		}
	}

	public const float AnimateTimeLength = 0.75f;

	private ChangerTimer _timer = new ChangerTimer();

	private GlobalTopmostWindow.ESceneChangeType _changeType;

	private float _timeOut;

	private int _createTick;

	private bool _isOpening;

	private int _animateBeginTick;

	private IContents _effectContents;

	private float _timeScale = 1f;

	public float GetAnimationFrame()
	{
		if (_effectContents == null)
		{
			return 1f;
		}
		return Mathf.Clamp01((float)(_timer.Tick - _animateBeginTick) * 0.001f / 0.75f);
	}

	public bool IsCompleteAnimation()
	{
		if (_effectContents == null)
		{
			return true;
		}
		return (float)(_timer.Tick - _animateBeginTick) * 0.001f >= 0.75f;
	}

	public bool IsClosed()
	{
		if (_effectContents == null)
		{
			return true;
		}
		if (_isOpening)
		{
			return false;
		}
		return IsCompleteAnimation();
	}

	public void Open(float timeOut, GlobalTopmostWindow.ESceneChangeType type, float timeScale = 1f)
	{
		_changeType = type;
		_timeOut = timeOut;
		_timer.TimeScale = (_timeScale = timeScale);
		switch (type)
		{
		case GlobalTopmostWindow.ESceneChangeType.BlackScreen:
		{
			ScreenEffect.StockData stockData4 = new ScreenEffect.StockData();
			stockData4.Code = "UI.Common.SceneChange";
			stockData4.LocalTimer = _timer;
			_effectContents = ScreenEffect.PlayEffect(GlobalUIRoot.ELayerType.GlobalUI, stockData4);
			break;
		}
		case GlobalTopmostWindow.ESceneChangeType.Cloud:
		{
			ScreenEffect.StockData stockData3 = new ScreenEffect.StockData();
			stockData3.Code = "UI.Cloud";
			stockData3.LocalTimer = _timer;
			_effectContents = ScreenEffect.PlayEffect(GameUIRoot.ELayerType.UI, stockData3);
			break;
		}
		case GlobalTopmostWindow.ESceneChangeType.EclipseNextCampaign:
		case GlobalTopmostWindow.ESceneChangeType.ChangeBattleField:
		{
			ScreenEffect.StockData stockData2 = new ScreenEffect.StockData();
			stockData2.Code = "UI.EclipsDungeon.SceneChange";
			stockData2.LocalTimer = _timer;
			_effectContents = ScreenEffect.PlayEffect(GlobalUIRoot.ELayerType.GlobalUI, stockData2);
			break;
		}
		case GlobalTopmostWindow.ESceneChangeType.EclipseNextTeam:
		{
			ScreenEffect.StockData stockData = new ScreenEffect.StockData();
			stockData.Code = "UI.EclipsDungeon.SceneChangeHero";
			stockData.LocalTimer = _timer;
			_effectContents = ScreenEffect.PlayEffect(GlobalUIRoot.ELayerType.GlobalUI, stockData);
			break;
		}
		}
		_isOpening = true;
		_animateBeginTick = _timer.Tick;
		_createTick = _timer.Tick;
	}

	public void Close()
	{
		if (_effectContents != null)
		{
			_isOpening = false;
			_animateBeginTick = _timer.Tick;
			_effectContents.PlayPhase("End");
		}
	}

	void GlobalTopmostWindow.IPrivateCaller.Release()
	{
		if (_effectContents != null)
		{
			_effectContents.ReleaseAll();
			_effectContents = null;
		}
	}

	public bool IsTimeout()
	{
		return (float)(_timer.Tick - _createTick) * 0.001f > _timeOut;
	}

	public void Pause()
	{
		_timer.TimeScale = 0f;
	}

	public void Resume()
	{
		_timer.TimeScale = _timeScale;
	}
}
