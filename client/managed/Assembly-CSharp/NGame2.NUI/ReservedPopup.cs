using System;

namespace NGame2.NUI;

public class ReservedPopup : IReservedAction
{
	public IWindow Window;

	public Action OpenAction;

	void IReservedAction.Execute()
	{
		Vespa_System_Action_Extension_Method.fire(OpenAction);
	}

	bool IReservedAction.Ended()
	{
		if (Window != null)
		{
			return !Window.IsOpen();
		}
		return true;
	}
}
