using System.Collections;
using NGame2.NParty;
using NGame2.NScene;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class EnterLobby : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby);
		if (GlobalAutoBehaviour<PartyManager>.isValidInstance)
		{
			GlobalAutoBehaviour<PartyManager>.instance.ResetParty();
		}
		yield break;
	}
}
