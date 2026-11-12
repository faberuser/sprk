using System;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class InspectUserTryBattleReturnToFriend : LobbyScene2.IReturnState
{
	public InspectUserReturnToFriendParam Param;

	public string BgmCode => string.Empty;

	public InspectUserTryBattleReturnToFriend(InspectUserReturnToFriendParam param)
	{
		Param = param;
	}

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<InspectUserManagement>.instance.RestoreToFriend(Param);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
