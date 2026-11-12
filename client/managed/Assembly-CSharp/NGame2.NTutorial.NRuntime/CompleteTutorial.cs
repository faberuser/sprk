using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.CompleteTutorial;
using NShared.NEvent;
using NVespa;

namespace NGame2.NTutorial.NRuntime;

public class CompleteTutorial : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		NShared.CompleteTutorial.Response response = null;
		yield return SingletonUpdater<TutorialManager>.instance.coRequestCompleteTutorial(delegate(NShared.CompleteTutorial.Response iResponse)
		{
			response = iResponse;
		});
		yield return SingletonUpdater<TutorialManager>.instance.coResponseCompleteTutorial(response, null);
	}
}
