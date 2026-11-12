using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class OpenEntryMenu : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 1);
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 2);
		bool argBool2 = TutorialUtility.GetArgBool(tutorialData, 3);
		bool isComplete = false;
		SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenuReal(argInt, argInt2, argBool, argBool2, isIgnoreTrigger: false, delegate
		{
			isComplete = true;
		});
		while (!isComplete)
		{
			yield return null;
		}
	}
}
