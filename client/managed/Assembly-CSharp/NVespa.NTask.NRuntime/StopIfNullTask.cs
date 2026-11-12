using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NTask.NRuntime;

public class StopIfNullTask : ITask
{
	private Object _obj;

	private ITask _task;

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

	public StopIfNullTask(Object obj, ITask task)
	{
		_obj = obj;
		_task = task;
		_debugInfo = new TaskDebugInfo(this);
	}

	public override string ToString()
	{
		return _debugInfo.ToString();
	}

	IEnumerator<TaskCurrentData> ITask.Run()
	{
		if (_task == null)
		{
			yield return TaskCurrentData.SUCCESS;
			yield break;
		}
		if (_obj == null)
		{
			yield return TaskCurrentData.FAIL;
			yield break;
		}
		using TaskRunner runner = new TaskRunner(_debugInfo, _task);
		while (runner.MoveNextWhileRunning())
		{
			yield return runner.Current;
			if (_obj == null)
			{
				yield return TaskCurrentData.FAIL;
				yield break;
			}
		}
		yield return new TaskCurrentData(runner.SuccessOrFail());
	}
}
