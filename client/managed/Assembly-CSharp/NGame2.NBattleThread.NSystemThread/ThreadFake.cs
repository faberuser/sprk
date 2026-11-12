using System;
using System.Threading;
using NVespa;
using UnityEngine;

namespace NGame2.NBattleThread.NSystemThread;

public class ThreadFake : MonoBehaviour, ISystemThread
{
	private ThreadInstance _threadInstance;

	private bool _isOnlineMode;

	private int _battleThreadRunning;

	public static ThreadFake CreateThreadFake(ThreadInstance threadInstance, bool isOnlineMode)
	{
		ThreadFake threadFake = new GameObject("ThreadFake").AddComponent<ThreadFake>();
		threadFake._threadInstance = threadInstance;
		threadFake._isOnlineMode = isOnlineMode;
		return threadFake;
	}

	void ISystemThread.StartThread()
	{
		if (_battleThreadRunning <= 0)
		{
			Interlocked.Exchange(ref _battleThreadRunning, 1);
		}
	}

	void ISystemThread.StopThread()
	{
		Interlocked.Exchange(ref _battleThreadRunning, 0);
	}

	private void Update()
	{
		if (_battleThreadRunning == 0 || _threadInstance == null)
		{
			return;
		}
		try
		{
			if (_isOnlineMode)
			{
				_threadInstance.ThreadUpdateOnline();
			}
			else
			{
				_threadInstance.ThreadUpdateOffline();
			}
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
		}
	}
}
