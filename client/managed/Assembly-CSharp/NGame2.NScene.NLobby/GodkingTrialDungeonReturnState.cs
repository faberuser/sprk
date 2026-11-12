using System;
using NGame2.NBattleContext;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class GodkingTrialDungeonReturnState : LobbyScene2.IReturnState
{
	private int _chapterIndex;

	private int _dungeonIndex;

	public string BgmCode => string.Empty;

	public GodkingTrialDungeonReturnState(int chapterIndex, int dungeonindex)
	{
		_chapterIndex = chapterIndex;
		_dungeonIndex = dungeonindex;
	}

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		bool lastBattleWin = SingletonAuto<BattleContext>.instance.LastBattleWin;
		if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenHeroDeck) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyDeck();
			Vespa_System_Action_Extension_Method.fire(onEnd);
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenNextDungeon) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			Vespa_System_Action_Extension_Method.fire(onEnd);
		}
		else if (info.OpenFlag == LobbyScene2.ReturnStateOpenFlag.None)
		{
			if (lastBattleWin)
			{
				SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenGodkingTrialDungeonList();
			}
			else
			{
				SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenGodkingTrialDungeonInfo(_chapterIndex);
			}
			Vespa_System_Action_Extension_Method.fire(onEnd);
		}
		else
		{
			SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenGodkingTrialDungeonList();
			Vespa_System_Action_Extension_Method.fire(onEnd);
		}
	}
}
