using System;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class AchivementTryBattleReturnState : LobbyScene2.IReturnState
{
	public string BgmCode => string.Empty;

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<AchievementManagement>.instance.RestoreHeroDetailView();
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
