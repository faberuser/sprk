using System.Collections;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class EndBattleWin : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
{
	private class EndBattleWinListener : NGame2.NUI.NWindow.NBattle.EndBattleWin.Listener
	{
		public bool OnCloseRequest(NGame2.NUI.NWindow.NBattle.EndBattleWin closeThis)
		{
			return true;
		}

		public void OnInputEnabled(NGame2.NUI.NWindow.NBattle.EndBattleWin caller)
		{
		}

		public void OnClickReturnToLobby(NGame2.NUI.NWindow.NBattle.EndBattleWin caller)
		{
		}

		public void ShowStatistics()
		{
		}
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
		SceneAutoBehaviour<EndCampaign>.instance.TutorialEndBattleWin(argInt);
	}
}
