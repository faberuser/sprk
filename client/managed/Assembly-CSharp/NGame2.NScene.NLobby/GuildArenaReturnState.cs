using System;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class GuildArenaReturnState : LobbyScene2.IReturnState
{
	public string BgmCode => "BGM2.Guild.GuildEntryMenu";

	void LobbyScene2.IReturnState.Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<GuildArenaManagement>.instance.GuildArenaReturnStateFunction();
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
