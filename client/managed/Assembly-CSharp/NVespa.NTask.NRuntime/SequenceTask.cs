using System;
using System.Collections;
using System.Collections.Generic;

namespace NVespa.NTask.NRuntime;

[IgnoreTaskStack]
public class SequenceTask : ITaskContainer, ITask
{
	private readonly LinkedList<ITask> _taskList = new LinkedList<ITask>();

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

	public SequenceTask()
	{
		_debugInfo = new TaskDebugInfo(this);
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
		if (task == null)
		{
			task.NullTaskWarning();
			return null;
		}
		_taskList.AddLast(task);
		return task;
	}

	public ITask AddTask(Action action)
	{
		return AddTask(new DelegateTask(action));
	}

	public ITask AddTask(IEnumerator enumerator)
	{
		return AddTask(new DelegateTask(enumerator));
	}

	public ITask AddTask(IEnumerator<TaskCurrentData> enumerator)
	{
		return AddTask(new DelegateTask(enumerator));
	}

	IEnumerator<TaskCurrentData> ITask.Run()
	{
		using (new TaskFinalizer(OnEndRun))
		{
			LinkedListNode<ITask> current = _taskList.First;
			while (current != null)
			{
				ITask value = current.Value;
				current = current.Next;
				if (value == null)
				{
					value.NullTaskWarning();
					continue;
				}
				using TaskRunner runner = new TaskRunner(_debugInfo, value);
				while (runner.MoveNextWhileRunning())
				{
					yield return runner.Current;
				}
				if (runner.Current.IsFail())
				{
					yield return TaskCurrentData.FAIL;
					yield break;
				}
			}
			yield return TaskCurrentData.SUCCESS;
		}
	}
}
