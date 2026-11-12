using System;
using NGame2.NUI.NManager.NLobby;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class EclipseReturnState : LobbyScene2.IReturnState
{
	public string BgmCode => "";

	void LobbyScene2.IReturnState.Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<SoulWeaponManagement>.instance.EclipseReturnState(onEnd);
	}
}
