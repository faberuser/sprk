using System.Collections;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class OpenRaidLevelSelect : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime, RaidLevelSelect.Listener
{
	private TutorialData _tutorialData;

	void RaidLevelSelect.Listener.OnChangeRoomSetting(RaidLevelSelect caller, int raidLevel, RaidRoomType type, bool isViolateBanRule)
	{
	}

	void RaidLevelSelect.Listener.OnChangeSingleRoomSetting(RaidLevelSelect caller, RaidData raidData)
	{
	}

	void RaidLevelSelect.Listener.OnSelectRaidLevel(RaidLevelSelect caller, int raidIndex, int level)
	{
	}

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
		SceneAutoBehaviour<RaidManagement>.instance.GetInstance<RaidLevelSelect>().Open(this, argInt, argInt2);
	}

	void RaidLevelSelect.Listener.OnSelectRaidLevelForSingleRaid(RaidLevelSelect caller, int raidIndex, int level)
	{
	}
}
