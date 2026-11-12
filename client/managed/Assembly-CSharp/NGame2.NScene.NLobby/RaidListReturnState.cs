using System;
using NGame2.NUI.NManager.NLobby;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class RaidListReturnState : LobbyScene2.IReturnState
{
	public string BgmCode => string.Empty;

	void LobbyScene2.IReturnState.Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<RaidManagement>.instance.OpenRaidSelect();
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
