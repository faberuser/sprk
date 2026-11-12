using System;
using System.Collections;
using System.Collections.Generic;

namespace NVespa.NTask;

public interface ITaskContainer : ITask
{
	ITask AddTask(ITask task);

	ITask AddTask(Action action);

	ITask AddTask(IEnumerator enumerator);

	ITask AddTask(IEnumerator<TaskCurrentData> enumerator);
}
