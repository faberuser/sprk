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

public class SetHeroSkillExtend : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 1);
		int argInt3 = TutorialUtility.GetArgInt(tutorialData, 2);
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(argInt);
		if (cachedHeroInfo == null)
		{
			VespaDebug.LogError("SetHeroSkillExtend : Hero not found. heroIndex=" + argInt);
			return;
		}
		cachedHeroInfo.SetSkillExtend(argInt2, argInt3);
		MyHeroManagement myHeroManagement = (SceneAutoBehaviour<MyHeroManagement>.isValidInstance ? SceneAutoBehaviour<MyHeroManagement>.instance : null);
		if (myHeroManagement != null)
		{
			myHeroManagement.RefreshSkillList();
			myHeroManagement.GetInstance<HeroSkillDesc>().Refresh();
			myHeroManagement.GetInstance<HeroSkillDesc>().PlayExtendedEffect(argInt3);
		}
	}
}
