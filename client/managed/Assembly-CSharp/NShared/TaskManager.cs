using System;
using System.Collections.Generic;
using NVespa;

namespace NShared;

public class TaskManager
{
	private object _taskListSync = new object();

	private readonly List<Action> _taskList = new List<Action>();

	public void Invoke(Action action)
	{
		lock (_taskListSync)
		{
			_taskList.Add(action);
		}
	}

	public void Flush()
	{
		Action[] array = null;
		lock (_taskListSync)
		{
			array = _taskList.ToArray();
			_taskList.Clear();
		}
		Action[] array2 = array;
		foreach (Action action in array2)
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				VespaDebug.LogError("Task error : " + ex);
			}
		}
	}
}
