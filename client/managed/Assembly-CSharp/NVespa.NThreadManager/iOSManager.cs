using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NVespa.NUtil;
using UnityEngine;

namespace NVespa.NThreadManager;

public class iOSManager : ThreadManager.IPlatformManager
{
	private class ThreadInstance : ThreadManager.IThreadInstance
	{
		public Action Action;

		public Action<ThreadManager.IThreadInstance> InstanceAction;

		public Action Callback;

		void ThreadManager.IThreadInstance.Notify(string notify)
		{
		}
	}

	private object _threadQueueSync = new object();

	private object _mainQueueSync = new object();

	private Queue<ThreadInstance> _threadQueue = new Queue<ThreadInstance>();

	private Queue<Action> _mainQueue = new Queue<Action>();

	private Executer _globalExecuter;

	private int _processorCount;

	private int _maxThreadCount;

	private int _currentThreadCount;

	private int _threadNo;

	private float _lastTimeForThreadCount;

	private int _threadCountPerSecond;

	private float _maxThreadCountPerSecond;

	public int TotalThreadQueueCount => _threadQueue.Count + _currentThreadCount;

	public iOSManager(int maxThreadCount)
	{
		_processorCount = SystemInfo.processorCount;
		_maxThreadCount = Mathf.Max(2, _processorCount - 2);
		_maxThreadCount = Mathf.Min(_maxThreadCount, maxThreadCount);
		_globalExecuter = SingletonUpdater<Executer>.instance;
	}

	string ThreadManager.IPlatformManager.Dump(bool detail)
	{
		return $"ThreadQueue={_threadQueue.Count}, MainQueue={_mainQueue.Count}, CurThreadCount={_currentThreadCount}, MaxThreadCountPerSecond={_maxThreadCountPerSecond}";
	}

	public void Enqueue(Action action, Action onComplete)
	{
		ThreadInstance threadInstance = new ThreadInstance();
		threadInstance.Action = action;
		threadInstance.Callback = onComplete;
		lock (_threadQueueSync)
		{
			_threadQueue.Enqueue(threadInstance);
		}
	}

	public void Enqueue(Action<ThreadManager.IThreadInstance> action, Action onComplete)
	{
		ThreadInstance threadInstance = new ThreadInstance();
		threadInstance.InstanceAction = action;
		threadInstance.Callback = onComplete;
		lock (_threadQueueSync)
		{
			_threadQueue.Enqueue(threadInstance);
		}
	}

	public void ChainUpdate()
	{
		if (ThreadManager.MainThreadIntercept != ThreadManager.MainThreadInterceptType.None)
		{
			if (ThreadManager.MainThreadIntercept == ThreadManager.MainThreadInterceptType.DisableThread)
			{
				while (ExecOneOfThreadQueue(isThread: false))
				{
				}
			}
			else if (ThreadManager.MainThreadIntercept == ThreadManager.MainThreadInterceptType.DisableThread_Single)
			{
				ExecOneOfThreadQueue(isThread: false);
			}
			else if (TotalThreadQueueCount > _maxThreadCount)
			{
				if (ThreadManager.MainThreadIntercept == ThreadManager.MainThreadInterceptType.Single)
				{
					ExecOneOfThreadQueue(isThread: false);
				}
				else if (ThreadManager.MainThreadIntercept == ThreadManager.MainThreadInterceptType.Total)
				{
					while (TotalThreadQueueCount > _maxThreadCount && ExecOneOfThreadQueue(isThread: false))
					{
					}
				}
			}
		}
		if (ThreadManager.MainThreadIntercept != ThreadManager.MainThreadInterceptType.DisableThread && ThreadManager.MainThreadIntercept != ThreadManager.MainThreadInterceptType.DisableThread_Single)
		{
			int num = _maxThreadCount - _currentThreadCount;
			for (int i = 0; i < num; i++)
			{
				if (_threadQueue.Count == 0)
				{
					break;
				}
				Interlocked.Increment(ref _currentThreadCount);
				Thread thread = new Thread(ThreadStart);
				thread.Name = $"VespaThread {_threadNo}";
				thread.Start();
				_threadNo++;
				_threadCountPerSecond++;
			}
		}
		while (true)
		{
			Action action = null;
			if (_mainQueue.Count > 0)
			{
				lock (_mainQueueSync)
				{
					if (_mainQueue.Count > 0)
					{
						action = _mainQueue.Dequeue();
					}
				}
			}
			if (action == null)
			{
				break;
			}
			Vespa_System_Action_Extension_Method.fire(action);
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num2 = realtimeSinceStartup - _lastTimeForThreadCount;
		if (num2 > 1f)
		{
			float b = (float)_threadCountPerSecond / num2;
			_maxThreadCountPerSecond = Mathf.Max(_maxThreadCountPerSecond, b);
			_lastTimeForThreadCount = realtimeSinceStartup;
			_threadCountPerSecond = 0;
		}
	}

	private void ThreadStart()
	{
		Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		int num = 0;
		while (ExecOneOfThreadQueue(isThread: true))
		{
			num++;
		}
		Interlocked.Decrement(ref _currentThreadCount);
	}

	private bool ExecOneOfThreadQueue(bool isThread)
	{
		if (_threadQueue.Count == 0)
		{
			return false;
		}
		ThreadInstance threadInstance = null;
		lock (_threadQueueSync)
		{
			if (_threadQueue.Count > 0)
			{
				threadInstance = _threadQueue.Dequeue();
			}
		}
		if (threadInstance == null)
		{
			return false;
		}
		Vespa_System_Action_Extension_Method.fire(threadInstance.Action);
		Vespa_System_Action_Extension_Method.fire(threadInstance.InstanceAction, threadInstance);
		if (threadInstance.Callback != null)
		{
			lock (_mainQueueSync)
			{
				_mainQueue.Enqueue(threadInstance.Callback);
			}
		}
		return true;
	}
}
