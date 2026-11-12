using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;

namespace NGame2.NTutorial.NRuntime;

public class OpenHeroWindow : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		Hero3DViewPanel.EToggle argEnum = TutorialUtility.GetArgEnum<Hero3DViewPanel.EToggle>(tutorialData, 1);
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(argInt);
		if (cachedHeroInfo == null)
		{
			VespaDebug.LogError("OpenHeroWindow : Hero not found. heroIndex=" + argInt);
			yield break;
		}
		if (!SceneAutoBehaviour<MyHeroManagement>.isValidInstance)
		{
			VespaDebug.LogError("OpenHeroWindow : MyHeroManagement is not found");
			yield break;
		}
		bool completeOpen = false;
		Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask().ADDTASK(SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHeroAsync(cachedHeroInfo, argEnum)), delegate
		{
			completeOpen = true;
		}).WrapperTouchControl().DisableTouch()
			.Commit();
		while (!completeOpen)
		{
			yield return null;
		}
	}
}
