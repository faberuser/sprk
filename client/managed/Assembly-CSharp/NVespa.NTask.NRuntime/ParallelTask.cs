using System;
using System.Collections;
using System.Collections.Generic;

namespace NVespa.NTask.NRuntime;

[IgnoreTaskStack]
public class ParallelTask : ITaskContainer, ITask
{
	private readonly List<ITask> _taskList = new List<ITask>();

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

	public ParallelTask(params ITask[] tasks)
	{
		_debugInfo = new TaskDebugInfo(this);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(tasks); i++)
		{
			ITask task = tasks[i];
			if (task == null)
			{
				task.NullTaskWarning();
			}
			else
			{
				_taskList.Add(task);
			}
		}
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
		_taskList.Add(task);
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
			LinkedList<Tuple<ITask, TaskRunner>> runnerList = new LinkedList<Tuple<ITask, TaskRunner>>();
			for (int i = 0; i < _taskList.Count; i++)
			{
				ITask task = _taskList[i];
				TaskRunner i_item = new TaskRunner(_debugInfo, task);
				runnerList.AddLast(new Tuple<ITask, TaskRunner>(task, i_item));
			}
			while (true)
			{
				ETouchControl eTouchControl = ETouchControl.DontCare;
				LinkedListNode<Tuple<ITask, TaskRunner>> linkedListNode = runnerList.First;
				while (linkedListNode != null)
				{
					LinkedListNode<Tuple<ITask, TaskRunner>> node = linkedListNode;
					_ = linkedListNode.Value.Item1;
					TaskRunner item = linkedListNode.Value.Item2;
					linkedListNode = linkedListNode.Next;
					if (!item.MoveNext() || !item.Current.IsRunning())
					{
						runnerList.Remove(node);
						item.Dispose();
						continue;
					}
					TaskCurrentData current = item.Current;
					if (current.TouchControl == ETouchControl.Enable)
					{
						if (eTouchControl == ETouchControl.DontCare)
						{
							eTouchControl = ETouchControl.Enable;
						}
					}
					else if (current.TouchControl == ETouchControl.Disable)
					{
						eTouchControl = ETouchControl.Disable;
					}
				}
				if (__Extension_Method_ICollection__.COUNT(runnerList) <= 0)
				{
					break;
				}
				yield return new TaskCurrentData(ETaskResult.Running, eTouchControl);
			}
			yield return TaskCurrentData.SUCCESS;
		}
	}
}
