using System;

namespace NVespa.NTask;

public class TaskFinalizer : IDisposable
{
	public Action Finalizer { get; private set; }

	public TaskFinalizer(Action finalizer)
	{
		Finalizer = finalizer;
	}

	void IDisposable.Dispose()
	{
		Vespa_System_Action_Extension_Method.fire(Finalizer);
	}
}
