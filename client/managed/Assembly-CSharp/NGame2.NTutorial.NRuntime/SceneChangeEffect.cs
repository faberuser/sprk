using System.Collections;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;

namespace NGame2.NTutorial.NRuntime;

public class SceneChangeEffect : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 0);
		GlobalTopmostWindow.ESceneChangeType changeType = EnumTools.Parse(TutorialUtility.GetArg(tutorialData, 1), GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		float argFloat = TutorialUtility.GetArgFloat(tutorialData, 2, 10f);
		if (argBool)
		{
			SceneChanger sceneChanger = SingletonUpdater<TutorialManager>.instance.PlaySceneChangeEffect(changeType, argFloat);
			if (sceneChanger != null)
			{
				while (!sceneChanger.IsCompleteAnimation())
				{
					yield return null;
				}
			}
		}
		else
		{
			SingletonUpdater<TutorialManager>.instance.StopSceneChangeEffect();
		}
		yield return null;
	}
}
