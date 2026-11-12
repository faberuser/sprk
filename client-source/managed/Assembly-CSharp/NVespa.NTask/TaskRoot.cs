using System;
using System.Collections.Generic;

namespace NVespa.NTask;

public class TaskRoot
{
	public ITask Task { get; private set; }

	public IEnumerator<TaskCurrentData> Runner { get; private set; }

	public TaskCurrentData Current { get; private set; }

	public DateTime AddedTime { get; private set; }

	public DateTime EndTime { get; private set; }

	public bool Ended { get; private set; }

	public TaskRoot(ITask task)
	{
		Task = task;
		Runner = null;
		Current = TaskCurrentData.RUNNING;
		AddedTime = DateTime.UtcNow;
		Ended = false;
	}

	public void Close()
	{
		if (!Ended)
		{
			Ended = true;
			if (Runner != null)
			{
				Runner.Dispose();
				Runner = null;
			}
			EndTime = DateTime.UtcNow;
		}
	}

	public TaskCurrentData UpdateTask()
	{
		if (Task == null)
		{
			return TaskCurrentData.FAIL;
		}
		if (!Current.IsRunning())
		{
			return Current;
		}
		if (Runner == null)
		{
			Runner = Task.Run();
		}
		Runner.MoveNext();
		Current = Runner.Current;
		return Current;
	}
}
