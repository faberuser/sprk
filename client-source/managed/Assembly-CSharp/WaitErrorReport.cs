using System;
using NGame2.NUI.NWindow;
using UnityEngine;

public class WaitErrorReport : CustomYieldInstruction
{
	private bool isWaiting = true;

	public override bool keepWaiting => isWaiting;

	public WaitErrorReport(Enum error, string infomation, Action complete)
	{
		WaitErrorReport waitErrorReport = this;
		GlobalMessagePopup.OpenGlobalError(error, delegate
		{
			Vespa_System_Action_Extension_Method.fire(complete);
			waitErrorReport.isWaiting = false;
		});
	}

	public WaitErrorReport(string error, string infomation, Action complete)
	{
		WaitErrorReport waitErrorReport = this;
		GlobalMessagePopup.OpenGlobalError(error, delegate
		{
			Vespa_System_Action_Extension_Method.fire(complete);
			waitErrorReport.isWaiting = false;
		});
	}
}
