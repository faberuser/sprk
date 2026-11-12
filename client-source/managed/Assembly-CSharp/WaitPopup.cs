using System;
using NGame2.NUI.NWindow;
using UnityEngine;

public class WaitPopup : CustomYieldInstruction
{
	private bool isWaiting = true;

	public override bool keepWaiting => isWaiting;

	public WaitPopup(string title, string message)
	{
		GlobalMessagePopup.OpenGlobal(title, message, delegate
		{
			isWaiting = false;
		}, retryButton: false);
	}

	public WaitPopup(string title, string message, Action complete)
	{
		WaitPopup waitPopup = this;
		GlobalMessagePopup.OpenGlobal(title, message, delegate
		{
			Vespa_System_Action_Extension_Method.fire(complete);
			waitPopup.isWaiting = false;
		}, retryButton: false);
	}
}
