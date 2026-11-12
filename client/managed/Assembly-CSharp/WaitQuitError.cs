using System;
using NGame2.NScene;
using NGame2.NUI.NWindow;
using NVespa.NSingleton;
using UnityEngine;

public class WaitQuitError : CustomYieldInstruction
{
	public override bool keepWaiting => true;

	public WaitQuitError(string message)
	{
		GlobalMessagePopup.OpenGlobalError(message, GlobalAutoBehaviour<LoadingManager>.instance.Quit);
	}

	public WaitQuitError(Enum error)
	{
		GlobalMessagePopup.OpenGlobalError(error, GlobalAutoBehaviour<LoadingManager>.instance.Quit);
	}
}
