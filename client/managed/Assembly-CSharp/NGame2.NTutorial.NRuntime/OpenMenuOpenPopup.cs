using System.Collections;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class OpenMenuOpenPopup : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		BottomButtonData byTutorialIndex = GameRoot.Instance.BottomButtonDataContainer.GetByTutorialIndex(tutorialData.Index);
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 1);
		int endX = TutorialUtility.GetArgInt(tutorialData, 2);
		int endY = TutorialUtility.GetArgInt(tutorialData, 3);
		float durationFly = TutorialUtility.GetArgFloat(tutorialData, 4);
		float durationEffect = TutorialUtility.GetArgFloat(tutorialData, 5);
		if (byTutorialIndex == null || !SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			return;
		}
		LobbyRightMenu rightMenu = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyRightMenu>();
		ButtonAnimateWindow buttonWindow = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<ButtonAnimateWindow>();
		MenuOpenWindow instance = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<MenuOpenWindow>();
		if (rightMenu != null && buttonWindow != null && instance != null)
		{
			rightMenu.SetFlyButton(byTutorialIndex.Type);
			instance.Open(byTutorialIndex.Type, delegate
			{
				rightMenu.StartButtonFlyAnimation(durationFly, durationEffect);
				buttonWindow.StartButtonFlyAnimation(new Vector3(endX, endY, 0f), durationFly, durationEffect);
			});
			buttonWindow.Init(byTutorialIndex.Type, new Vector3(argInt, argInt2, 0f));
		}
	}
}
