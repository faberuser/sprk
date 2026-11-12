using System;
using System.Collections.Generic;

namespace NVespa.NTask.NRuntime;

[IgnoreTaskStack]
public class DelegateConditionTask : DelegateTask
{
	public DelegateConditionTask(Func<bool> action)
		: base(coAction(action))
	{
	}

	private static IEnumerator<TaskCurrentData> coAction(Func<bool> action)
	{
		ETaskResult result = (Vespa_System_Action_Extension_Method.fire(action) ? ETaskResult.Success : ETaskResult.Fail);
		yield return new TaskCurrentData(result);
	}
}
