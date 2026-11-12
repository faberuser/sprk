using System.Collections;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class MovePortalChapter : ITutorialRuntime, IEventRuntime
{
	private PortalSelect3 portalWindow;

	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		float argFloat = TutorialUtility.GetArgFloat(tutorialData, 0);
		int argInt = TutorialUtility.GetArgInt(tutorialData, 1);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 2);
		int argInt3 = TutorialUtility.GetArgInt(tutorialData, 3);
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 4);
		portalWindow = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<PortalSelect3>(openedOnly: true);
		if (portalWindow != null && portalWindow.IsOpen())
		{
			bool complete = false;
			portalWindow.MoveToPortalComponent(argInt, argFloat, argBool, argInt2, argInt3, delegate
			{
				complete = true;
			});
			while (!complete)
			{
				yield return null;
			}
		}
	}
}
