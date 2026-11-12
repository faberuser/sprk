using System.Collections;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class ReserveLobbyPopup : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		string arg = TutorialUtility.GetArg(tutorialData, 0);
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 1, defaultValue: true);
		if (!SceneAutoBehaviour<LobbyPopupQueue>.isValidInstance)
		{
			VespaDebug.LogError("LobbyPopupQueue is not value.");
			yield break;
		}
		bool popupEnded = false;
		SceneAutoBehaviour<LobbyPopupQueue>.instance.OpenReservedPopup(arg, delegate
		{
			popupEnded = true;
		});
		bool gameUiEnabled = window.EnableGameUi;
		window.EnableGameUi = argBool;
		while (!popupEnded)
		{
			yield return null;
		}
		window.EnableGameUi = gameUiEnabled;
	}
}
