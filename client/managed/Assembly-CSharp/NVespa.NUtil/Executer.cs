using System;
using System.Collections.Generic;

namespace NVespa.NUtil;

public class Executer : SingletonUpdater<Executer>
{
	public Action UpdateDelegate;

	private List<Tuple<WeakReference, Action, float>> _delayList = new List<Tuple<WeakReference, Action, float>>();

	private List<Tuple<WeakReference, Action, int>> _frameDelayList = new List<Tuple<WeakReference, Action, int>>();

	private List<Tuple<WeakReference, Action, DateTime>> _systemDelayList = new List<Tuple<WeakReference, Action, DateTime>>();

	private object _nextListSync = new object();

	private List<Tuple<WeakReference, Action>> _nextList = new List<Tuple<WeakReference, Action>>();

	public static int ThreadRunCount { get; private set; }

	protected override bool ThreadSafe => true;

	public void DelayRun(float delay, object obj, Action action)
	{
		if (obj == null)
		{
			return;
		}
		WeakReference i_item = new WeakReference(obj);
		lock (_delayList)
		{
			_delayList.Add(Tuple.Create(i_item, action, GlobalTimer.time + delay));
		}
	}

	public void FrameDelayRun(int frameCount, object obj, Action action)
	{
		if (obj == null)
		{
			return;
		}
		WeakReference i_item = new WeakReference(obj);
		lock (_frameDelayList)
		{
			_frameDelayList.Add(Tuple.Create(i_item, action, GlobalTimer.frameCount + frameCount));
		}
	}

	public void SystemDelayRun(float delay, object obj, Action action)
	{
		if (obj == null)
		{
			return;
		}
		WeakReference i_item = new WeakReference(obj);
		lock (_systemDelayList)
		{
			DateTime i_item2 = DateTime.Now.AddSeconds(delay);
			_systemDelayList.Add(Tuple.Create(i_item, action, i_item2));
		}
	}

	public void NextRun(object obj, Action action)
	{
		if (obj == null)
		{
			return;
		}
		WeakReference i_item = new WeakReference(obj);
		lock (_nextListSync)
		{
			_nextList.Add(Tuple.Create(i_item, action));
		}
	}

	public void NextRun(Action action)
	{
		lock (_nextListSync)
		{
			_nextList.Add(Tuple.Create<WeakReference, Action>(null, action));
		}
	}

	public void RunOrPendingIfNotMainThread(Action action)
	{
		if (UnityCache.IsMainThread)
		{
			Vespa_System_Action_Extension_Method.fire(action);
		}
		else
		{
			NextRun(action);
		}
	}

	public override void ChainUpdate()
	{
		Vespa_System_Action_Extension_Method.fire(UpdateDelegate);
		lock (_delayList)
		{
			for (int i = 0; i < _delayList.Count; i++)
			{
				Tuple<WeakReference, Action, float> tuple = _delayList[i];
				if (!(tuple.Item3 > GlobalTimer.time))
				{
					if (!Vespa_Common_Class_Extension_Method.IsUnityNull(tuple.Item1.Target))
					{
						Vespa_System_Action_Extension_Method.fire(tuple.Item2);
					}
					_delayList.RemoveAt(i);
					i--;
				}
			}
		}
		lock (_frameDelayList)
		{
			for (int j = 0; j < _frameDelayList.Count; j++)
			{
				Tuple<WeakReference, Action, int> tuple2 = _frameDelayList[j];
				if (tuple2.Item3 <= GlobalTimer.frameCount)
				{
					if (!Vespa_Common_Class_Extension_Method.IsUnityNull(tuple2.Item1.Target))
					{
						Vespa_System_Action_Extension_Method.fire(tuple2.Item2);
					}
					_frameDelayList.RemoveAt(j);
					j--;
				}
			}
		}
		lock (_systemDelayList)
		{
			for (int k = 0; k < _systemDelayList.Count; k++)
			{
				Tuple<WeakReference, Action, DateTime> tuple3 = _systemDelayList[k];
				if (!(tuple3.Item3 > DateTime.Now))
				{
					if (!Vespa_Common_Class_Extension_Method.IsUnityNull(tuple3.Item1.Target))
					{
						Vespa_System_Action_Extension_Method.fire(tuple3.Item2);
					}
					_systemDelayList.RemoveAt(k);
					k--;
				}
			}
		}
		while (true)
		{
			Tuple<WeakReference, Action> tuple4 = null;
			if (_nextList.Count > 0)
			{
				lock (_nextListSync)
				{
					if (_nextList.Count > 0)
					{
						tuple4 = _nextList[0];
						_nextList.RemoveAt(0);
					}
				}
			}
			if (tuple4 != null)
			{
				if (tuple4.Item1 == null || !Vespa_Common_Class_Extension_Method.IsUnityNull(tuple4.Item1.Target))
				{
					Vespa_System_Action_Extension_Method.fire(tuple4.Item2);
				}
				continue;
			}
			break;
		}
	}
}
