using NGame2.NUI.NWindow.NTutorial;
using NShared;

namespace NGame2.NTutorial;

public interface ITutorialRuntimeImmediate : ITutorialRuntime, IEventRuntime
{
	void RunImmediate(TutorialWindow window, TutorialData tutorialData);
}
