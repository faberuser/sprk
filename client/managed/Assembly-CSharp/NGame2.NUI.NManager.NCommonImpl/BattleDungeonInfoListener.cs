using System;
using NGame2.NBattleContext;
using NGame2.NDispatchBattle;
using NGame2.NUI.NWindow;

namespace NGame2.NUI.NManager.NCommonImpl;

public class BattleDungeonInfoListener : BattleDungeonInfo.Listener
{
	public EnterDungeonData EnterDungeon = new EnterDungeonData();

	public BattleDungeonInfo BattleDungeonInfo;

	public Action<EnterDungeonData> OnReadyBattle;

	public Action OnResetEventDungeon;

	public Action<EnterDungeonData> OnMoveEnter;

	public Action<EnterDungeonData> OnClickPartyPlay;

	public IDungeonRemainTimer DungeonRemainTimer;

	void BattleDungeonInfo.Listener.OnReadyBattle(EnterDungeonData enterDungeon)
	{
		EnterDungeon = enterDungeon;
		Vespa_System_Action_Extension_Method.fire(OnReadyBattle, EnterDungeon);
	}

	void BattleDungeonInfo.Listener.OnResetEventDungeon()
	{
		Vespa_System_Action_Extension_Method.fire(OnResetEventDungeon);
	}

	bool BattleDungeonInfo.Listener.OnCloseRequest(BattleDungeonInfo closeThis)
	{
		return true;
	}

	void BattleDungeonInfo.Listener.OnMoveEnter(EnterDungeonData enterDungeon)
	{
		EnterDungeon = enterDungeon;
		Vespa_System_Action_Extension_Method.fire(OnMoveEnter, EnterDungeon);
	}

	void BattleDungeonInfo.Listener.OnClickPartyPlayPopup(EnterDungeonData enterDungeon)
	{
		EnterDungeon = enterDungeon;
		Vespa_System_Action_Extension_Method.fire(OnClickPartyPlay, EnterDungeon);
	}

	void BattleDungeonInfo.Listener.OnClickDispatch(EnterDungeonData enterDungeon)
	{
		EnterDungeon = enterDungeon;
		ContentUIDispatchBattle.OpenGlobalChapter(enterDungeon);
	}

	int BattleDungeonInfo.Listener.GetDungeonRemainTime(EnterDungeonData enterDungeon)
	{
		if (enterDungeon == null)
		{
			return -1;
		}
		if (DungeonRemainTimer == null)
		{
			return -1;
		}
		int remainTime = DungeonRemainTimer.GetRemainTime(enterDungeon);
		if (remainTime > 0)
		{
			return remainTime;
		}
		if (!DungeonRemainTimer.IsShowAtRemainTimeZero())
		{
			return -1;
		}
		return 0;
	}
}
