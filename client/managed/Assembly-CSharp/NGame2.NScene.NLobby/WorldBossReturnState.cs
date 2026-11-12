using System;
using NGame2.NBattleContext;
using NGame2.NUI.NManager.NLobby;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class WorldBossReturnState : LobbyScene2.IReturnState
{
	private EnterDungeonData _enterDungeon;

	string LobbyScene2.IReturnState.BgmCode => null;

	public WorldBossReturnState(EnterDungeonData enterDungeon)
	{
		_enterDungeon = enterDungeon;
	}

	void LobbyScene2.IReturnState.Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<WorldBossManagement>.instance.OpenWorldBossRanking(delegate
		{
			if (!WorldBossManagement.WorldBossKilledByMe)
			{
				Vespa_System_Action_Extension_Method.fire(onEnd);
			}
			else
			{
				SceneAutoBehaviour<WorldBossManagement>.instance.OpenWorldBossKillRewardPopup();
				WorldBossManagement.WorldBossKilledByMe = false;
				WorldBossManagement.WorldBossKilledRewardInfo = null;
				Vespa_System_Action_Extension_Method.fire(onEnd);
			}
		});
	}
}
