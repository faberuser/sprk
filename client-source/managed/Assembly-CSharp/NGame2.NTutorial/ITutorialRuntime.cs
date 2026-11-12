using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;

namespace NGame2.NTutorial;

public interface ITutorialRuntime : IEventRuntime
{
	bool OnEvent(IEvent triggerEvent);

	IEnumerator Run(TutorialWindow window, TutorialData tutorialData);
}
