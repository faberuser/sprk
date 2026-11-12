using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NParty;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NChatting;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.NParty;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class OpenSuppressPartySetting : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime, BattlePartySetting.IDataProvider, RaidPartySetting.Listener, BattlePartySetting.Listener
{
	private EnterDungeonData _enterDungeon = new EnterDungeonData(1, 1, 0);

	private int[] _heroIndices;

	private RaidPartySetting raidWindow;

	private RaidData _raidData;

	private TutorialData _tutorialData;

	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		Vespa_Common_Class_Extension_Method.Interface((ITutorialRuntimeImmediate)this).RunImmediate(window, tutorialData);
		yield break;
	}

	void ITutorialRuntimeImmediate.RunImmediate(TutorialWindow window, TutorialData tutorialData)
	{
		_tutorialData = tutorialData;
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 1);
		RaidRoomInfo roomInfo = new RaidRoomInfo
		{
			RoomNo = 0L,
			RaidLevel = argInt2,
			RaidIndex = argInt,
			GoalIndex = 0,
			MasterNick = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick,
			MasterAccountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId,
			CurPartyMember = 1,
			Opened = 0
		};
		GlobalAutoBehaviour<PartyManager>.instance.SetRaidPartyInfo(argInt, argInt2, roomInfo);
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(argInt, argInt2);
		if (data == null)
		{
			VespaDebug.LogError("RaidData not found : raid=" + data);
			return;
		}
		int chapterIndex = data.ChapterIndex;
		int dungeonIndex = data.DungeonIndex;
		CampaignDungeonData data2 = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data2 == null)
		{
			VespaDebug.LogError("Raid DungeonData found : raid=" + data?.ToString() + " chapter=" + chapterIndex + " dungeon=" + dungeonIndex);
			return;
		}
		int staminaOfDifficulty = data2.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina());
		if (staminaOfDifficulty <= 0)
		{
			VespaDebug.LogError("Raid chicken is 0 : raid=" + data?.ToString() + " chapter=" + chapterIndex + " dungeon=" + dungeonIndex);
			return;
		}
		_raidData = data;
		UIHelper.CloseWindowsBeforeRaidPartySetting();
		EnterDungeonData enterDungeon = new EnterDungeonData(data.ChapterIndex, data.DungeonIndex, data.DungeonIndex, NShared.DungeonDifficultyType.Easy);
		DefaultBattlePartySettingDataProdiver dataProvider = new DefaultBattlePartySettingDataProdiver(null, StaminaType.GuildSuppressKey, staminaOfDifficulty, Battle.START_BATTLE, enterDungeon);
		raidWindow = SceneAutoBehaviour<RaidManagement>.instance.GetInstance<GuildSuppressPartySetting>();
		raidWindow.Open();
		raidWindow.SetTutorialWindow();
		raidWindow.UserStat = UserManager.UserStatType.Gold;
		raidWindow.InitForRaid(this, dataProvider, data);
		raidWindow.SetStaminaCount(0);
		raidWindow.UpdateStartBattleState();
		BaseWindowStaminaObserver observer = new BaseWindowStaminaObserver
		{
			StaminaType = StaminaType.GuildSuppressKey
		};
		SceneAutoBehaviour<LobbyManagement>.instance.OpenLobbyTopmostMenu(observer);
		SetBan();
	}

	public void SetBan()
	{
		if (_tutorialData.Argument.Length < 2)
		{
			return;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < _tutorialData.Argument.Length; i++)
		{
			if (i != 0 && i != 1)
			{
				int argInt = TutorialUtility.GetArgInt(_tutorialData, i);
				list.Add(argInt);
			}
		}
		if (raidWindow != null)
		{
			raidWindow.SetBanByHeroIndices(list);
		}
	}

	void BattlePartySetting.IDataProvider.SetPartyMemberHeroIndex(int slotIndex, int heroIndex, bool isMainParty = true)
	{
	}

	void BattlePartySetting.IDataProvider.ResetPartyMember()
	{
	}

	EHeroState BattlePartySetting.IDataProvider.GetPartyHeroState()
	{
		return EHeroState.Free;
	}

	string BattlePartySetting.IDataProvider.GetStartButtonLabelText()
	{
		return LocalizationManager.GetString(Battle.START_BATTLE);
	}

	StaminaType BattlePartySetting.IDataProvider.GetStaminaType()
	{
		return StaminaType.None;
	}

	int BattlePartySetting.IDataProvider.GetStaminaConsumeCount()
	{
		return 0;
	}

	EnterDungeonData BattlePartySetting.IDataProvider.GetEnterDungeon()
	{
		return null;
	}

	int[] BattlePartySetting.IDataProvider.GetForcedPartyMemberHeroIndices()
	{
		return null;
	}

	int[] BattlePartySetting.IDataProvider.GetPartyMemberHeroIndices(bool isMainParty, int slotCount)
	{
		return _heroIndices;
	}

	int BattlePartySetting.IDataProvider.GetPartyMemberHeroIndex(int slotIndex, bool isMainParty)
	{
		return 0;
	}

	List<CreatureTagType> BattlePartySetting.IDataProvider.GetSelectCreatureTagTypeList()
	{
		return null;
	}

	List<CreatureTagType> BattlePartySetting.IDataProvider.GetAcceptedCreatureTagTypeList()
	{
		return null;
	}

	List<CreatureData> BattlePartySetting.IDataProvider.GetBannedHeroDataList()
	{
		return null;
	}

	void RaidPartySetting.Listener.OnClickInvite(RaidPartySetting caller)
	{
		CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_3"));
	}

	void RaidPartySetting.Listener.OnClickRaidInfo(RaidPartySetting caller)
	{
		CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_3"));
	}

	void RaidPartySetting.Listener.OnClickPartyMemberName(MemberInfo memberInfo)
	{
	}

	void RaidPartySetting.Listener.OnPartyMemberJoin(MemberInfo memberInfo)
	{
	}

	void RaidPartySetting.Listener.OnPartyMemberLeave(long accountId)
	{
	}

	void RaidPartySetting.Listener.OnAutoDestroyGuildRaidParty()
	{
	}

	void RaidPartySetting.Listener.OnSubmitPartyChat(string chat)
	{
	}

	List<ChatInfo> RaidPartySetting.Listener.GetPartyChat()
	{
		return null;
	}

	void RaidPartySetting.Listener.OnClickEmoticonSelect(RaidPartySetting caller)
	{
	}

	bool RaidPartySetting.Listener.CanInputPartyChat()
	{
		return false;
	}

	void RaidPartySetting.Listener.OnClickRoomSetting(int raidIndex, int raidLevel)
	{
		CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_3"));
	}

	void RaidPartySetting.Listener.OnClickSingleRoomSetting(int raidIndex, int raidLevel)
	{
	}

	BanRuleData RaidPartySetting.Listener.GetRaidBanData(RaidData raidData)
	{
		return null;
	}

	void RaidPartySetting.Listener.OnEnterRaidPartySetting(RaidData raidData)
	{
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
	}

	bool BattlePartySetting.Listener.OnCloseRequest(BattlePartySetting closeThis)
	{
		return false;
	}

	void BattlePartySetting.Listener.OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
		CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_3"));
	}

	bool BattlePartySetting.Listener.OpenAutoNextBattleSelect()
	{
		return false;
	}

	void RaidPartySetting.Listener.OnClickOpenHero()
	{
		CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_3"));
	}

	void RaidPartySetting.Listener.OnClickOpenInventory()
	{
		CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_3"));
	}

	bool BattlePartySetting.IDataProvider.EnableOnetimeBooster()
	{
		return false;
	}

	void RaidPartySetting.Listener.OnClickClose(RaidPartySetting caller)
	{
		CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_3"));
	}

	void RaidPartySetting.Listener.OnClickAffixSetting(int raidindex, int raidLevel, int[] affixIndices, bool canChange)
	{
	}

	void RaidPartySetting.Listener.OnClickStartBattle(RaidPartySetting caller)
	{
		if (raidWindow.GetPartyMemberListCount() < raidWindow.RaidData.DeckCount)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_8"));
		}
	}

	int[] BattlePartySetting.IDataProvider.GetAffixIndices()
	{
		return null;
	}

	void BattlePartySetting.IDataProvider.SetAffixIndices(int[] affixIndices)
	{
	}

	RaidPartySetting.ClassEnterPermissionBanTypeData BattlePartySetting.IDataProvider.GetClassEnterPermissionBanTypeData()
	{
		return null;
	}

	List<BanRuleData> BattlePartySetting.IDataProvider.GetBanRuleDataList()
	{
		return null;
	}
}
