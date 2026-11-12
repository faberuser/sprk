using System.Collections.Generic;

namespace NVespa.NTask.NRuntime;

[IgnoreTaskStack]
public class DecoratorTask : ITask
{
	private readonly ITask _task;

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

	public DecoratorTask(ITask task)
	{
		_task = task;
		_debugInfo = new TaskDebugInfo(this);
	}

	public override string ToString()
	{
		return _debugInfo.ToString();
	}

	public virtual bool IsValid()
	{
		return true;
	}

	IEnumerator<TaskCurrentData> ITask.Run()
	{
		if (_task == null)
		{
			yield return TaskCurrentData.SUCCESS;
			yield break;
		}
		using TaskRunner runner = new TaskRunner(_debugInfo, _task);
		while (runner.MoveNextWhileRunning())
		{
			if (!IsValid())
			{
				yield return TaskCurrentData.FAIL;
				yield break;
			}
			yield return runner.Current;
		}
		yield return new TaskCurrentData(runner.SuccessOrFail());
	}
}
