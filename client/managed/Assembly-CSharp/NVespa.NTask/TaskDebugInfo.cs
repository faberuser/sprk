using System.Collections.Generic;
using System.Diagnostics;

namespace NVespa.NTask;

public class TaskDebugInfo
{
	public string Name = string.Empty;

	public TaskDebugInfo(ITask task)
	{
	}

	[Conditional("UNITY_EDITOR")]
	public void AddTask(ITask task)
	{
	}

	[Conditional("UNITY_EDITOR")]
	public void AddTask(IEnumerable<ITask> tasks)
	{
	}

	[Conditional("UNITY_EDITOR")]
	public void OnBegin(ITask task, IEnumerator<TaskCurrentData> runner)
	{
	}

	[Conditional("UNITY_EDITOR")]
	public void OnEnd(ITask task)
	{
	}

	public override string ToString()
	{
		return string.Empty;
	}
}
