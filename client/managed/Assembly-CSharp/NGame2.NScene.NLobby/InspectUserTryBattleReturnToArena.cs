using System;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class InspectUserTryBattleReturnToArena : BaseEntryMenuReturnState
{
	public InspectUserReturnToArenaParam Param;

	public string BgmCode => string.Empty;

	public InspectUserTryBattleReturnToArena(InspectUserReturnToArenaParam param)
	{
		Param = param;
	}

	public override void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<InspectUserManagement>.instance.RestoreToArena(Param);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
