using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class ClearBattlePartySettingHero : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		BattlePartySetting battlePartySettingWindow = GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow((BattlePartySetting e) => e.IsOpen());
		if ((bool)battlePartySettingWindow && battlePartySettingWindow.IsOpen())
		{
			while (battlePartySettingWindow.IsOpeningWindowAnimating())
			{
				yield return null;
			}
			battlePartySettingWindow.DeselectAllPartyMember();
		}
		else
		{
			VespaDebug.LogError("BattlePartySetting window not found");
		}
	}
}
