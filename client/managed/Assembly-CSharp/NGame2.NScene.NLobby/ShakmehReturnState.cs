using System;
using NGame2.NBattleContext;
using NGame2.NUI.NManager.NLobby;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class ShakmehReturnState : LobbyScene2.IReturnState
{
	private EnterDungeonData _enterDungeon;

	public string BgmCode => string.Empty;

	public ShakmehReturnState(EnterDungeonData enterDungeon)
	{
		_enterDungeon = enterDungeon;
	}

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<LobbyManagement>.instance.ExecuteCurrentNode(delegate
		{
			Vespa_System_Action_Extension_Method.fire(onEnd);
		});
	}
}
