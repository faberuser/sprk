using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

public static class Vespa_Task_Runtime_StopIfNullTask_ExtensionMethod
{
	public static StopIfNullTask WrapperStopIfNull(this ITask This, Object obj)
	{
		return new StopIfNullTask(obj, This);
	}
}
