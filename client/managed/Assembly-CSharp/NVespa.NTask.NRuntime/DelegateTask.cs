using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NTask.NRuntime;

[IgnoreTaskStack]
public class DelegateTask : ITask
{
	private readonly IEnumerator<TaskCurrentData> _action;

	private readonly TaskDebugInfo _debugInfo;

	string ITask.Name
	{
		get
		{
			return _debugInfo.Name;
		}
		set
		{
			_debugInfo.Name = value;
		}
	}

	public DelegateTask(IEnumerator<TaskCurrentData> action)
	{
		_debugInfo = new TaskDebugInfo(this);
		_action = action;
	}

	public DelegateTask(IEnumerator enumerator)
	{
		_debugInfo = new TaskDebugInfo(this);
		_action = coEnumerator(enumerator);
	}

	public DelegateTask(Action action)
	{
		_debugInfo = new TaskDebugInfo(this);
		_action = coAction(action);
	}

	public DelegateTask(Func<ITask> taskCreator)
	{
		_debugInfo = new TaskDebugInfo(this);
		_action = coTaskCreator(taskCreator);
	}

	public override string ToString()
	{
		return _debugInfo.ToString();
	}

	private static IEnumerator<TaskCurrentData> coAction(Action action)
	{
		Vespa_System_Action_Extension_Method.fire(action);
		yield return new TaskCurrentData(ETaskResult.Success);
	}

	private IEnumerator<TaskCurrentData> coTaskCreator(Func<ITask> taskCreator)
	{
		ITask runningTask = Vespa_System_Action_Extension_Method.fire(taskCreator);
		using (TaskRunner runner = new TaskRunner(_debugInfo, runningTask))
		{
			while (runner.MoveNextWhileRunning())
			{
				yield return runner.Current;
			}
		}
		yield return new TaskCurrentData(ETaskResult.Success);
	}

	private static IEnumerator<TaskCurrentData> coEnumerator(IEnumerator enumerator)
	{
		if (enumerator == null)
		{
			yield return new TaskCurrentData(ETaskResult.Success);
			yield break;
		}
		Stack<IEnumerator> stack = new Stack<IEnumerator>();
		stack.Push(enumerator);
		while (stack.Count > 0)
		{
			IEnumerator enumerator2 = stack.Peek();
			if (!enumerator2.MoveNext())
			{
				stack.Pop();
				continue;
			}
			object current = enumerator2.Current;
			if (current is IEnumerator item)
			{
				stack.Push(item);
				continue;
			}
			while (HandleUnityInstruction(current))
			{
				yield return new TaskCurrentData(ETaskResult.Running);
			}
			yield return new TaskCurrentData(ETaskResult.Running);
		}
		yield return new TaskCurrentData(ETaskResult.Success);
	}

	private static bool HandleUnityInstruction(object instruction)
	{
		if (instruction is WWW wWW)
		{
			return !wWW.isDone;
		}
		_ = instruction is Coroutine;
		return false;
	}

	IEnumerator<TaskCurrentData> ITask.Run()
	{
		return _action;
	}
}
