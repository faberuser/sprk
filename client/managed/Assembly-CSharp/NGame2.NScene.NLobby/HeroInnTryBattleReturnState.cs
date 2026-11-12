using System;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class HeroInnTryBattleReturnState : LobbyScene2.IReturnState
{
	public int HeroIndex;

	public string BgmCode => string.Empty;

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<HeroInnManagement>.instance.RestoreHeroDetailView(HeroIndex);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
