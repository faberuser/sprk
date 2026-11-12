using System;
using System.Collections.Generic;

namespace NVespa.NTask;

public class TaskRunner : IDisposable
{
	public TaskDebugInfo DebugInfo { get; private set; }

	public ITask RunningTask { get; private set; }

	public IEnumerator<TaskCurrentData> Runner { get; private set; }

	public TaskCurrentData Current => Runner.Current;

	public TaskRunner(TaskDebugInfo debugInfo, ITask runningTask)
	{
		DebugInfo = debugInfo;
		RunningTask = runningTask;
		Runner = RunningTask.Run();
		_ = DebugInfo;
	}

	public TaskRunner(IEnumerator<TaskCurrentData> runner)
	{
		Runner = runner;
	}

	public void Dispose()
	{
		_ = DebugInfo;
	}

	public bool MoveNext()
	{
		return Runner.MoveNext();
	}

	public bool MoveNextWhileRunning()
	{
		if (!Runner.MoveNext())
		{
			return false;
		}
		return Runner.Current.IsRunning();
	}

	public bool IsRunning()
	{
		if (Runner != null)
		{
			return Runner.Current.IsRunning();
		}
		return false;
	}

	public bool IsSuccess()
	{
		if (Runner != null)
		{
			return Runner.Current.IsSuccess();
		}
		return false;
	}

	public bool IsFail()
	{
		if (Runner != null)
		{
			return Runner.Current.IsFail();
		}
		return false;
	}

	public ETaskResult SuccessOrFail()
	{
		if (Runner == null)
		{
			return ETaskResult.Success;
		}
		if (!Runner.Current.IsFail())
		{
			return ETaskResult.Success;
		}
		return ETaskResult.Fail;
	}
}
