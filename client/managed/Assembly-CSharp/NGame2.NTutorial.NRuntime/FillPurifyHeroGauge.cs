using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class FillPurifyHeroGauge : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		int after = TutorialUtility.GetArgInt(tutorialData, 1);
		int maxPurified = PurifyHeroRoom.GetMaxPurified(1, 0);
		PurifyHeroRoom purifyHeroRoomWindow = SceneAutoBehaviour<HeroTrialDungeonManagement>.instance.GetInstance<PurifyHeroRoom>();
		bool played = false;
		purifyHeroRoomWindow.PlayPurifyHeroAnim(argInt, after, delegate
		{
			played = true;
		});
		while (!played)
		{
			yield return null;
		}
		purifyHeroRoomWindow.SetupPurifyButtons(after == maxPurified);
	}
}
