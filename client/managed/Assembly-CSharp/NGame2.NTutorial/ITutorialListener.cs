using System;
using NShared;

namespace NGame2.NTutorial;

public interface ITutorialListener
{
	void OnTutorialBegin(TutorialData tutorialData);

	void OnTutorialEnd(int tutorialIndex, Action onEnd);
}
