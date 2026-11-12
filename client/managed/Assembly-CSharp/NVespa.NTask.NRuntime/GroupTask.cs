using System;
using System.Collections;
using System.Collections.Generic;

namespace NVespa.NTask.NRuntime;

[IgnoreTaskStack]
public class GroupTask : ITaskContainer, ITask, IDisposable
{
	private SequenceTask _mainSequence;

	private SequenceTask _finalSequence;

	private TaskDebugInfo _debugInfo;

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

	public GroupTask()
	{
		_debugInfo = new TaskDebugInfo(this);
	}

	void IDisposable.Dispose()
	{
		this.Commit();
	}

	public override string ToString()
	{
		return _debugInfo.ToString();
	}

	private void OnEndRun()
	{
	}

	public ITask AddTask(ITask task)
	{
		return GetMainSequenceTask().AddTask(task);
	}

	public ITask AddTask(Action action)
	{
		return GetMainSequenceTask().AddTask(action);
	}

	public ITask AddTask(IEnumerator action)
	{
		return GetMainSequenceTask().AddTask(action);
	}

	public ITask AddTask(IEnumerator<TaskCurrentData> action)
	{
		return GetMainSequenceTask().AddTask(action);
	}

	public ITask AddAsyncTask(Action<Action> asyncAction)
	{
		return AddTask(coAsyncTask(asyncAction));
	}

	private IEnumerator coAsyncTask(Action<Action> asyncAction)
	{
		bool called = false;
		Vespa_System_Action_Extension_Method.fire(asyncAction, delegate
		{
			called = true;
		});
		while (!called)
		{
			yield return null;
		}
	}

	public ITask AddFinalizer(ITask task)
	{
		return _finalSequence.AddTask(task);
	}

	public ITask AddFinalizer(Action action)
	{
		return _finalSequence.AddTask(action);
	}

	private SequenceTask GetMainSequenceTask()
	{
		if (_mainSequence == null)
		{
			_mainSequence = new SequenceTask();
		}
		return _mainSequence;
	}

	private SequenceTask GetFinalSequenceTask()
	{
		if (_finalSequence == null)
		{
			_finalSequence = new SequenceTask();
		}
		return _finalSequence;
	}

	IEnumerator<TaskCurrentData> ITask.Run()
	{
		using (new TaskFinalizer(OnEndRun))
		{
			ETaskResult taskResult = ETaskResult.Success;
			if (_mainSequence != null)
			{
				using TaskRunner runner = new TaskRunner(_debugInfo, _mainSequence);
				while (runner.MoveNextWhileRunning())
				{
					yield return runner.Current;
				}
				taskResult = runner.Current.Result;
			}
			if (_finalSequence != null)
			{
				using TaskRunner runner = new TaskRunner(_debugInfo, _finalSequence);
				while (runner.MoveNextWhileRunning())
				{
					yield return runner.Current;
				}
			}
			yield return new TaskCurrentData(taskResult);
		}
	}
}
