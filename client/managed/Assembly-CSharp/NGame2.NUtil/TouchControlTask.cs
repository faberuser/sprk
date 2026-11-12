using System.Collections.Generic;
using NVespa.NTask;

namespace NGame2.NUtil;

public class TouchControlTask : ITask
{
	private readonly ITask _task;

	private readonly TaskDebugInfo _debugInfo;

	private ETouchControl _disableTouch;

	private bool _ended;

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

	public TouchControlTask(ITask task)
	{
		_task = task;
		_debugInfo = new TaskDebugInfo(this);
	}

	public TouchControlTask DisableTouch()
	{
		_disableTouch = ETouchControl.Disable;
		return this;
	}

	public TouchControlTask EnableTouch()
	{
		_disableTouch = ETouchControl.Enable;
		return this;
	}

	public TouchControlTask DisableTouch(bool disable)
	{
		if (!disable)
		{
			return EnableTouch();
		}
		return DisableTouch();
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
		using TaskRunner runner = new TaskRunner(_debugInfo, _task);
		while (runner.MoveNextWhileRunning())
		{
			TaskCurrentData current = runner.Current;
			if (current.TouchControl == ETouchControl.DontCare)
			{
				current.TouchControl = _disableTouch;
			}
			yield return current;
		}
		yield return new TaskCurrentData(runner.SuccessOrFail());
	}
}
