using System;
using System.Collections.Generic;
using NShared;

namespace NGame2.NBattleThread.NEventService;

public class EventRaiser : IBattleEventRaiser
{
	public class EventQueue
	{
		public HashSet<object> KeySet = new HashSet<object>();

		public List<Action> RaiseActionList = new List<Action>(20);
	}

	private EventQueue _queue = new EventQueue();

	private EventRouter _router;

	public EventRaiser(EventRouter router)
	{
		_router = router;
	}

	public void FlushQueue()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_queue.KeySet) || !__Extension_Method_ICollection__.IsNullOrEmpty(_queue.RaiseActionList))
		{
			_queue.KeySet.Clear();
			for (int i = 0; i < _queue.RaiseActionList.Count; i++)
			{
				Vespa_System_Action_Extension_Method.fire(_queue.RaiseActionList[i]);
			}
			_queue.RaiseActionList.Clear();
		}
	}

	void IBattleEventRaiser.RaiseBattleEvent<TContext, TData>(TContext context, TData data)
	{
		if (context == null || context.BattleInstance == null)
		{
			return;
		}
		if (_queue == null)
		{
			RaiseBattleEventInternal(context, data);
		}
		else if (__Extension_Method_ICollection__.IsNullOrEmpty(_queue.KeySet))
		{
			RaiseBattleEventInternal(context, data);
		}
		else if (!_queue.KeySet.Remove(data))
		{
			_queue.RaiseActionList.Add(delegate
			{
				RaiseBattleEventInternal(context, data);
			});
		}
		else if (__Extension_Method_ICollection__.IsNullOrEmpty(_queue.KeySet))
		{
			for (int num = 0; num < _queue.RaiseActionList.Count; num++)
			{
				Vespa_System_Action_Extension_Method.fire(_queue.RaiseActionList[num]);
			}
			_queue.RaiseActionList.Clear();
		}
	}

	void IBattleEventRaiser.QueueBattleEvent<TContext, TData>(TContext context, TData data)
	{
		if (_queue == null)
		{
			_queue = new EventQueue();
		}
		if (_queue.KeySet.Add(data))
		{
			_queue.RaiseActionList.Add(delegate
			{
				RaiseBattleEventInternal(context, data);
			});
		}
	}

	private void RaiseBattleEventInternal<TContext, TData>(TContext context, TData data) where TContext : IBattleEventServiceContext where TData : BaseBattleCallback
	{
		if (_router != null)
		{
			_router.Handle(context, data);
		}
	}
}
