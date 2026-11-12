using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NWindow;
using NVespa;
using NVespa.NTask;
using NVespa.NTask.NRuntime;

namespace NGame2.NUI.NManager.NTask;

[IgnoreTaskStack]
public class WindowTaskFactory<T> where T : BaseManagement<T>
{
	private readonly BaseManagement<T> _management;

	private readonly Func<Type, IWindow> _windowGetter;

	public WindowTaskFactory(BaseManagement<T> management, Func<Type, IWindow> windowGetter)
	{
		_management = management;
		_windowGetter = windowGetter;
	}

	public ITask Run(Action action)
	{
		return new DelegateTask(action);
	}

	public ITask Run(IEnumerator action)
	{
		return new DelegateTask(action);
	}

	public ITask Run(IEnumerator<TaskCurrentData> action)
	{
		return new DelegateTask(action);
	}

	public ITask Run(Func<bool> conditionAction)
	{
		return new DelegateConditionTask(conditionAction);
	}

	public ITask EndTask()
	{
		return Run(() => false);
	}

	public ITask Run<TInstance>(Action<TInstance> action) where TInstance : BaseWindow
	{
		TInstance window = GetWindow<TInstance>();
		if (window == null)
		{
			VespaDebug.LogError("Failed to find window : " + typeof(TInstance).Name);
			return null;
		}
		return new DelegateTask(delegate
		{
			Vespa_System_Action_Extension_Method.fire(action, window);
		});
	}

	public ITask Run<TInstance>(Func<TInstance, ITask> action) where TInstance : BaseWindow
	{
		TInstance window = GetWindow<TInstance>();
		if (window == null)
		{
			VespaDebug.LogError("Failed to find window : " + typeof(TInstance).Name);
			return null;
		}
		return new DelegateTask(() => Vespa_System_Action_Extension_Method.fire(action, window));
	}

	public ITask Run<TInstance>(Func<TInstance, IEnumerator> action) where TInstance : BaseWindow
	{
		TInstance window = GetWindow<TInstance>();
		if (window == null)
		{
			VespaDebug.LogError("Failed to find window : " + typeof(TInstance).Name);
			return null;
		}
		return new DelegateTask(Vespa_System_Action_Extension_Method.fire(action, window));
	}

	public ITask Run<TInstance>(Func<TInstance, IEnumerator<TaskCurrentData>> action) where TInstance : BaseWindow
	{
		TInstance window = GetWindow<TInstance>();
		if (window == null)
		{
			VespaDebug.LogError("Failed to find window : " + typeof(TInstance).Name);
			return null;
		}
		return new DelegateTask(Vespa_System_Action_Extension_Method.fire(action, window));
	}

	public ITask Open<TInstance>(bool immediately = false) where TInstance : BaseWindow
	{
		Action<TInstance> initAction = null;
		return Open(initAction, immediately);
	}

	public ITask Open<TInstance>(Action<TInstance> initAction, bool immediately = false) where TInstance : BaseWindow
	{
		TInstance window = GetWindow<TInstance>();
		if (window == null)
		{
			VespaDebug.LogError("Failed to find window : " + typeof(TInstance).Name);
			return null;
		}
		return window.OpenAsync(new DelegateTask(delegate
		{
			Vespa_System_Action_Extension_Method.fire(initAction, window);
		}), immediately);
	}

	public ITask Open<TInstance>(Func<TInstance, ITask> initFunc, bool immediately = false) where TInstance : BaseWindow
	{
		TInstance window = GetWindow<TInstance>();
		if (window == null)
		{
			VespaDebug.LogError("Failed to find window : " + typeof(TInstance).Name);
			return null;
		}
		return window.OpenAsync(new DelegateTask(() => Vespa_System_Action_Extension_Method.fire(initFunc, window)), immediately);
	}

	public ITask Open<TInstance>(ITask initTask, bool immediately = false) where TInstance : BaseWindow
	{
		TInstance window = GetWindow<TInstance>();
		if (window == null)
		{
			VespaDebug.LogError("Failed to find window : " + typeof(TInstance).Name);
			return null;
		}
		return window.OpenAsync(initTask, immediately);
	}

	public ITask Close<TInstance>() where TInstance : BaseWindow
	{
		return new BranchTask().If(IsOpen<TInstance>()).Then(_management.GetInstance<TInstance>().CloseAsync());
	}

	public ITask IsOpen<TInstance>() where TInstance : BaseWindow
	{
		return new DelegateConditionTask(() => _management.IsOpened<TInstance>());
	}

	public ITask Branch(ITask condition = null, ITask onSuccess = null, ITask onFail = null, ITask onFinalize = null)
	{
		return new BranchTask(condition, onSuccess, onFail, onFinalize);
	}

	public ITask Parallel(params ITask[] tasks)
	{
		return new ParallelTask(tasks);
	}

	public ITask Sequence(string name, params ITask[] tasks)
	{
		return Vespa_Task_ExtensionMethod.NAME(Sequence(tasks), name);
	}

	public ITask Sequence(params ITask[] tasks)
	{
		SequenceTask sequenceTask = new SequenceTask();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(tasks); i++)
		{
			ITask task = tasks[i];
			if (task == null)
			{
				task.NullTaskWarning();
			}
			else
			{
				sequenceTask.AddTask(task);
			}
		}
		return sequenceTask;
	}

	public ITask Check<TInstance>(Func<TInstance, bool> action) where TInstance : BaseWindow
	{
		TInstance window = GetWindow<TInstance>();
		if (window == null)
		{
			VespaDebug.LogError("Failed to find window : " + typeof(TInstance).Name);
			return null;
		}
		return new DelegateConditionTask(() => Vespa_System_Action_Extension_Method.fire(action, window));
	}

	private TInstance GetWindow<TInstance>() where TInstance : BaseWindow
	{
		return Vespa_System_Action_Extension_Method.fire(_windowGetter, typeof(TInstance)) as TInstance;
	}
}
