using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;

namespace NGame2.NTutorial;

public interface ICustomTutorialSkippable
{
	IEnumerator Skip(TutorialWindow window, TutorialData tutorialData);
}
