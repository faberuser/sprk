using System;

namespace NGame2.NUI;

public class ReserveConditionalPopup : IReservedAction
{
	private bool _isFailedOpenCodition;

	public Action OpenAction;

	public Func<bool> OpenCondition;

	public Func<bool> EndCondition;

	void IReservedAction.Execute()
	{
		if (OpenCondition())
		{
			Vespa_System_Action_Extension_Method.fire(OpenAction);
		}
		else
		{
			_isFailedOpenCodition = true;
		}
	}

	bool IReservedAction.Ended()
	{
		if (_isFailedOpenCodition)
		{
			return true;
		}
		if (EndCondition == null)
		{
			return true;
		}
		return EndCondition();
	}
}
