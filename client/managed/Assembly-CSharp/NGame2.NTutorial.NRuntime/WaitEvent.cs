using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;

namespace NGame2.NTutorial.NRuntime;

public class WaitEvent : ITutorialRuntime, IEventRuntime
{
	private string _eventName = "";

	private List<string> _argList = new List<string>();

	private bool _eventReceived;

	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		if (triggerEvent == null)
		{
			return false;
		}
		string name = triggerEvent.GetType().Name;
		if (!string.Equals(_eventName, name))
		{
			return false;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_argList) && !triggerEvent.Check(_argList))
		{
			return false;
		}
		_eventReceived = true;
		return true;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		_eventName = TutorialUtility.GetArg(tutorialData, 0);
		for (int i = 1; i <= 10; i++)
		{
			string arg = TutorialUtility.GetArg(tutorialData, i);
			if (String_Extension_Method.IsNullOrEmpty(arg))
			{
				break;
			}
			_argList.Add(arg);
		}
		while (!_eventReceived)
		{
			yield return null;
		}
	}
}
