using NGame2.NBattleContext;
using NGame2.NMatch;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NVespa;

namespace NGame2.NUI.NManager;

public class DuelManagement : BaseManagement<DuelManagement>, BattlePartySetting.Listener, DuelWaitingPopup.Listener
{
	private long _duelGroupId;

	private long _friendId;

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<BattlePartySetting>("Resources/Prefab2/Window/BattlePartySetting/BattlePartySetting");
		AddInstance<DuelWaitingPopup>("Resources/Prefab2/Window/Match/DuelWaitingPopup");
	}

	private void Reset()
	{
	}

	public void CancelDuel()
	{
		CloseInstance<DuelWaitingPopup>();
		CloseInstance<BattlePartySetting>();
	}

	public void OpenDuelWaitingPopup(long playerId, long groupId, DuelWaitingPopup.EWaitingType waitingType)
	{
		_friendId = playerId;
		_duelGroupId = groupId;
		GetInstance<DuelWaitingPopup>().Open(waitingType, this);
	}

	public void CloseDuelWaitingPopup()
	{
		CloseInstance<DuelWaitingPopup>();
	}

	public void OpenBattlePartySetting(long friendId, long groupId)
	{
		_friendId = friendId;
		_duelGroupId = groupId;
		EnterDungeonData enterDungeon = new EnterDungeonData();
		DefaultBattlePartySettingDataProdiver dataProvider = new DefaultBattlePartySettingDataProdiver(GetType().Name, StaminaType.Chicken, 0, Battle.START_BATTLE, enterDungeon);
		GetInstance<BattlePartySetting>().Open(this, dataProvider);
	}

	void DuelWaitingPopup.Listener.OnCancel()
	{
		SingletonUpdater<DuelManager>.instance.CancelCurrentDuel();
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		CloseInstance<BattlePartySetting>();
		SingletonUpdater<DuelManager>.instance.CompletePartySetting(_duelGroupId, battlePartyInfo.heroIndices, battlePartyInfo.leaderHeroIndex);
	}

	bool BattlePartySetting.Listener.OnCloseRequest(BattlePartySetting caller)
	{
		SingletonUpdater<DuelManager>.instance.CancelCurrentDuel();
		return true;
	}

	void BattlePartySetting.Listener.OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
	}

	bool BattlePartySetting.Listener.OpenAutoNextBattleSelect()
	{
		return false;
	}
}
