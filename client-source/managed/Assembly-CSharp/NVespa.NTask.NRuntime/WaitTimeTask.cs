using System;
using System.Collections.Generic;

namespace NVespa.NTask.NRuntime;

[IgnoreTaskStack]
public class WaitTimeTask : DelegateTask
{
	public WaitTimeTask(float second)
		: base(coAction(second))
	{
	}

	private static IEnumerator<TaskCurrentData> coAction(float second)
	{
		DateTime now = DateTime.Now;
		while ((DateTime.Now - now).TotalSeconds < (double)second)
		{
			yield return new TaskCurrentData(ETaskResult.Running);
		}
		yield return new TaskCurrentData(ETaskResult.Success);
	}
}
