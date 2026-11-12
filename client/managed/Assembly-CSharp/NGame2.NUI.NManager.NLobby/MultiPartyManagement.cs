using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NChatting;
using NGame2.NContents;
using NGame2.NParty;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NChatting;
using NShared;
using NShared.CreatePartyDungeonRoom;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NShared.NParty;
using NShared.SearchPartyDungeonRoom;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.NLobby;

public class MultiPartyManagement : BaseManagement<MultiPartyManagement>, MultiPartySetting.Listener, RaidPartySetting.Listener, BattlePartySetting.Listener, PartyDungeonRoomSetting.Listener, PartyManager.INewPartyListener, IChatListener, PartyPlayPopup.Listener, PartyDungeonRoomList.Listener
{
	private PartyDungeonData _partyDungeonData;

	protected override void Awake()
	{
		base.Awake();
		AddInstance<MultiPartySetting>("Resources/Prefab2/Window/Multi/MultiPartySetting");
		AddInstance<RaidInviteList>("Resources/Prefab2/Window/Raid/RaidInviteList");
		AddInstance<PartyDungeonRoomList>("Resources/Prefab2/Window/PartyDungeon/PartyDungeonRoomList");
		AddInstance<PartyDungeonRoomSetting>("Resources/Prefab2/Window/PartyDungeon/PartyDungeonRoomSetting");
		GlobalAutoBehaviour<PartyManager>.instance.AddNewPartyListener(this);
		GlobalAutoBehaviour<ChattingManager>.instance.AddChatListener(this);
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			if (GlobalAutoBehaviour<PartyManager>.isValidInstance)
			{
				GlobalAutoBehaviour<PartyManager>.instance.RemoveNewPartyListener(this);
			}
			if (GlobalAutoBehaviour<ChattingManager>.isValidInstance)
			{
				GlobalAutoBehaviour<ChattingManager>.instance.RemoveChatListener(this);
			}
		}
	}

	public void CreateMultiParty(int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty)
	{
		PartyDungeonData data = GameRoot.Instance.PartyDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			CreateMultiParty(data.DungeonType, chapterIndex, dungeonIndex, difficulty, isPublic: true);
		}
	}

	public void CreateMultiParty(PartyDungeonType dungeonType, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty, bool isPublic)
	{
		NShared.CreatePartyDungeonRoom.Request request = new NShared.CreatePartyDungeonRoom.Request
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			DungeonDifficulty = difficulty,
			DungeonType = dungeonType,
			Opened = (byte)(isPublic ? 1u : 0u)
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.CreatePartyDungeonRoom.Response response)
		{
			if (response.Result != NShared.CreatePartyDungeonRoom.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				RaidRoomInfo roomInfo = new RaidRoomInfo
				{
					RoomNo = response.RoomNo,
					DungeonType = dungeonType,
					ChapterIndex = chapterIndex,
					DungeonIndex = dungeonIndex,
					DungeonDifficulty = difficulty,
					MasterNick = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick,
					MasterAccountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId,
					CurPartyMember = 1,
					Opened = request.Opened
				};
				GlobalAutoBehaviour<PartyManager>.instance.CreateParty(chapterIndex, dungeonIndex, roomInfo, isRaid: false);
			}
		});
	}

	public void OpenMultiPartySetting(int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			VespaDebug.LogError("DungeonData found :  chapter=" + chapterIndex + " dungeon=" + dungeonIndex + " difficulty=" + difficulty);
			return;
		}
		int staminaOfDifficulty = data.GetStaminaOfDifficulty(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina());
		if (staminaOfDifficulty < 0)
		{
			VespaDebug.LogError("chicken is under 0 :  chapter=" + chapterIndex + " dungeon=" + dungeonIndex + " difficulty=" + difficulty);
		}
		else
		{
			_partyDungeonData = GameRoot.Instance.PartyDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
			UIHelper.CloseWindowsBeforeRaidPartySetting();
			EnterDungeonData enterDungeon = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, UIHelper.GetDungeonDifficultyType(difficulty));
			DefaultBattlePartySettingDataProdiver dataProvider = new DefaultBattlePartySettingDataProdiver(null, StaminaType.Chicken, staminaOfDifficulty, Battle.START_BATTLE, enterDungeon);
			GetInstance<MultiPartySetting>().Open(this, dataProvider);
		}
	}

	void PartyPlayPopup.Listener.OnClick_PartyCreate(EnterDungeonData enterDungeonInfo)
	{
		if (IsEnablePartyPlay(enterDungeonInfo))
		{
			CreateMultiParty(enterDungeonInfo.Chapter, enterDungeonInfo.LinkedDungeon, UIHelper.GetPartyDungeonDifficultyType(enterDungeonInfo.Difficulty));
		}
		else
		{
			MessagePopup.Open("NotEnter", "NotEnter");
		}
	}

	void PartyPlayPopup.Listener.OnClick_PartyList()
	{
		OpenPartyDungeonRoomList();
	}

	void PartyPlayPopup.Listener.OnClick_PartySearch(EnterDungeonData enterDungeonInfo)
	{
		if (IsEnablePartyPlay(enterDungeonInfo))
		{
			GlobalAutoBehaviour<PartyManager>.instance.PveSearchPartyStart(enterDungeonInfo.Chapter, enterDungeonInfo.LinkedDungeon, UIHelper.GetPartyDungeonDifficultyType(enterDungeonInfo.Difficulty));
		}
		else
		{
			MessagePopup.Open("NotEnter", "NotEnter");
		}
	}

	public static PartyDungeonDifficultyType GetPartyDifficultyByDungeonDifficulty(NShared.DungeonDifficultyType dungeonDifficulty)
	{
		return EnumAttributeContainer<NShared.DungeonDifficultyType, DungeonDifficultyTypeAttribute>.Get(dungeonDifficulty)?.PartyDungeonDifficultyType ?? PartyDungeonDifficultyType.None;
	}

	public static bool CheckPartyDungeonContentsStatus()
	{
		ContentsStatus contentsStatus = SingletonAuto<ContentsStatusManager>.instance.GetContentsStatus(ContentsStatusType.PartyDungeon);
		if (contentsStatus == ContentsStatus.Disable || contentsStatus == ContentsStatus.Hide)
		{
			return false;
		}
		return true;
	}

	public static PartyDungeonData GetPartyDungeonData(EnterDungeonData enterDungeonData)
	{
		return GetPartyDungeonData(enterDungeonData.Chapter, enterDungeonData.LinkedDungeon);
	}

	public static PartyDungeonData GetPartyDungeonData(int chpaterIndex, int dungeonIndex)
	{
		return GameRoot.Instance.PartyDungeonDataContainer.GetData(chpaterIndex, dungeonIndex);
	}

	public static bool IsEnablePartyPlay(int chpaterIndex, int dungeonIndex, NShared.DungeonDifficultyType difficulty)
	{
		PartyDungeonData partyDungeonData = GetPartyDungeonData(chpaterIndex, dungeonIndex);
		if (partyDungeonData == null)
		{
			return false;
		}
		return difficulty switch
		{
			NShared.DungeonDifficultyType.Easy => partyDungeonData.Easy, 
			NShared.DungeonDifficultyType.Normal => partyDungeonData.Normal, 
			NShared.DungeonDifficultyType.Hard => partyDungeonData.Hard, 
			NShared.DungeonDifficultyType.Hell => partyDungeonData.Hell, 
			_ => false, 
		};
	}

	public static bool IsEnablePartyPlay(EnterDungeonData enterDungeonData)
	{
		if (!SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.PartyPlayDungeon))
		{
			return false;
		}
		return IsEnablePartyPlay(enterDungeonData.Chapter, enterDungeonData.LinkedDungeon, enterDungeonData.Difficulty);
	}

	public void OpenAutoSearchCancelPopup()
	{
		string stringDirect = LocalizationManager.GetStringDirect("PARTY_SEARCH");
		string stringDirect2 = LocalizationManager.GetStringDirect("PARTY_SEARCH_CANCEL_WARNING");
		MessagePopup3Button.OpenGlobal(stringDirect, stringDirect2, delegate
		{
			GlobalAutoBehaviour<PartyAutoSearchManager>.instance.CloseSearch();
		}, null, NShared.NLocalization.Common.COMMON_CONFIRM, NShared.NLocalization.Common.COMMON_CANCEL);
	}

	public bool CheckEnterBattle()
	{
		if (!GlobalAutoBehaviour<PartyAutoSearchManager>.instance.DoAutoSearch)
		{
			return true;
		}
		OpenAutoSearchCancelPopup();
		return false;
	}

	public bool CheckEnterBattle(EnterDungeonData enterDungeonData)
	{
		if (!GlobalAutoBehaviour<PartyAutoSearchManager>.instance.DoAutoSearch)
		{
			return true;
		}
		if (enterDungeonData == null)
		{
			OpenAutoSearchCancelPopup();
			return false;
		}
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(enterDungeonData.Chapter, enterDungeonData.LinkedDungeon);
		if (data == null)
		{
			OpenAutoSearchCancelPopup();
			return false;
		}
		switch (data.BattleType)
		{
		case BattleType.Campaign:
		case BattleType.Labyrinth:
		case BattleType.Boss:
		case BattleType.Hideout:
		case BattleType.Conquest:
		case BattleType.UnderPrison:
			return true;
		case BattleType.None:
		case BattleType.CustomCampaign:
		case BattleType.Trial:
		case BattleType.Arena:
		case BattleType.Tower:
		case BattleType.Pit:
		case BattleType.Party:
		case BattleType.Raid:
		case BattleType.OrdealTower:
		case BattleType.ChallengeTower:
		case BattleType.WorldBoss:
		case BattleType.GuildRaid:
		case BattleType.ArenaBanPick:
		case BattleType.RaidSingle:
		case BattleType.Maze:
		case BattleType.GuildSuppress:
		case BattleType.HardRaid:
		case BattleType.ChallengeRaid:
		case BattleType.ArcdimRaid:
		case BattleType.ArcdimHardRaid:
			OpenAutoSearchCancelPopup();
			return false;
		default:
			return false;
		}
	}

	public void StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
	}

	public bool OnCloseRequest(BattlePartySetting closeThis)
	{
		return true;
	}

	public void OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
	}

	public bool OpenAutoNextBattleSelect()
	{
		return false;
	}

	public void OnClickInvite(RaidPartySetting caller)
	{
		long[] partyMemberIds = GlobalAutoBehaviour<PartyManager>.instance.GetPartyMemberIds();
		GetInstance<RaidInviteList>().Open(SceneAutoBehaviour<RaidManagement>.instance, partyMemberIds);
	}

	public void OnClickRaidInfo(RaidPartySetting caller)
	{
	}

	public void OnClickPartyMemberName(MemberInfo memberInfo)
	{
		InspectUserManagement.InspectUserData inspectUser = new InspectUserManagement.InspectUserData(memberInfo, InspectUserCategory.Party);
		SceneAutoBehaviour<InspectUserManagement>.instance.OpenMenuPopup(inspectUser);
	}

	public void OnPartyMemberJoin(MemberInfo memberInfo)
	{
		RaidInviteList raidInviteList = GetInstance<RaidInviteList>();
		if ((bool)raidInviteList && raidInviteList.IsOpen())
		{
			raidInviteList.UpdateInvited(memberInfo.AccountId, invited: true);
		}
	}

	public void OnPartyMemberLeave(long accountId)
	{
		RaidInviteList raidInviteList = GetInstance<RaidInviteList>();
		if ((bool)raidInviteList && raidInviteList.IsOpen())
		{
			raidInviteList.UpdateInvited(accountId, invited: false);
		}
		MultiPartySetting multiPartySetting = GetInstance<MultiPartySetting>();
		if (multiPartySetting.IsOpen() && accountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
		{
			multiPartySetting.Close();
		}
	}

	public void OnAutoDestroyGuildRaidParty()
	{
		NGame2.NUI.NWindow.RaidInfo raidInfo = GetInstance<NGame2.NUI.NWindow.RaidInfo>();
		if (raidInfo != null)
		{
			raidInfo.Close();
		}
	}

	public void OnSubmitPartyChat(string chat)
	{
		SceneAutoBehaviour<ChattingManagement>.instance.SendPartyChat(chat);
	}

	public List<ChatInfo> GetPartyChat()
	{
		return GlobalAutoBehaviour<ChattingManager>.instance.GetChatList(ChatType.Party, 0L);
	}

	public bool CanInputPartyChat()
	{
		return !IsOpened<RaidInviteList>();
	}

	public void OnClickEmoticonSelect(RaidPartySetting caller)
	{
		ChattingPartyComponent chattingComponent = caller.PartyChat;
		if (!(chattingComponent == null))
		{
			ChattingEmoticonSelectPopup.Open(caller, delegate(EmoticonData emoticonData)
			{
				string value = "(" + emoticonData.Keyword + ")";
				chattingComponent.Input.value = value;
				chattingComponent.Input.selectAllTextOnFocus = false;
				chattingComponent.Input.isSelected = true;
			});
		}
	}

	public void OnClickRoomSetting(int raidIndex, int raidLevel)
	{
	}

	public void OnClickSingleRoomSetting(int raidIndex, int raidLevel)
	{
	}

	public void OnClickPartyRoomSetting()
	{
		PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
		_ = partyInfo.PartyRoom.DungeonType;
		int chapterIndex = partyInfo.ChapterIndex;
		int dungeonIndex = partyInfo.DungeonIndex;
		PartyDungeonDifficultyType difficulty = partyInfo.Difficulty;
		OpenPartyDungeonRoomSetting(partyInfo.PartyRoom.DungeonType, chapterIndex, dungeonIndex, difficulty);
	}

	public BanRuleData GetRaidBanData(RaidData raidData)
	{
		return null;
	}

	public void OnEnterRaidPartySetting(RaidData raidData)
	{
	}

	public void OnClickOpenHero()
	{
		List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
		CachedHeroInfo cachedHeroInfo = ((__Extension_Method_ICollection__.COUNT(heroInfoList) > 0) ? heroInfoList[0] : null);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfoList); i++)
		{
			if (heroInfoList[i] != null)
			{
				cachedHeroInfo = heroInfoList[i];
				break;
			}
		}
		if (cachedHeroInfo == null)
		{
			VespaDebug.LogError("Cannot select hero.");
		}
		else
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHero(cachedHeroInfo);
		}
	}

	public void OnClickOpenInventory()
	{
		SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
	}

	public void OnClickStartBattle(RaidPartySetting caller)
	{
		caller.OnClickStartBattle();
	}

	public void OnClickClose(RaidPartySetting caller)
	{
	}

	public void OnClickAffixSetting(int raidIndex, int raidLeve, int[] affixIndices, bool canChange)
	{
	}

	void PartyManager.INewPartyListener.OnPartyCreated()
	{
		PartyInfo partyInfo = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
		if (!partyInfo.IsValid())
		{
			VespaDebug.LogError("Party created but is invalid");
		}
		else if (GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo() == null)
		{
			VespaDebug.LogError("Party created but cannot find my member info.");
		}
		else if (GameRoot.Instance.PartyDungeonDataContainer.GetData(partyInfo.ChapterIndex, partyInfo.DungeonIndex) != null)
		{
			OpenMultiPartySetting(partyInfo.ChapterIndex, partyInfo.DungeonIndex, partyInfo.Difficulty);
		}
	}

	void PartyManager.INewPartyListener.OnPartyInvited(long senderId, InvitePartyReq data)
	{
	}

	void IChatListener.OnChatAdded(ChatInfo chatInfo, bool showPreview, EmotiType emoticon)
	{
		if (_partyDungeonData != null && chatInfo != null && chatInfo.ChatType == ChatType.Party)
		{
			MultiPartySetting multiPartySetting = SceneAutoBehaviour<MultiPartyManagement>.instance.GetInstance<MultiPartySetting>();
			if ((bool)multiPartySetting && (bool)multiPartySetting.PartyChat)
			{
				multiPartySetting.PartyChat.AddChat(chatInfo);
				GlobalAutoBehaviour<ChattingManager>.instance.MakeChatChecked(chatInfo.ChatType, 0L);
				multiPartySetting.PartyChat.Reposition();
			}
		}
	}

	void IChatListener.OnWhisperUserAdded(long accountId, string userName)
	{
	}

	void IChatListener.OnWhisperUserRemoved(long accountId)
	{
	}

	public void CreatePartyDungeonRoom(PartyDungeonType type, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty, bool isPublic)
	{
		CreateMultiParty(type, chapterIndex, dungeonIndex, difficulty, isPublic);
	}

	public void ChangePartyRoomSetting(PartyDungeonType type, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty, bool isResetSquard)
	{
		CloseDungeonRoomSetting();
		GlobalAutoBehaviour<PartyManager>.instance.SendChangeMultiRoomInfo(type, chapterIndex, dungeonIndex, difficulty, isResetSquard);
	}

	public void ReCreatePartyDungeonRoom(PartyDungeonType type, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty, bool isPublic)
	{
		CloseDungeonRoomSetting();
		MultiPartySetting multiPartySetting = GetInstance<MultiPartySetting>();
		if (multiPartySetting.IsOpen())
		{
			multiPartySetting.Close();
		}
		CreateMultiParty(type, chapterIndex, dungeonIndex, difficulty, isPublic);
	}

	public void OpenPartyDungeonRoomSetting(PartyDungeonType type, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty)
	{
		GetInstance<PartyDungeonRoomSetting>().OpenSetting(this, chapterIndex, dungeonIndex, difficulty);
	}

	public void CloseDungeonRoomSetting()
	{
		GetInstance<PartyDungeonRoomSetting>().Close();
	}

	void PartyDungeonRoomList.Listener.OnClickEnterDungeon(RaidRoomInfo roomInfo)
	{
		SceneAutoBehaviour<RaidManagement>.instance.RequestRaidRoomEnter(roomInfo);
	}

	void PartyDungeonRoomList.Listener.RefreshList(PartyDungeonType dungeonType, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType diff, Action onResponse = null)
	{
		RequestPartyDungeonRoomList(dungeonType, chapterIndex, dungeonIndex, diff, onResponse);
	}

	void PartyDungeonRoomList.Listener.OnClickPartyCreate()
	{
		SceneAutoBehaviour<MultiPartyManagement>.instance.GetInstance<PartyDungeonRoomSetting>().Open(SceneAutoBehaviour<MultiPartyManagement>.instance);
	}

	void PartyDungeonRoomList.Listener.OnClickAutoParty(PartyDungeonType dungeonType, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType diff)
	{
		GlobalAutoBehaviour<PartyManager>.instance.PveSearchPartyStart(dungeonType, chapterIndex, dungeonIndex, diff);
	}

	public void OpenPartyDungeonRoomList()
	{
		RequestPartyDungeonRoomList(PartyDungeonType.Campaign, 0, 0, PartyDungeonDifficultyType.None);
	}

	private void RequestPartyDungeonRoomList(PartyDungeonType dungeonType, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType diff, Action onResponse = null)
	{
		NShared.SearchPartyDungeonRoom.Request request = new NShared.SearchPartyDungeonRoom.Request
		{
			DungeonType = dungeonType,
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			DungeonDifficulty = diff
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SearchPartyDungeonRoom.Response response)
		{
			if (response.Result != NShared.SearchPartyDungeonRoom.ResultType.Success)
			{
				CenterMessage.Open(LocalizationManager.GetString(response.Result));
			}
			if (response.BaseResult != BaseResultType.Success)
			{
				CenterMessage.Open(LocalizationManager.GetString(response.BaseResult));
			}
			PartyDungeonRoomList partyDungeonRoomList = GetInstance<PartyDungeonRoomList>();
			if (!partyDungeonRoomList.IsOpen())
			{
				partyDungeonRoomList.Open(response.RoomInfos, this);
			}
			else
			{
				partyDungeonRoomList.RefreshList(response.RoomInfos);
			}
			Vespa_System_Action_Extension_Method.fire(onResponse);
		});
	}
}
