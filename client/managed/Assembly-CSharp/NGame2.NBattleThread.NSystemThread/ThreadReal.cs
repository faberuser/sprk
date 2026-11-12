using System;
using System.Threading;
using NShared;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NBattleThread.NSystemThread;

public class ThreadReal : ISystemThread
{
	private static int _battleCount;

	private ThreadInstance _threadInstance;

	private bool _isOnlineMode;

	private Thread _battleThread;

	private int _sleepMs = 30;

	public ThreadReal(ThreadInstance threadInstance, bool isOnlineMode, bool enhanceMode = false)
	{
		_battleCount++;
		_threadInstance = threadInstance;
		_isOnlineMode = isOnlineMode;
		if (enhanceMode)
		{
			_battleThread = new Thread(ThreadUpdateEnhance);
		}
		else
		{
			_battleThread = new Thread(ThreadUpdate);
		}
		_battleThread.Name = "Kings Raid Battle " + _battleCount;
		SetSleepMs();
	}

	private void SetSleepMs()
	{
		if (_isOnlineMode)
		{
			IBattleEventServiceContext threadInstance = _threadInstance;
			if (threadInstance != null && threadInstance.BattleInstance != null)
			{
				int num = System.Math.Max(threadInstance.BattleInstance.GameSpeedRatio, 1);
				_sleepMs = System.Math.Max(_sleepMs / num, 1);
			}
		}
	}

	void ISystemThread.StartThread()
	{
		if (_battleThread != null)
		{
			_battleThread.Start();
		}
	}

	void ISystemThread.StopThread()
	{
		if (_battleThread != null)
		{
			Thread battleThread = _battleThread;
			Interlocked.Exchange(ref _battleThread, null);
			battleThread.Join();
		}
	}

	private void ThreadUpdate()
	{
		if (_isOnlineMode)
		{
			ThreadUpdateOnline();
		}
		else
		{
			ThreadUpdateOffline();
		}
	}

	private void ThreadUpdateEnhance()
	{
		if (_isOnlineMode)
		{
			ThreadUpdateOnlineEnhnace();
		}
		else
		{
			ThreadUpdateOffline();
		}
	}

	private void ThreadUpdateOffline()
	{
		while (_threadInstance != null && _battleThread != null)
		{
			try
			{
				int num = _threadInstance.ThreadUpdateOffline();
				if (ProfileManager.IsLogEnabled)
				{
					ProfileManager.RecordBattleDelta((float)num / 1000f);
				}
				if (num < _sleepMs)
				{
					Thread.Sleep(_sleepMs - num);
				}
			}
			catch (Exception obj)
			{
				VespaDebug.LogError(obj);
				Thread.Sleep(100);
			}
		}
	}

	private void ThreadUpdateOnline()
	{
		while (_threadInstance != null && _battleThread != null)
		{
			try
			{
				int num = _threadInstance.ThreadUpdateOnline();
				if (ProfileManager.IsLogEnabled)
				{
					ProfileManager.RecordBattleDelta((float)num / 1000f);
				}
				if (num < _sleepMs)
				{
					Thread.Sleep(_sleepMs - num);
				}
			}
			catch (Exception obj)
			{
				VespaDebug.LogError(obj);
				Thread.Sleep(100);
			}
		}
	}

	private void ThreadUpdateOnlineEnhnace()
	{
		while (_threadInstance != null && _battleThread != null)
		{
			try
			{
				int num = _threadInstance.ThreadUpdateOnlineEnhance();
				if (ProfileManager.IsLogEnabled)
				{
					ProfileManager.RecordBattleDelta((float)num / 1000f);
				}
				if (num < _sleepMs)
				{
					Thread.Sleep(_sleepMs - num);
				}
			}
			catch (Exception obj)
			{
				VespaDebug.LogError(obj);
				Thread.Sleep(100);
			}
		}
	}
}
