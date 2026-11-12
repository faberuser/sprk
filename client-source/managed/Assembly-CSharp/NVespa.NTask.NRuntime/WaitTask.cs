using System;
using System.Collections.Generic;

namespace NVespa.NTask.NRuntime;

[IgnoreTaskStack]
public class WaitTask : DelegateTask
{
	public WaitTask(Func<bool> action, bool waitWhile = true)
		: base(coAction(action, waitWhile))
	{
	}

	private static IEnumerator<TaskCurrentData> coAction(Func<bool> action, bool waitWhile)
	{
		while (Vespa_System_Action_Extension_Method.fire(action) == waitWhile)
		{
			yield return TaskCurrentData.RUNNING;
		}
		yield return TaskCurrentData.SUCCESS;
	}
}
