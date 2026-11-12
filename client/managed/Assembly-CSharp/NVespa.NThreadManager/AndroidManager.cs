using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NGame2.NScene;
using NVespa.NUtil;
using UnityEngine;

namespace NVespa.NThreadManager;

public class AndroidManager : ThreadManager.IPlatformManager
{
	private class ThreadInstance : ThreadManager.IThreadInstance
	{
		public enum EState
		{
			PendingThreadQueue,
			BeginThreadJob,
			EndThreadJob,
			PendingMainQueue,
			BeginMainJob,
			End,
			Exception
		}

		public Action Action;

		public Action<ThreadManager.IThreadInstance> InstanceAction;

		public Action Callback;

		public volatile EState State;

		public volatile string Notify = string.Empty;

		void ThreadManager.IThreadInstance.Notify(string notify)
		{
			Notify = notify;
		}
	}

	private class ThreadData
	{
		public Thread SystemThread;

		public ThreadInstance Current;
	}

	private object _threadQueueSync = new object();

	private object _mainQueueSync = new object();

	private Queue<ThreadInstance> _threadQueue = new Queue<ThreadInstance>();

	private Queue<ThreadInstance> _mainQueue = new Queue<ThreadInstance>();

	private Executer _globalExecuter;

	private List<ThreadData> _threadList = new List<ThreadData>();

	private ThreadData _mainThread = new ThreadData();

	private volatile int _threadExecuteCount;

	private int _processorCount;

	private int _enqueueCount;

	public int TotalThreadQueueCount => _threadQueue.Count + _threadExecuteCount;

	public AndroidManager(int maxThreadCount)
	{
		_processorCount = SystemInfo.processorCount;
		int a = Mathf.Max(2, _processorCount - 2);
		a = Mathf.Min(a, maxThreadCount);
		_globalExecuter = SingletonUpdater<Executer>.instance;
		for (int i = 0; i < a; i++)
		{
			ThreadData threadData = new ThreadData();
			threadData.SystemThread = new Thread(ThreadStart);
			threadData.SystemThread.Priority = System.Threading.ThreadPriority.AboveNormal;
			threadData.SystemThread.Name = $"VespaThread {i}";
			threadData.SystemThread.Start(threadData);
			_threadList.Add(threadData);
		}
	}

	string ThreadManager.IPlatformManager.Dump(bool detail)
	{
		string text = $"ThreadCount={_threadList.Count}, ThreadQueue={_threadQueue.Count}, MainQueue={_mainQueue.Count}, ThreadExe={_threadExecuteCount}, EnqueueCount={_enqueueCount}";
		if (!detail)
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(text);
		return stringBuilder.ToString();
	}

	public void Enqueue(Action action, Action onComplete)
	{
		Interlocked.Increment(ref _enqueueCount);
		ThreadInstance threadInstance = new ThreadInstance();
		threadInstance.Action = action;
		threadInstance.Callback = onComplete;
		threadInstance.State = ThreadInstance.EState.PendingThreadQueue;
		lock (_threadQueueSync)
		{
			_threadQueue.Enqueue(threadInstance);
		}
	}

	public void Enqueue(Action<ThreadManager.IThreadInstance> action, Action onComplete)
	{
		Interlocked.Increment(ref _enqueueCount);
		ThreadInstance threadInstance = new ThreadInstance();
		threadInstance.InstanceAction = action;
		threadInstance.Callback = onComplete;
		threadInstance.State = ThreadInstance.EState.PendingThreadQueue;
		lock (_threadQueueSync)
		{
			_threadQueue.Enqueue(threadInstance);
		}
	}

	public void EnqueueMain(Action onComplete)
	{
	}

	public void ChainUpdate()
	{
		if (ThreadManager.MainThreadIntercept != ThreadManager.MainThreadInterceptType.None)
		{
			if (ThreadManager.MainThreadIntercept == ThreadManager.MainThreadInterceptType.DisableThread)
			{
				while (ExecOneOfThreadQueue(isThread: false, _mainThread))
				{
				}
			}
			else if (ThreadManager.MainThreadIntercept == ThreadManager.MainThreadInterceptType.DisableThread_Single)
			{
				ExecOneOfThreadQueue(isThread: false, _mainThread);
			}
			else if (TotalThreadQueueCount > _threadList.Count)
			{
				if (ThreadManager.MainThreadIntercept == ThreadManager.MainThreadInterceptType.Single)
				{
					ExecOneOfThreadQueue(isThread: false, _mainThread);
				}
				else if (ThreadManager.MainThreadIntercept == ThreadManager.MainThreadInterceptType.Total)
				{
					while (TotalThreadQueueCount > _threadList.Count && ExecOneOfThreadQueue(isThread: false, _mainThread))
					{
					}
				}
			}
		}
		while (true)
		{
			ThreadInstance threadInstance = null;
			if (_mainQueue.Count > 0)
			{
				lock (_mainQueueSync)
				{
					if (_mainQueue.Count > 0)
					{
						threadInstance = _mainQueue.Dequeue();
					}
				}
			}
			if (threadInstance != null)
			{
				if (threadInstance.Callback != null)
				{
					threadInstance.State = ThreadInstance.EState.BeginMainJob;
					Vespa_System_Action_Extension_Method.fire(threadInstance.Callback);
					threadInstance.Callback = null;
				}
				threadInstance.State = ThreadInstance.EState.End;
				continue;
			}
			break;
		}
	}

	private void ThreadStart(object obj)
	{
		ThreadData threadThis = obj as ThreadData;
		while (true)
		{
			if (ThreadManager.MainThreadIntercept == ThreadManager.MainThreadInterceptType.DisableThread || ThreadManager.MainThreadIntercept == ThreadManager.MainThreadInterceptType.DisableThread_Single)
			{
				Thread.Sleep(1000);
			}
			else if (!ExecOneOfThreadQueue(isThread: true, threadThis))
			{
				Thread.Sleep(100);
			}
		}
	}

	private bool ExecOneOfThreadQueue(bool isThread, ThreadData threadThis)
	{
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
		if (threadThis != null)
		{
			threadThis.Current = threadInstance;
		}
		if (isThread)
		{
			Interlocked.Increment(ref _threadExecuteCount);
		}
		threadInstance.State = ThreadInstance.EState.BeginThreadJob;
		try
		{
			_ = LoadingManager.LoadSceneCount;
			Vespa_System_Action_Extension_Method.fire(threadInstance.Action);
			Vespa_System_Action_Extension_Method.fire(threadInstance.InstanceAction, threadInstance);
		}
		catch (Exception ex)
		{
			VespaDebug.LogError(ex.ToString());
			threadInstance.Notify = "ThreadAction Exception: " + ex.ToString();
			threadInstance.State = ThreadInstance.EState.Exception;
		}
		if (threadInstance.State == ThreadInstance.EState.Exception)
		{
			return true;
		}
		threadInstance.State = ThreadInstance.EState.EndThreadJob;
		if (isThread)
		{
			Interlocked.Decrement(ref _threadExecuteCount);
		}
		if (threadThis != null)
		{
			threadThis.Current = null;
		}
		if (threadInstance.Callback != null)
		{
			lock (_mainQueueSync)
			{
				threadInstance.State = ThreadInstance.EState.PendingMainQueue;
				_mainQueue.Enqueue(threadInstance);
			}
		}
		else
		{
			threadInstance.State = ThreadInstance.EState.End;
		}
		return true;
	}
}
