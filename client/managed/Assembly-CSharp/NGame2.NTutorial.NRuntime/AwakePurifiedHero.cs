using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class AwakePurifiedHero : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		PurifyHeroRoom instance = SceneAutoBehaviour<HeroTrialDungeonManagement>.instance.GetInstance<PurifyHeroRoom>();
		bool awaken = false;
		instance.PlayAwakeHeroAnim(null, delegate
		{
			awaken = true;
		}, GetNewHero2.EOpenMode.Awaken, showNewHeroPopup: false);
		while (!awaken)
		{
			yield return null;
		}
	}
}
