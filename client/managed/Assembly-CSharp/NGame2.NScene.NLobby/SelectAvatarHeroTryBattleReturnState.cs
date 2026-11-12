using System;
using NGame2.NUI.NManager.NLobby;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class SelectAvatarHeroTryBattleReturnState : LobbyScene2.IReturnState
{
	public string BgmCode => string.Empty;

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<LobbyManagement>.instance.RestoreHeroDetailViewForSelectArvatarHero();
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
