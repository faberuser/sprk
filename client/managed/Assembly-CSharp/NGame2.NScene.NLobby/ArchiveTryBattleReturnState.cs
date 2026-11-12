using System;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class ArchiveTryBattleReturnState : LobbyScene2.IReturnState
{
	public string BgmCode => string.Empty;

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<NGame2.NUI.NManager.IllustratedBookManagement>.instance.StartRestoreHeroDetailView(onEnd);
	}
}
