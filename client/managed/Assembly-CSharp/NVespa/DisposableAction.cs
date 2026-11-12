using System;

namespace NVespa;

public class DisposableAction : IDisposable
{
	private Action _onDispose;

	public DisposableAction(Action onDispose)
	{
		_onDispose = onDispose;
	}

	public virtual void Dispose()
	{
		Vespa_System_Action_Extension_Method.fire(_onDispose);
		_onDispose = null;
	}
}
