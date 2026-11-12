using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class OpenPurifyHeroSelect : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		Vespa_Common_Class_Extension_Method.Interface((ITutorialRuntimeImmediate)this).RunImmediate(window, tutorialData);
		yield break;
	}

	void ITutorialRuntimeImmediate.RunImmediate(TutorialWindow window, TutorialData tutorialData)
	{
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(argInt);
		if (heroInfo == null)
		{
			VespaDebug.LogError("Hero info not found. heroIndex=" + argInt);
			return;
		}
		PurifyHeroSelect purifyHeroSelectWindow = SceneAutoBehaviour<HeroTrialDungeonManagement>.instance.GetInstance<PurifyHeroSelect>();
		purifyHeroSelectWindow.Open(delegate
		{
			purifyHeroSelectWindow.SetupTutorialLayout();
			purifyHeroSelectWindow.SetupPurifiableHeroesList(new CachedHeroInfo[1] { heroInfo });
		});
	}
}
