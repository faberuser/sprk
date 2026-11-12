using System;
using NGame2.NBattleContext;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class GuildRaidReturnState : BaseEntryMenuReturnState
{
	private EnterDungeonData _enterDungeon;

	private GuildRaidDungeonInfo _guildRaidDungeonInfo;

	public GuildRaidReturnState(EnterDungeonData enterDungeon, GuildRaidDungeonInfo info)
	{
		_enterDungeon = enterDungeon;
		_guildRaidDungeonInfo = info;
	}

	public override void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<GuildManagement2>.instance.OpenGuildRaidDungeonList(_guildRaidDungeonInfo);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
