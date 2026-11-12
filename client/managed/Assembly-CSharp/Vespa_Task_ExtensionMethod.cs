using System;
using System.Collections;
using System.Collections.Generic;
using NVespa;
using NVespa.NTask;
using NVespa.NTask.NRuntime;

public static class Vespa_Task_ExtensionMethod
{
	public static void Commit(this ITask This)
	{
		if (This != null)
		{
			SingletonUpdater<TaskManager>.instance.AddTask(This);
		}
	}

	public static void Commit(this ITask This, Type callerType)
	{
		if (This != null)
		{
			SingletonUpdater<TaskManager>.instance.AddTask(callerType, This);
		}
	}

	public static T ENDTASK<T>(this T This) where T : ITaskContainer
	{
		This?.AddTask(new DelegateConditionTask(() => false));
		return This;
	}

	public static T ADDTASK<T>(this T This, ITask task) where T : ITaskContainer
	{
		This?.AddTask(task);
		return This;
	}

	public static T ADDTASK<T>(this T This, Action action) where T : ITaskContainer
	{
		This?.AddTask(action);
		return This;
	}

	public static T ADDCONDITIONTASK<T>(this T This, Func<bool> func) where T : ITaskContainer
	{
		This?.AddTask(new DelegateConditionTask(func));
		return This;
	}

	public static T ADDTASK<T>(this T This, IEnumerator enumerator) where T : ITaskContainer
	{
		This?.AddTask(enumerator);
		return This;
	}

	public static T ADDTASK<T>(this T This, IEnumerator<TaskCurrentData> enumerator) where T : ITaskContainer
	{
		This?.AddTask(enumerator);
		return This;
	}

	public static T NAME<T>(this T This, string name) where T : ITask
	{
		if (This != null)
		{
			This.Name = name;
		}
		return This;
	}

	public static void NullTaskWarning(this ITask This)
	{
	}
}
