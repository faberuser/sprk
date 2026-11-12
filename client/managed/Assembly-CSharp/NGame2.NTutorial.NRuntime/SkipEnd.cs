using System.Collections;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class SkipEnd : ITutorialRuntime, IEventRuntime, ICustomTutorialSkippable
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		window.ClearSkip();
		yield break;
	}

	IEnumerator ICustomTutorialSkippable.Skip(TutorialWindow window, TutorialData tutorialData)
	{
		string arg = TutorialUtility.GetArg(tutorialData, 0);
		string arg2 = TutorialUtility.GetArg(tutorialData, 1);
		string arg3 = TutorialUtility.GetArg(tutorialData, 2);
		string waitPrefabName = "GuideSelect";
		if (!String_Extension_Method.IsNullOrEmpty(arg))
		{
			bool popupClosed = false;
			string stringDirect = LocalizationManager.GetStringDirect(arg2);
			string stringDirect2 = LocalizationManager.GetStringDirect(arg3);
			TutorialSkipPopup.Open(arg, delegate
			{
				popupClosed = true;
			}, stringDirect, stringDirect2);
			bool gameUiEnabled = window.EnableGameUi;
			window.EnableGameUi = true;
			while (!popupClosed)
			{
				yield return null;
			}
			if (!String_Extension_Method.IsNullOrEmpty(waitPrefabName) && SceneAutoBehaviour<LobbyManagement>.isValidInstance)
			{
				GuideSelect guideSelect = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<GuideSelect>();
				while (guideSelect != null && guideSelect.IsOpen())
				{
					yield return null;
				}
			}
			if (window == null)
			{
				yield break;
			}
			window.EnableGameUi = gameUiEnabled;
		}
		if (!(window == null))
		{
			window.ClearSkip();
		}
	}
}
