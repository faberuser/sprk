using System;
using System.Collections.Generic;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NGlobalTopmost;

namespace NVespa.NTask;

public class TaskManager : SingletonUpdater<TaskManager>
{
	private class TouchDisabler : ITouchDisabler
	{
		public bool IsValid => true;
	}

	private bool _disableTouch;

	private TouchDisabler _touchDisabler = new TouchDisabler();

	private bool _updating;

	public IncreaseDic<Type, List<TaskRoot>> TaskDic = new IncreaseDic<Type, List<TaskRoot>>();

	public static void OnSceneChanged()
	{
		if (SingletonUpdater<TaskManager>.isValidInstance && SingletonUpdater<TaskManager>.instance.TaskDic.IsNullOrEmpty())
		{
			if (SingletonUpdater<TaskManager>.instance._disableTouch)
			{
				GlobalTopmostWindow.instance.RemoveTouchDisabler(SingletonUpdater<TaskManager>.instance._touchDisabler);
			}
			SingletonUpdater<TaskManager>.instance.TaskDic.Clear();
			SingletonUpdater<TaskManager>.ReleaseSingleton();
		}
	}

	public void CancelAllTask()
	{
		for (int i = 0; i < TaskDic.Count; i++)
		{
			List<TaskRoot> valueByIndex = TaskDic.GetValueByIndex(i);
			for (int j = 0; j < valueByIndex.Count; j++)
			{
				TaskRoot taskRoot = valueByIndex[j];
				if (taskRoot == null || taskRoot.Task == null || taskRoot.Ended)
				{
					valueByIndex.RemoveAt(j);
					j--;
				}
				else
				{
					EndTask(taskRoot);
				}
			}
			valueByIndex.Clear();
		}
		TaskDic.Clear();
	}

	public void AddTask(ITask task)
	{
		List<TaskRoot> taskList = GetTaskList(GetType(), createIfNot: true);
		AddTask(taskList, task);
	}

	public void AddTask(Type callerType, ITask task)
	{
		if (callerType == null)
		{
			callerType = GetType();
		}
		List<TaskRoot> taskList = GetTaskList(callerType, createIfNot: true);
		AddTask(taskList, task);
	}

	public void AddTask<T>(ITask task)
	{
		AddTask(typeof(T), task);
	}

	private List<TaskRoot> GetTaskList(Type type, bool createIfNot)
	{
		List<TaskRoot> vALUE = TaskDic.GETVALUE(type, null);
		if (vALUE != null)
		{
			return vALUE;
		}
		if (!createIfNot)
		{
			return null;
		}
		vALUE = new List<TaskRoot>();
		TaskDic.Add(type, vALUE);
		return vALUE;
	}

	private void AddTask(List<TaskRoot> taskAddList, ITask task)
	{
		GlobalTopmostWindow.instance.SetOneTimeDisable();
		AddTaskInternal(taskAddList, task);
		if (_updating)
		{
			return;
		}
		bool disableTouch = false;
		for (int i = 0; i < TaskDic.Count; i++)
		{
			List<TaskRoot> valueByIndex = TaskDic.GetValueByIndex(i);
			for (int j = 0; j < valueByIndex.Count; j++)
			{
				TaskRoot taskRoot = valueByIndex[j];
				if (taskRoot != null && taskRoot.Task != null && !taskRoot.Ended && taskRoot.Current.IsRunning())
				{
					if (taskRoot.Current.TouchControl == ETouchControl.Disable)
					{
						disableTouch = true;
					}
					break;
				}
			}
		}
		UpdateTouchControl(disableTouch);
	}

	private void AddTaskInternal(List<TaskRoot> taskList, ITask task)
	{
		if (task == null)
		{
			task.NullTaskWarning();
			return;
		}
		TaskRoot taskRoot = new TaskRoot(task);
		if (taskList.Count == 0 && !taskRoot.UpdateTask().IsRunning())
		{
			EndTask(taskRoot);
		}
		else
		{
			taskList.Add(taskRoot);
		}
	}

	private void EndTask(TaskRoot taskRoot)
	{
		if (!taskRoot.Ended)
		{
			taskRoot.Close();
		}
	}

	public bool RemoveTask(ITask task)
	{
		if (task == null)
		{
			task.NullTaskWarning();
			return false;
		}
		for (int i = 0; i < TaskDic.Count; i++)
		{
			List<TaskRoot> valueByIndex = TaskDic.GetValueByIndex(i);
			for (int j = 0; j < valueByIndex.Count; j++)
			{
				TaskRoot taskRoot = valueByIndex[j];
				if (taskRoot != null && taskRoot.Task == task)
				{
					valueByIndex.RemoveAt(j);
					EndTask(taskRoot);
					return true;
				}
			}
		}
		return false;
	}

	public void ForEachTask(Action<ITask> action)
	{
		for (int i = 0; i < TaskDic.Count; i++)
		{
			List<TaskRoot> valueByIndex = TaskDic.GetValueByIndex(i);
			for (int j = 0; j < valueByIndex.Count; j++)
			{
				TaskRoot taskRoot = valueByIndex[j];
				Vespa_System_Action_Extension_Method.fire(action, taskRoot.Task);
			}
		}
	}

	public override void ChainUpdate()
	{
		using (new DisposableAction(delegate
		{
			_updating = false;
		}))
		{
			_updating = true;
			ChainUpdateInternal();
		}
	}

	private void ChainUpdateInternal()
	{
		bool disableTouch = false;
		for (int i = 0; i < TaskDic.Count; i++)
		{
			List<TaskRoot> valueByIndex = TaskDic.GetValueByIndex(i);
			for (int j = 0; j < valueByIndex.Count; j++)
			{
				TaskRoot taskRoot = valueByIndex[j];
				if (taskRoot == null || taskRoot.Task == null || taskRoot.Ended)
				{
					valueByIndex.RemoveAt(j);
					j--;
					continue;
				}
				try
				{
					TaskCurrentData taskCurrentData = taskRoot.UpdateTask();
					if (!taskCurrentData.IsRunning())
					{
						valueByIndex.RemoveAt(j);
						j--;
						EndTask(taskRoot);
						continue;
					}
					if (taskCurrentData.TouchControl == ETouchControl.Disable)
					{
						disableTouch = true;
					}
				}
				catch (Exception obj)
				{
					VespaDebug.LogError(obj);
					valueByIndex.RemoveAt(j);
					j--;
					EndTask(taskRoot);
					continue;
				}
				break;
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(valueByIndex))
			{
				TaskDic.RemoveByIndex(i);
				i--;
			}
		}
		UpdateTouchControl(disableTouch);
	}

	private void UpdateTouchControl(bool disableTouch)
	{
		if (_disableTouch != disableTouch)
		{
			_disableTouch = disableTouch;
			if (_disableTouch)
			{
				GlobalTopmostWindow.instance.AddTouchDisabler(_touchDisabler);
			}
			else
			{
				GlobalTopmostWindow.instance.RemoveTouchDisabler(_touchDisabler);
			}
		}
	}
}
