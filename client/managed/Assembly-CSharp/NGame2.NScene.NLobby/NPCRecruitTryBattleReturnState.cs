using System;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class NPCRecruitTryBattleReturnState : LobbyScene2.IReturnState
{
	public string BgmCode => string.Empty;

	public NpcRecruitType RecruitType { get; set; }

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<NPCRecruitManagement>.instance.RestoreHeroDetailView(RecruitType);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
