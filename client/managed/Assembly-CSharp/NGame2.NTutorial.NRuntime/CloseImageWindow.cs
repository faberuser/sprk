using System.Collections;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class CloseImageWindow : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		yield return null;
	}

	void ITutorialRuntimeImmediate.RunImmediate(TutorialWindow window, TutorialData tutorialData)
	{
		TutorialImageWindow tutorialImageWindow = null;
		if (SceneAutoBehaviour<BattleManagement>.isValidInstance)
		{
			SceneAutoBehaviour<BattleManagement>.instance.AddInstance<TutorialImageWindow>("Resources/Prefab2/Window/Tutorial/TutorialImageWindow");
			tutorialImageWindow = SceneAutoBehaviour<BattleManagement>.instance.GetInstance<TutorialImageWindow>();
		}
		else if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			SceneAutoBehaviour<LobbyManagement>.instance.AddInstance<TutorialImageWindow>("Resources/Prefab2/Window/Tutorial/TutorialImageWindow");
			tutorialImageWindow = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<TutorialImageWindow>();
		}
		if (tutorialImageWindow != null && tutorialImageWindow.IsOpen())
		{
			tutorialImageWindow.Close();
		}
	}
}
