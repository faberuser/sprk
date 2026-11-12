using System;
using NGame2.NUI.NWindow;
using UnityEngine;

public class WaitErrorPopup : CustomYieldInstruction
{
	private bool isWaiting = true;

	public override bool keepWaiting => isWaiting;

	public WaitErrorPopup(string message)
	{
		GlobalMessagePopup.OpenGlobalError(message, delegate
		{
			isWaiting = false;
		});
	}

	public WaitErrorPopup(Enum error)
	{
		GlobalMessagePopup.OpenGlobalError(error, delegate
		{
			isWaiting = false;
		});
	}

	public WaitErrorPopup(string message, Action complete)
	{
		WaitErrorPopup waitErrorPopup = this;
		GlobalMessagePopup.OpenGlobalError(message, delegate
		{
			Vespa_System_Action_Extension_Method.fire(complete);
			waitErrorPopup.isWaiting = false;
		});
	}
}
