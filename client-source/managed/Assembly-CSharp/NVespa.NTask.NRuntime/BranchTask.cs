using System;
using System.Collections.Generic;

namespace NVespa.NTask.NRuntime;

[IgnoreTaskStack]
public class BranchTask : ITask
{
	private readonly TaskDebugInfo _debugInfo;

	public ITask Condition { get; private set; }

	public ITask OnSuccess { get; private set; }

	public ITask OnFail { get; private set; }

	public ITask OnFinalize { get; private set; }

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

	public BranchTask()
	{
		_debugInfo = new TaskDebugInfo(this);
	}

	public BranchTask(ITask condition, ITask onSuccess, ITask onFail, ITask onFinalize)
	{
		_debugInfo = new TaskDebugInfo(this);
		ConditionTask(condition);
		OnSuccessTask(onSuccess);
		OnFailTask(onFail);
		OnFinalizeTask(onFinalize);
	}

	public override string ToString()
	{
		return _debugInfo.ToString();
	}

	public BranchTask If(Func<bool> func)
	{
		return ConditionTask(func);
	}

	public BranchTask If(ITask condition)
	{
		return ConditionTask(condition);
	}

	public BranchTask ConditionTask(Func<bool> func)
	{
		return ConditionTask(new DelegateConditionTask(func));
	}

	public BranchTask ConditionTask(ITask condition)
	{
		Condition = condition;
		return this;
	}

	public BranchTask Then(Action action)
	{
		return OnSuccessTask(action);
	}

	public BranchTask Then(Func<bool> func)
	{
		return OnSuccessTask(func);
	}

	public BranchTask Then(ITask onSuccess)
	{
		return OnSuccessTask(onSuccess);
	}

	public BranchTask OnSuccessTask(Action action)
	{
		return OnSuccessTask(new DelegateTask(action));
	}

	public BranchTask OnSuccessTask(Func<bool> func)
	{
		return OnSuccessTask(new DelegateConditionTask(func));
	}

	public BranchTask OnSuccessTask(ITask onSuccess)
	{
		OnSuccess = onSuccess;
		return this;
	}

	public BranchTask Else(Action action)
	{
		return OnFailTask(action);
	}

	public BranchTask Else(Func<bool> func)
	{
		return OnFailTask(func);
	}

	public BranchTask Else(ITask onSuccess)
	{
		return OnFailTask(onSuccess);
	}

	public BranchTask OnFailTask(Action action)
	{
		return OnFailTask(new DelegateTask(action));
	}

	public BranchTask OnFailTask(Func<bool> func)
	{
		return OnFailTask(new DelegateConditionTask(func));
	}

	public BranchTask OnFailTask(ITask onFail)
	{
		OnFail = onFail;
		return this;
	}

	public BranchTask OnFinalizeTask(Action action)
	{
		return OnFinalizeTask(new DelegateTask(action));
	}

	public BranchTask OnFinalizeTask(Func<bool> func)
	{
		return OnFinalizeTask(new DelegateConditionTask(func));
	}

	public BranchTask OnFinalizeTask(ITask onFinalize)
	{
		OnFinalize = onFinalize;
		return this;
	}

	private void OnEndRun()
	{
	}

	IEnumerator<TaskCurrentData> ITask.Run()
	{
		using (new TaskFinalizer(OnEndRun))
		{
			ETaskResult taskResult = ETaskResult.Success;
			if (Condition != null)
			{
				bool num;
				using (TaskRunner runner = new TaskRunner(_debugInfo, Condition))
				{
					while (runner.MoveNextWhileRunning())
					{
						yield return runner.Current;
					}
					num = runner.IsSuccess();
				}
				if (num)
				{
					if (OnSuccess != null)
					{
						using TaskRunner runner = new TaskRunner(_debugInfo, OnSuccess);
						while (runner.MoveNextWhileRunning())
						{
							yield return runner.Current;
						}
						taskResult = runner.SuccessOrFail();
					}
				}
				else if (OnFail != null)
				{
					using TaskRunner runner = new TaskRunner(_debugInfo, OnFail);
					while (runner.MoveNextWhileRunning())
					{
						yield return runner.Current;
					}
					taskResult = runner.SuccessOrFail();
				}
			}
			if (OnFinalize != null)
			{
				using TaskRunner runner = new TaskRunner(_debugInfo, OnFinalize);
				while (runner.MoveNextWhileRunning())
				{
					yield return runner.Current;
				}
			}
			yield return new TaskCurrentData(taskResult);
		}
	}
}
