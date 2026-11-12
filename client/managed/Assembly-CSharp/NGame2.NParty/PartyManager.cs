using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NContents;
using NGame2.NEventTrigger;
using NGame2.NGuild;
using NGame2.NMessageLog;
using NGame2.NNotify;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NSocket;
using NGame2.NUI;
using NGame2.NUI.NHelper.NAffix;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.BeginCampaign;
using NShared.ChangePartyDungeonRoomInfo;
using NShared.ChangeRaidRoomInfo;
using NShared.DelegateRaidRoomMaster;
using NShared.JoinPartyDungeonRoom;
using NShared.JoinRaidRoom;
using NShared.LeavePartyDungeonRoom;
using NShared.LeaveRaidRoom;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NShared.NParty;
using NShared.PingPartyDungeonRoom;
using NShared.PingRaidRoom;
using NShared.SearchPartyDungeonRoom;
using NShared.WaitPartyDungeonRoom;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NParty;

public class PartyManager : GlobalAutoBehaviour<PartyManager>, IUserObserver<UserManager.UserStatInfo>, IUserObserver
{
	public interface IPartyListener
	{
		void OnPartyDestroy(bool isMasterLeave = false);

		void OnPartyMemberJoin(MemberInfo memberInfo);

		void OnPartyMemberDeckHeroChanged(MemberInfo memberInfo, int heroIndex, CachedHeroInfo heroInfo, bool isMain = true);

		void OnPartyMemberReady(MemberInfo memberInfo, bool isReady);

		void OnPartyMemberChangeAvatar(MemberInfo memberInfo);

		void OnPartyMemberLeave(MemberInfo memberInfo);

		void OnChangePartyMemberStatus(MemberInfo memberInfo);

		void OnPartyRoomInfoUpdated(RaidRoomInfo raidRoomInfo, bool isViolateBanRule);

		void OnPartyRoomMasterChanged(MemberInfo memberInfo);

		void OnPartyMemberResults(MemberInfo memberInfo);

		void OnPartyRoomSetRepeat(bool IsRepeat);

		void OnPartyMemberDeckCostumeChanged(long accountId, int heroIndex, CachedHeroInfo heroInfo);
	}

	public interface INewPartyListener
	{
		void OnPartyCreated();

		void OnPartyInvited(long senderId, InvitePartyReq data);
	}

	public interface IRewardListener
	{
		void OnSelectRewardItem(MemberInfo memberInfo);
	}

	public class JoinPartyReqHelperClass
	{
		public long InvitorAccountId { get; set; }

		public DateTime SendJoinPartyReqTime { get; set; }
	}

	public class StartBattleResult
	{
		public enum ErrorCode
		{
			None,
			DungeonDataNotFound,
			MyMemberInfoNotFound,
			NullGameServerResponse,
			BeginCampaignBaseError,
			BeginCampaignFailed,
			NoBattleServer,
			BattleServerConnectError,
			BattleServerLoginError,
			HeroDuplicatedNotAllowed
		}

		public ErrorCode Error;

		public NShared.BeginCampaign.Response GameServerResponse;
	}

	private float RaidRoomPingInterval;

	private float JoinPartyResWaitTime;

	private List<IPartyListener> _partyListenerList;

	private List<INewPartyListener> _newPartyListenerList;

	private List<IRewardListener> _rewardListenerList;

	private DateTime _lastSendRaidRoomPingTime = DateTime.UtcNow;

	private DateTime _lastSendMultiRoomPingTime = DateTime.UtcNow;

	public float PingInterval = 10f;

	public float CheckInterval = 15f;

	public float ExpiredTime = 25f;

	private JoinPartyReqHelperClass JoinPartyReqHelper;

	private bool IsSendBattleStart;

	private Action<long, PartyMemberDungeonInfoRes> _otherDungeonInfoAction;

	public static bool IsSingle;

	private const string _searchSuccessRoomCountKey = "PartySearchMinimumRoom";

	public PartyInfo PartyInfo { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		_partyListenerList = new List<IPartyListener>();
		_newPartyListenerList = new List<INewPartyListener>();
		_rewardListenerList = new List<IRewardListener>();
		PartyInfo = new PartyInfo();
		PartyInfo.PartyMembers = new List<MemberInfo>();
		PartyInfo.Reset();
		RaidRoomPingInterval = GameRoot.Instance.ConstantDataContainer.GetValueInt("RaidRoomPingInterval");
		JoinPartyResWaitTime = GameRoot.Instance.ConstantDataContainer.GetValueInt("JoinPartyResWaitTime");
		RegisterMessageLogHandler();
	}

	protected void RegisterMessageLogHandler()
	{
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<InvitePartyReq>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<JoinPartyReq>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<JoinPartyRes>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<JoinPartyNotice>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<LeaveParty>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<PartyMemberBanned>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<PartyDeckHeroInfo>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<PartyDeckHeroCancel>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<PartyRoomReady>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<PartyBattleStart>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<PartyMemberChangeAvatar>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<PartyMemberSelectReward>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<PartyMemberDungeonInfoReq>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<PartyMemberDungeonInfoRes>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<ChangePartyMemberStatus>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<ChangePartyRoomInfo>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<ChangePartyRoomMaster>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<JoinPartyReqCancel>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<KickedRaidMemberRes>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<RaidRoomDestroyed>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<ChangePartyDungeonRoomInfo>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<MemberResultsRes>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<PartyRoomRepeat>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<InvalidPartyMember>
		{
			Handler = Handle
		});
		SingletonAuto<MessageLogHandler>.instance.RegisterHandler(new DelegateMessageProtocolHandler<PartyDeckCostumeInfo>
		{
			Handler = Handle
		});
	}

	public void SetRaidPartyInfo(int raidIndex, int raidLevel, RaidRoomInfo roomInfo)
	{
		PartyInfo.Reset();
		PartyInfo.SetRaid(raidIndex, raidLevel);
		PartyInfo.PartyMembers = new List<MemberInfo>();
		PartyInfo.PartyMembers.Add(CreateMyMemberInfo(isPartyMaster: true));
		PartyInfo.CreatedTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		PartyInfo.PartyRoom = roomInfo;
	}

	public void CreateParty(int key1, int key2, RaidRoomInfo roomInfo, bool isRaid, bool callBack = true)
	{
		PartyInfo.Reset();
		if (isRaid)
		{
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(key1, key2);
			if (data.Type == RaidType.GuildSuppress || data.Type == RaidType.GuildSuppressSingle)
			{
				int level = key2;
				if (data.Type == RaidType.GuildSuppressSingle)
				{
					level = GameRoot.Instance.GuildSuppressSessionDataContainer.GetMultiLevel(data.Level);
				}
				GuildSuppressPlayInfo guildSuppressPlayInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressPlayInfo(key1, level);
				if (guildSuppressPlayInfo == null)
				{
					return;
				}
				int dungeonIndexByLevel = GameRoot.Instance.GuildSuppressSessionDataContainer.GetDungeonIndexByLevel(guildSuppressPlayInfo.SessionIndex, guildSuppressPlayInfo.Level);
				SetGuildSuppress(dungeonIndexByLevel, key1, key2);
			}
			else
			{
				PartyInfo.SetRaid(key1, key2);
			}
		}
		else
		{
			PartyInfo.SetMulti(key1, key2, roomInfo.DungeonDifficulty);
		}
		PartyInfo.PartyMembers = new List<MemberInfo>();
		PartyInfo.PartyMembers.Add(CreateMyMemberInfo(isPartyMaster: true));
		PartyInfo.CreatedTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		PartyInfo.PartyRoom = roomInfo;
		if (callBack)
		{
			_newPartyListenerList.ForEach(delegate(INewPartyListener e)
			{
				e.OnPartyCreated();
			});
		}
	}

	public bool IsValidParty()
	{
		if (PartyInfo != null)
		{
			return PartyInfo.IsValid();
		}
		return false;
	}

	public void SetRaid(int raidIndex, int raidLevel)
	{
		PartyInfo.SetRaid(raidIndex, raidLevel);
	}

	public void SetMulti(int chapterIndex, int dungeonIndex)
	{
	}

	private int SetJoinPartyMemberIndex(PartyInfo partyInfo)
	{
		int maxPartyMemberCount = GetMaxPartyMemberCount(partyInfo);
		List<MemberInfo> partyMembers = partyInfo.PartyMembers;
		bool flag = false;
		for (int i = 0; i < maxPartyMemberCount; i++)
		{
			flag = false;
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(partyMembers); j++)
			{
				if (partyMembers[j].MemberIndex == i)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return i;
			}
		}
		return -1;
	}

	public void SetGuildSuppress(int guildSuppressDungeonIndex, int raidIndex, int raidLevel)
	{
		PartyInfo.GuildSuppressDungeonIndex = guildSuppressDungeonIndex;
		PartyInfo.SetRaid(raidIndex, raidLevel);
	}

	public int GetMaxPartyMemberCount(PartyInfo partyInfo)
	{
		if (!IsValidParty())
		{
			return 0;
		}
		if (partyInfo.RaidIndex != 0 && partyInfo.RaidLevel != 0)
		{
			int raidIndex = partyInfo.RaidIndex;
			int raidLevel = partyInfo.RaidLevel;
			return GetMaxPartyMemberCountForRaid(raidIndex, raidLevel);
		}
		if (partyInfo.ChapterIndex != 0 && partyInfo.DungeonIndex != 0)
		{
			int chapterIndex = partyInfo.ChapterIndex;
			int dungeonIndex = partyInfo.DungeonIndex;
			return GetMaxPartyMemberCountForMulti(chapterIndex, dungeonIndex);
		}
		return 0;
	}

	public int GetMaxPartyMemberCount()
	{
		return GetMaxPartyMemberCount(PartyInfo);
	}

	public int GetMaxPartyMemberCountForMulti(int chapterIndex, int dungeonIndex)
	{
		PartyDungeonData data = GameRoot.Instance.PartyDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return 0;
		}
		return System.Math.Max(data.PlayerCount, 2);
	}

	public int GetMaxPartyMemberCountForRaid(int raidIndex, int raidLevel)
	{
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel);
		if (data == null)
		{
			return 0;
		}
		return System.Math.Max(data.PlayerCount, data.IsOnlineSingle ? 1 : 2);
	}

	public RaidType GetRaidType()
	{
		return GameRoot.Instance.RaidDataContainer.GetData(PartyInfo.RaidIndex, PartyInfo.RaidLevel)?.Type ?? RaidType.DragonNormal;
	}

	public bool CanInviteSomeone()
	{
		if (!IsValidParty())
		{
			return true;
		}
		MemberInfo myPartyMemberInfo = GetMyPartyMemberInfo();
		if (myPartyMemberInfo == null)
		{
			return false;
		}
		if (!myPartyMemberInfo.IsMaster)
		{
			return false;
		}
		if (IsBattleStarted())
		{
			return false;
		}
		int maxPartyMemberCount = GetMaxPartyMemberCount();
		return __Extension_Method_ICollection__.COUNT(PartyInfo.PartyMembers) < maxPartyMemberCount;
	}

	public bool IsBattleStarted()
	{
		if (!IsSendBattleStart)
		{
			return PartyInfo.IsBattleStarted();
		}
		return true;
	}

	public bool IsFullHero()
	{
		int num = 0;
		if (PartyInfo.RaidIndex != 0 && PartyInfo.RaidLevel != 0)
		{
			int raidIndex = PartyInfo.RaidIndex;
			int raidLevel = PartyInfo.RaidLevel;
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel);
			if (data == null)
			{
				return false;
			}
			num = data.DeckCount;
		}
		else if (PartyInfo.ChapterIndex != 0 && PartyInfo.DungeonIndex != 0)
		{
			int chapterIndex = PartyInfo.ChapterIndex;
			int dungeonIndex = PartyInfo.DungeonIndex;
			PartyDungeonData data2 = GameRoot.Instance.PartyDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
			if (data2 == null)
			{
				return false;
			}
			num = data2.DeckCount;
		}
		if (num > PartyInfo.GetDeckCount())
		{
			return false;
		}
		return true;
	}

	private void Update()
	{
		UpdateSendRaidRoomPing();
		UpdateSendMultiRoomPing();
		UpdateCheckJoinPartyRes();
	}

	bool IUserObserver.IsGlobal()
	{
		return true;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo.UserStatType == UserManager.UserStatType.AvatarHeroIndex)
		{
			int avatarHeroIndex = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AvatarHeroIndex;
			PartyMemberChangeAvatar partyMemberChangeAvatar = new PartyMemberChangeAvatar();
			partyMemberChangeAvatar.HeroIndex = avatarHeroIndex;
			SendToAllMembers(partyMemberChangeAvatar);
		}
	}

	public void AddPartyListener(IPartyListener listener)
	{
		if (listener != null)
		{
			_partyListenerList.Add(listener);
		}
	}

	public void RemovePartyListener(IPartyListener listener)
	{
		_partyListenerList.Remove(listener);
	}

	public void AddNewPartyListener(INewPartyListener listener)
	{
		if (listener != null)
		{
			_newPartyListenerList.Add(listener);
		}
	}

	public void RemoveNewPartyListener(INewPartyListener listener)
	{
		_newPartyListenerList.Remove(listener);
	}

	public void AddRewardistener(IRewardListener listener)
	{
		if (listener != null)
		{
			_rewardListenerList.Add(listener);
		}
	}

	public void RemoveRewardListener(IRewardListener listener)
	{
		_rewardListenerList.Remove(listener);
	}

	public void StartBattle(int raidIndex, int raidLevel, int ItemBoosterCount, Action<StartBattleResult> onResult = null)
	{
		HashSet<int> heroIndexList = new HashSet<int>();
		foreach (MemberInfo partyMember in PartyInfo.PartyMembers)
		{
			AddDeckHero(partyMember.DeckHeros, ref heroIndexList, onResult);
		}
		IsSendBattleStart = true;
		string masterServerPrivateIp = GlobalAutoBehaviour<AccountManager>.instance.ServerPrivateIp;
		int masterServerPort = GlobalAutoBehaviour<AccountManager>.instance.ServerPort;
		string masterServerIp = masterServerPrivateIp;
		int masterServerPort2 = masterServerPort;
		int raidIndex2 = raidIndex;
		int raidLevel2 = raidLevel;
		Action<StartBattleResult> onResult2 = delegate(StartBattleResult result)
		{
			IsSendBattleStart = false;
			if (result.Error != StartBattleResult.ErrorCode.None)
			{
				Vespa_System_Action_Extension_Method.fire(onResult, result);
			}
			else
			{
				NShared.BeginCampaign.Response gameServerResponse = result.GameServerResponse;
				if (gameServerResponse == null || gameServerResponse.BaseResult != BaseResultType.Success || gameServerResponse.Result != NShared.BeginCampaign.ResultType.Success)
				{
					Vespa_System_Action_Extension_Method.fire(onResult, result);
				}
				else
				{
					string battleServerAddress = gameServerResponse.BattleServerAddress;
					int battleServerPort = gameServerResponse.BattleServerPort;
					RewardItemInfo[] dropItems = gameServerResponse.DropItems;
					PartyInfo.SetCampaignInfo(masterServerPrivateIp, masterServerPort, dropItems);
					PartyBattleStart data = new PartyBattleStart
					{
						BattleServerAddress = battleServerAddress,
						BattleServerPort = battleServerPort,
						RewardItems = dropItems,
						MasterServerAddress = masterServerPrivateIp,
						MasterServerPort = masterServerPort,
						RaidIndex = raidIndex,
						RaidLevel = raidLevel
					};
					SendToAllMembers(data);
					Vespa_System_Action_Extension_Method.fire(onResult, result);
				}
			}
		};
		VespaCoroutine.Start(this, coRequestBeginMultiplayCampaign(null, 0, masterServerIp, masterServerPort2, raidIndex2, raidLevel2, ItemBoosterCount, onResult2));
	}

	private void AddDeckHero(Dictionary<int, CachedHeroInfo> dic, ref HashSet<int> heroIndexList, Action<StartBattleResult> onResult)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(dic))
		{
			return;
		}
		foreach (KeyValuePair<int, CachedHeroInfo> item in dic)
		{
			CachedHeroInfo value = item.Value;
			if (value != null)
			{
				int heroIndex = value.HeroIndex;
				if (heroIndexList.Contains(heroIndex))
				{
					Vespa_System_Action_Extension_Method.fire(onResult, new StartBattleResult
					{
						Error = StartBattleResult.ErrorCode.HeroDuplicatedNotAllowed
					});
					break;
				}
				heroIndexList.Add(heroIndex);
			}
		}
	}

	public void StartBattleParty(int[] heroIndices, long[] accountIds, long ItemBoosterCount, Action<StartBattleResult> onResult = null)
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (MemberInfo partyMember in PartyInfo.PartyMembers)
		{
			foreach (KeyValuePair<int, CachedHeroInfo> deckHero in partyMember.DeckHeros)
			{
				CachedHeroInfo value = deckHero.Value;
				if (value != null)
				{
					int heroIndex = value.HeroIndex;
					if (hashSet.Contains(heroIndex))
					{
						Vespa_System_Action_Extension_Method.fire(onResult, new StartBattleResult
						{
							Error = StartBattleResult.ErrorCode.HeroDuplicatedNotAllowed
						});
						return;
					}
					hashSet.Add(heroIndex);
				}
			}
		}
		IsSendBattleStart = true;
		string masterServerPrivateIp = GlobalAutoBehaviour<AccountManager>.instance.ServerPrivateIp;
		int masterServerPort = GlobalAutoBehaviour<AccountManager>.instance.ServerPort;
		VespaCoroutine.Start(this, coRequestBeginMultiplayCampaign(null, 0, masterServerPrivateIp, masterServerPort, 0, 0, ItemBoosterCount, delegate(StartBattleResult result)
		{
			IsSendBattleStart = false;
			if (result.Error != StartBattleResult.ErrorCode.None)
			{
				if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
				{
					NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
				}
				Vespa_System_Action_Extension_Method.fire(onResult, result);
			}
			else
			{
				NShared.BeginCampaign.Response gameServerResponse = result.GameServerResponse;
				if (gameServerResponse == null || gameServerResponse.BaseResult != BaseResultType.Success || gameServerResponse.Result != NShared.BeginCampaign.ResultType.Success)
				{
					if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
					{
						NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
					}
					Vespa_System_Action_Extension_Method.fire(onResult, result);
				}
				else
				{
					string battleServerAddress = gameServerResponse.BattleServerAddress;
					int battleServerPort = gameServerResponse.BattleServerPort;
					RewardItemInfo[] dropItems = gameServerResponse.DropItems;
					PartyInfo.SetCampaignInfo(masterServerPrivateIp, masterServerPort, dropItems);
					Debug.Log("StartBattleParty HeroIndices : " + heroIndices.Length + "accountIds : " + accountIds.Length);
					PartyBattleStart data = new PartyBattleStart
					{
						BattleServerAddress = battleServerAddress,
						BattleServerPort = battleServerPort,
						RewardItems = dropItems,
						MasterServerAddress = masterServerPrivateIp,
						MasterServerPort = masterServerPort,
						RaidIndex = 0,
						RaidLevel = 0,
						HeroIndices = heroIndices,
						AccountIds = accountIds
					};
					SendToAllMembers(data);
					Vespa_System_Action_Extension_Method.fire(onResult, result);
				}
			}
		}));
	}

	private IEnumerator coRequestBeginMultiplayCampaign(string battleServerAddress, int battleServerPort, string masterServerIp, int masterServerPort, int raidIndex, int raidLevel, long ItemBoosterCount, Action<StartBattleResult> onResult)
	{
		int chapterIndex = PartyInfo.ChapterIndex;
		int dungeonIndex = PartyInfo.DungeonIndex;
		CampaignDungeonData dungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (dungeonData == null)
		{
			VespaDebug.LogError("Cannot find dungeon data : chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			Vespa_System_Action_Extension_Method.fire(onResult, new StartBattleResult
			{
				Error = StartBattleResult.ErrorCode.DungeonDataNotFound
			});
			yield break;
		}
		MemberInfo myPartyMemberInfo = GetMyPartyMemberInfo();
		if (myPartyMemberInfo == null)
		{
			VespaDebug.LogError("Cannot find my member info");
			Vespa_System_Action_Extension_Method.fire(onResult, new StartBattleResult
			{
				Error = StartBattleResult.ErrorCode.MyMemberInfoNotFound
			});
			yield break;
		}
		long masterId = PartyInfo.GetMasterId();
		long[] memberIds = PartyInfo.GetMemberIds();
		bool isMaster = myPartyMemberInfo.IsMaster;
		List<int> heroIndexList = new List<int>();
		List<long> accountIdList = new List<long>();
		foreach (CachedHeroInfo value in myPartyMemberInfo.DeckHeros.Values)
		{
			if (value != null && value.HeroIndex != 0)
			{
				heroIndexList.Add(value.HeroIndex);
				accountIdList.Add(GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
			}
		}
		List<int> aiHeroIndexList = new List<int>();
		foreach (CachedHeroInfo value2 in myPartyMemberInfo.SubDeckHeros.Values)
		{
			if (value2 != null && value2.HeroIndex != 0)
			{
				aiHeroIndexList.Add(value2.HeroIndex);
			}
		}
		List<int> deltaHeroIndexList = null;
		List<int> heroIndexList2 = null;
		if (SingletonAuto<CampaignContext>.isValidInstance && CampaignContext.CheckEachWaveEachTeam(dungeonData.BattleType))
		{
			AddHeroDeck(myPartyMemberInfo.DeckHeros, ref deltaHeroIndexList);
			AddHeroDeck(myPartyMemberInfo.SubDeckHeros, ref heroIndexList2);
			heroIndexList.Clear();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(deltaHeroIndexList))
			{
				heroIndexList.AddRange(deltaHeroIndexList);
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(heroIndexList2))
			{
				heroIndexList.AddRange(heroIndexList2);
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(heroIndexList))
			{
				accountIdList.Clear();
				AddAccountId(heroIndexList, ref accountIdList);
			}
		}
		List<long> list = new List<long>();
		List<CachedUserInfo> partyMemberInfoList = new List<CachedUserInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PartyInfo.PartyMembers); i++)
		{
			MemberInfo memberInfo = PartyInfo.PartyMembers[i];
			if (memberInfo != null)
			{
				partyMemberInfoList.Add(memberInfo);
				if (__Extension_Method_ICollection__.COUNT(memberInfo.DeckHeros) == 0)
				{
					list.Add(memberInfo.AccountId);
				}
			}
		}
		bool IsRepeatBattle = PartyInfo.IsRepeatBattle;
		int[] affixIndices = PartyInfo.GetAffixIndices();
		NShared.BeginCampaign.Request request = new NShared.BeginCampaign.Request
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			DungeonDifficulty = (int)UIHelper.GetDungeonDifficultyType(PartyInfo.PartyRoom.DungeonDifficulty),
			LinkedDungeonIndex = dungeonIndex,
			HeroIndices = heroIndexList.ToArray(),
			AccountIds = accountIdList.ToArray(),
			LeaderHeroIndex = 0,
			Repeat = (IsRepeatBattle && NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled()),
			RaidIndex = raidIndex,
			RaidLevel = raidLevel,
			MultiplayMasterId = masterId,
			MultiplayMemberIds = memberIds,
			MultiplayObserverIds = list.ToArray(),
			MultiplayMasterServerIp = masterServerIp,
			MultiplayMasterServerPort = masterServerPort,
			RaidRoomNo = ((PartyInfo.PartyRoom != null) ? PartyInfo.PartyRoom.RoomNo : 0),
			ItemBoosterCount = ItemBoosterCount,
			UseStamina = myPartyMemberInfo.UseStamina,
			DeltaHeroIndices = ((!__Extension_Method_ICollection__.IsNullOrEmpty(deltaHeroIndexList)) ? deltaHeroIndexList.ToArray() : null),
			ZetaHeroIndices = ((!__Extension_Method_ICollection__.IsNullOrEmpty(heroIndexList2)) ? heroIndexList2.ToArray() : null),
			AffixIndices = affixIndices
		};
		if (SingletonAuto<CampaignContext>.isValidInstance && CampaignContext.CheckEachWaveEachTeam(dungeonData.BattleType))
		{
			request.AiHeroIndices = ((!__Extension_Method_ICollection__.IsNullOrEmpty(deltaHeroIndexList)) ? deltaHeroIndexList.ToArray() : null);
		}
		else
		{
			request.AiHeroIndices = ((!__Extension_Method_ICollection__.IsNullOrEmpty(aiHeroIndexList)) ? aiHeroIndexList.ToArray() : null);
		}
		bool executeEnd = false;
		NShared.BeginCampaign.Response response = null;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BeginCampaign.Response iResponse)
		{
			executeEnd = true;
			response = iResponse;
		});
		while (!executeEnd)
		{
			yield return null;
		}
		if (response == null)
		{
			if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
			}
			Vespa_System_Action_Extension_Method.fire(onResult, new StartBattleResult
			{
				Error = StartBattleResult.ErrorCode.NullGameServerResponse
			});
			yield break;
		}
		if (response.BaseResult != BaseResultType.Success)
		{
			if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
			}
			Vespa_System_Action_Extension_Method.fire(onResult, new StartBattleResult
			{
				Error = StartBattleResult.ErrorCode.BeginCampaignBaseError,
				GameServerResponse = response
			});
			yield break;
		}
		if (response.Result != NShared.BeginCampaign.ResultType.Success)
		{
			if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
			{
				NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
			}
			Vespa_System_Action_Extension_Method.fire(onResult, new StartBattleResult
			{
				Error = StartBattleResult.ErrorCode.BeginCampaignFailed,
				GameServerResponse = response
			});
			yield break;
		}
		if (isMaster)
		{
			battleServerAddress = response.BattleServerAddress;
			battleServerPort = response.BattleServerPort;
			if (string.IsNullOrEmpty(battleServerAddress))
			{
				if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
				{
					NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
				}
				Vespa_System_Action_Extension_Method.fire(onResult, new StartBattleResult
				{
					Error = StartBattleResult.ErrorCode.NoBattleServer,
					GameServerResponse = response
				});
				yield break;
			}
		}
		GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Connect(battleServerAddress, battleServerPort, delegate(bool connected)
		{
			if (!connected)
			{
				if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
				{
					NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
				}
				VespaDebug.LogError("Failed to connect to battle server. addr=" + battleServerAddress + ":" + battleServerPort);
				Vespa_System_Action_Extension_Method.fire(onResult, new StartBattleResult
				{
					Error = StartBattleResult.ErrorCode.BattleServerConnectError,
					GameServerResponse = response
				});
			}
			else
			{
				BattleLoginReq battleLoginReq = new BattleLoginReq
				{
					AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id,
					MatchUid = 0L,
					MasterId = PartyInfo.GetMasterId(),
					GameServerHost = GlobalAutoBehaviour<AccountManager>.instance.ServerHost,
					HeroIndices = heroIndexList.ToArray(),
					ExtraData = new BattleInstanceExtraData
					{
						AffixIndices = PartyInfo.GetAffixIndices()
					}
				};
				if (SingletonAuto<CampaignContext>.isValidInstance && CampaignContext.CheckEachWaveEachTeam(dungeonData.BattleType))
				{
					battleLoginReq.AiHeroIndices = ((!__Extension_Method_ICollection__.IsNullOrEmpty(deltaHeroIndexList)) ? deltaHeroIndexList.ToArray() : null);
				}
				else
				{
					battleLoginReq.AiHeroIndices = ((!__Extension_Method_ICollection__.IsNullOrEmpty(aiHeroIndexList)) ? aiHeroIndexList.ToArray() : null);
				}
				if (isMaster)
				{
					battleLoginReq.UserInfos = partyMemberInfoList.ToArray();
				}
				GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Request(battleLoginReq, delegate(BattleLoginRes res)
				{
					if (res == null || res.Result != BattleLoginRes.ResultType.Success)
					{
						if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
						{
							NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
						}
						VespaDebug.LogError("Failed to BattleLogin : " + ((res != null) ? res.Result.ToString() : "null response"));
						Vespa_System_Action_Extension_Method.fire(onResult, new StartBattleResult
						{
							Error = StartBattleResult.ErrorCode.BattleServerLoginError,
							GameServerResponse = response
						});
					}
					else
					{
						if (IsRepeatBattle)
						{
							if (!NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
							{
								NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.OpenGlobal(_IsPartyPlay: true);
							}
							GlobalAutoBehaviour<PartyManager>.instance.PartyInfo.ResetReady();
						}
						VespaCoroutine.Stop(SceneAutoBehaviour<EndCampaign>.instance.coCheckPartyReadyAll);
						VespaCoroutine.Stop(SceneAutoBehaviour<EndCampaign>.instance.coWaitPartyMember);
						SingletonAuto<BattleContext>.instance.SetCampaign(request, response);
						SingletonAuto<BattleContext>.instance.MoveToBattleScene(LoadingManager.ELoadingType.Default, OnBattleStart);
						GlobalAutoBehaviour<AccountManager>.instance.ApplyBeginCampaignResponse(response);
						if (LobbyScene2.ListReturnState.Count > 0)
						{
							LobbyScene2.ListReturnState.Clear();
						}
						if (raidIndex != 0 && raidLevel != 0)
						{
							LobbyScene2.ListReturnState.Add(new RaidReturnState
							{
								AffixIndices = affixIndices,
								_raidData = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel)
							});
						}
						else if (chapterIndex != 0 && dungeonIndex != 0)
						{
							LobbyScene2.ListReturnState.Add(new MultiReturnState
							{
								_partyDungeonData = GameRoot.Instance.PartyDungeonDataContainer.GetData(chapterIndex, dungeonIndex)
							});
						}
						Vespa_System_Action_Extension_Method.fire(onResult, new StartBattleResult
						{
							Error = StartBattleResult.ErrorCode.None,
							GameServerResponse = response
						});
					}
				});
			}
		});
	}

	private void AddHeroDeck(Dictionary<int, CachedHeroInfo> heroInfoDic, ref List<int> heroIndexList)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(heroInfoDic) || heroInfoDic.Count <= 0)
		{
			return;
		}
		if (heroIndexList == null)
		{
			heroIndexList = new List<int>();
			heroIndexList.Clear();
		}
		foreach (CachedHeroInfo value in heroInfoDic.Values)
		{
			if (value != null && value.HeroIndex != 0)
			{
				heroIndexList.Add(value.HeroIndex);
			}
		}
	}

	private void AddAccountId(List<int> heroIndexList, ref List<long> accountIdList)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(heroIndexList))
		{
			if (accountIdList == null)
			{
				accountIdList = new List<long>();
				accountIdList.Clear();
			}
			for (int i = 0; i < heroIndexList.Count; i++)
			{
				accountIdList.Add(GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
			}
		}
	}

	public void SetPartyInfoForEndBattle(long[] accountIds, int[] heroIndices)
	{
		SingletonAuto<CampaignContext>.instance.AccountIds = accountIds;
		SingletonAuto<CampaignContext>.instance.HeroIndices = heroIndices;
	}

	private void OnBattleStart()
	{
		ChangeMemberStatus(MemberStatus.Battle);
		if (PartyInfo == null || PartyInfo.PartyMembers == null)
		{
			return;
		}
		foreach (MemberInfo partyMember in PartyInfo.PartyMembers)
		{
			partyMember.OnBattleStart();
		}
	}

	public void ProcessStartBattleResult(StartBattleResult result, bool isMaster)
	{
		StartBattleResult.ErrorCode error = result.Error;
		switch (error)
		{
		case StartBattleResult.ErrorCode.None:
			return;
		case StartBattleResult.ErrorCode.NoBattleServer:
			MessagePopup.OpenError(LocalizationManager.GetString(NShared.NLocalization.System.NO_BATTLE_SERVER));
			return;
		}
		switch (error)
		{
		case StartBattleResult.ErrorCode.NoBattleServer:
			MessagePopup.OpenError(LocalizationManager.GetString(PartyBattle.PARTY_BATTLE_CONNECT_FAIL));
			return;
		case StartBattleResult.ErrorCode.BattleServerLoginError:
			MessagePopup.OpenError(LocalizationManager.GetString(PartyBattle.PARTY_BATTLE_LOGIN_FAIL));
			return;
		case StartBattleResult.ErrorCode.HeroDuplicatedNotAllowed:
			MessagePopup.OpenError(LocalizationManager.GetString(PartyBattle.HERO_DUPLICATED_NOT_ALLOWED));
			return;
		}
		if (result.GameServerResponse == null)
		{
			return;
		}
		NShared.BeginCampaign.Response gameServerResponse = result.GameServerResponse;
		if (gameServerResponse.BaseResult != BaseResultType.Success)
		{
			MessagePopup.OpenError(gameServerResponse.BaseResult);
		}
		else if (isMaster && gameServerResponse.Result == NShared.BeginCampaign.ResultType.RaidNotProgressing)
		{
			MessagePopup.OpenError(gameServerResponse.Result, delegate
			{
				SendLeaveParty();
			});
		}
		else
		{
			MessagePopup.OpenError(gameServerResponse.Result);
		}
	}

	public void AddPartyMember(MemberInfo partyMember)
	{
		if (partyMember != null && partyMember.AccountId > 0)
		{
			RemovePartyMember(partyMember.AccountId);
			PartyInfo.PartyMembers.Add(partyMember);
			_partyListenerList.ForEach(delegate(IPartyListener e)
			{
				e.OnPartyMemberJoin(partyMember);
			});
		}
	}

	private bool RemovePartyMember(long accountId)
	{
		if (__Extension_Method_ICollection__.COUNT(PartyInfo.PartyMembers) == 0)
		{
			return false;
		}
		if (PartyInfo.PartyMembers.RemoveAll((MemberInfo e) => e.AccountId == accountId) > 0)
		{
			return true;
		}
		return false;
	}

	public long[] GetPartyMemberIds()
	{
		List<long> list = new List<long>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PartyInfo.PartyMembers); i++)
		{
			list.Add(PartyInfo.PartyMembers[i].AccountId);
		}
		return list.ToArray();
	}

	public MemberInfo GetPartyMemberInfo(long accountId)
	{
		if (!IsValidParty())
		{
			return null;
		}
		return PartyInfo.GetPartyMemberInfo(accountId);
	}

	private MemberInfo CreateMyMemberInfo(bool isPartyMaster)
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		MemberInfo memberInfo = new MemberInfo(GlobalAutoBehaviour<AccountManager>.instance.UserInfo)
		{
			IsMaster = isPartyMaster,
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex
		};
		if (isPartyMaster)
		{
			memberInfo.MemberIndex = 0;
		}
		return memberInfo;
	}

	public MemberInfo GetMyPartyMemberInfo()
	{
		return GetPartyMemberInfo(GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
	}

	public void ProcessPartyMemberLeave(long senderId)
	{
		MemberInfo memberInfo = GetPartyMemberInfo(senderId);
		if (memberInfo == null)
		{
			VespaDebug.LogError("ProcessPartyMemberLeave. member not found : " + senderId);
			return;
		}
		if (!RemovePartyMember(senderId))
		{
			VespaDebug.LogError("ProcessPartyMemberLeave. failed to remove member : " + senderId);
			return;
		}
		CenterMessage.Open(LocalizationManager.GetStringDirect("RAID_USER_OUT_CENTER_MSG", memberInfo.Nick));
		_partyListenerList.ForEach(delegate(IPartyListener e)
		{
			e.OnPartyMemberLeave(memberInfo);
		});
		if (GetMyPartyMemberInfo().IsMaster && PartyInfo.IsRepeatBattle && NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.IsOpen())
		{
			SendRoomRepeat(IsRepeat: false, PartyRoomRepeat.ResultType.CANCEL_LEAVE_PARTY);
			string cancelMessage = string.Format("{0}\n{1}", LocalizationManager.GetStringDirect("RAID_USER_OUT_CENTER_MSG", memberInfo.Nick), LocalizationManager.GetString(NShared.NLocalization.AutoRepeatBattle.AUTO_REPEAT_BATTLE_CANCELED));
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.StopAutoRepeatPartyPlay(cancelMessage, showPopup: true);
		}
	}

	public void ResetParty(bool isMasterLeave = false)
	{
		_partyListenerList.ForEach(delegate(IPartyListener e)
		{
			e.OnPartyDestroy(isMasterLeave);
		});
		PartyInfo.Reset();
		JoinPartyReqHelper = null;
	}

	private void UpdateSendMultiRoomPing()
	{
		if (IsValidParty() && PartyInfo.PartyRoom != null && !((DateTime.UtcNow - _lastSendMultiRoomPingTime).TotalSeconds < (double)RaidRoomPingInterval))
		{
			_lastSendMultiRoomPingTime = DateTime.UtcNow;
			SendMultiRoomPing();
		}
	}

	private void UpdateSendRaidRoomPing()
	{
		if (IsValidParty() && PartyInfo.PartyRoom != null && !((DateTime.UtcNow - _lastSendRaidRoomPingTime).TotalSeconds < (double)RaidRoomPingInterval))
		{
			_lastSendRaidRoomPingTime = DateTime.UtcNow;
			SendRaidRoomPing();
		}
	}

	private void UpdateCheckJoinPartyRes()
	{
		if (JoinPartyReqHelper != null && (DateTime.UtcNow - JoinPartyReqHelper.SendJoinPartyReqTime).TotalSeconds > (double)JoinPartyResWaitTime)
		{
			SendJoinPartyReqCancel(JoinPartyReqHelper.InvitorAccountId, JoinPartyReqCancel.ResultType.TooLate);
			CenterMessage.Open(LocalizationManager.GetString(JoinPartyRes.ResultType.InvalidParty));
			JoinPartyReqHelper = null;
			WaitingPopup.Close(this);
		}
	}

	public void BanUser(long accountId)
	{
		MemberInfo myPartyMemberInfo = GetMyPartyMemberInfo();
		if (myPartyMemberInfo != null && myPartyMemberInfo.IsMaster && myPartyMemberInfo.AccountId != accountId)
		{
			PartyInfo.AddBannedId(accountId);
			SendPartyMemberBanned(accountId);
		}
	}

	public void ChangeMemberStatus(MemberStatus memberStatus)
	{
		ChangePartyMemberStatus changePartyMemberStatus = new ChangePartyMemberStatus();
		changePartyMemberStatus.Status = memberStatus;
		SendToAllMembers(changePartyMemberStatus);
		Handle(GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId, changePartyMemberStatus);
	}

	public bool IsPartyRoomMaster()
	{
		if (PartyInfo == null || PartyInfo.PartyRoom == null)
		{
			return false;
		}
		return PartyInfo.PartyRoom.MasterAccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
	}

	private void ChangePartyMaster(long accountId)
	{
		MemberInfo masterMemberInfo = null;
		foreach (MemberInfo partyMember in PartyInfo.PartyMembers)
		{
			partyMember.IsMaster = partyMember.AccountId == accountId;
			if (partyMember.IsMaster)
			{
				partyMember.IsBattleReady = false;
				masterMemberInfo = partyMember;
			}
		}
		if (PartyInfo.PartyRoom != null)
		{
			PartyInfo.PartyRoom.MasterAccountId = masterMemberInfo.AccountId;
			PartyInfo.PartyRoom.MasterNick = masterMemberInfo.Nick;
		}
		if (IsPartyRoomMaster())
		{
			CenterMessage.Open(LocalizationManager.GetString(Party.PARTY_ROOM_MASTER_CHANGE));
			if (PartyInfo.RaidIndex != 0 && PartyInfo.RaidLevel != 0)
			{
				RaidData data = GameRoot.Instance.RaidDataContainer.GetData(PartyInfo.RaidIndex, PartyInfo.RaidLevel);
				if (data.Type != RaidType.GuildSuppress && data.Type != RaidType.Challenge)
				{
					int raidMaxLevel = RaidHelper.GetRaidMaxLevel(PartyInfo.RaidIndex);
					if (PartyInfo.RaidLevel > raidMaxLevel)
					{
						SendChangeRaidRoomInfo(raidMaxLevel, PartyInfo.PartyRoom.GoalIndex, 0, null);
					}
				}
			}
		}
		SendRoomRepeat(IsRepeat: false);
		_partyListenerList.ForEach(delegate(IPartyListener e)
		{
			e.OnPartyRoomMasterChanged(masterMemberInfo);
		});
	}

	public void SendToMember<T>(long receiverId, T data, Action onSuccess = null, Action onFail = null) where T : class
	{
		SingletonAuto<MessageLogHandler>.instance.SendToClient(new long[1] { receiverId }, data, onSuccess, onFail);
	}

	public void SendToAllMembers<T>(T data) where T : class
	{
		List<long> list = new List<long>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PartyInfo.PartyMembers); i++)
		{
			MemberInfo memberInfo = PartyInfo.PartyMembers[i];
			if (memberInfo.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id)
			{
				list.Add(memberInfo.AccountId);
			}
		}
		SingletonAuto<MessageLogHandler>.instance.SendToClient(list.ToArray(), data);
	}

	public void SendToFriend<T>(T data) where T : class
	{
		SingletonAuto<MessageLogHandler>.instance.SendToFriend(data);
	}

	public void SendToGuild<T>(T data) where T : class
	{
		SingletonAuto<MessageLogHandler>.instance.SendToGuild(data);
	}

	private InvitePartyReq CreateInvitePartyRequest(bool isSendOne)
	{
		int raidIndex = PartyInfo.RaidIndex;
		int raidLevel = PartyInfo.RaidLevel;
		PartyDungeonType dungeonType = PartyInfo.PartyRoom.DungeonType;
		int chapterIndex = PartyInfo.ChapterIndex;
		int dungeonIndex = PartyInfo.DungeonIndex;
		PartyDungeonDifficultyType dungeonDifficulty = PartyInfo.PartyRoom.DungeonDifficulty;
		long[] bannedids = PartyInfo.Bannedids;
		int[] affixIndices = PartyInfo.GetAffixIndices();
		return new InvitePartyReq
		{
			InvitorName = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Nick,
			RaidIndex = raidIndex,
			RaidLevel = raidLevel,
			BannedIds = bannedids,
			RoomNo = PartyInfo.PartyRoom.RoomNo,
			IsSendOne = isSendOne,
			dungeonType = dungeonType,
			chapterIndex = chapterIndex,
			dungeonIndex = dungeonIndex,
			difficulty = dungeonDifficulty,
			AffixIndices = affixIndices
		};
	}

	public void SendInvitePartyRequest(long inviteeId, string inviteeNick)
	{
		if (CanInviteSomeone())
		{
			PartyInfo.RemoveBannedId(inviteeId);
			InvitePartyReq data = CreateInvitePartyRequest(isSendOne: true);
			SendToMember(inviteeId, data);
			CenterMessage.Open(LocalizationManager.GetStringDirect("RAID_INVITE_USER_CENTER_MSG", inviteeNick));
		}
	}

	public void SendInvitePartyRequestToAllFriend()
	{
		if (CanInviteSomeone())
		{
			InvitePartyReq data = CreateInvitePartyRequest(isSendOne: false);
			SendToFriend(data);
			CenterMessage.Open(LocalizationManager.GetStringDirect("RAID_INVITE_FRIEND_USER_CENTER_MSG"));
		}
	}

	public void SendInvitePartyRequestToAllGuild()
	{
		if (CanInviteSomeone())
		{
			InvitePartyReq data = CreateInvitePartyRequest(isSendOne: false);
			SendToGuild(data);
			CenterMessage.Open(LocalizationManager.GetStringDirect("RAID_INVITE_GUILD_MEMBER_CENTER_MSG"));
		}
	}

	public void SendJoinPartyRequest_Accept(long invitorId, long roomNo = 0L)
	{
		DateTime utcNow = DateTime.UtcNow;
		JoinPartyReq joinPartyReq = new JoinPartyReq();
		joinPartyReq.Accepted = true;
		joinPartyReq.Invitee = CreateMyMemberInfo(isPartyMaster: false);
		joinPartyReq.RoomNo = roomNo;
		joinPartyReq.Result = JoinPartyReq.ResultType.Success;
		joinPartyReq.SendKey = utcNow.Ticks;
		SendToMember(invitorId, joinPartyReq);
		WaitingPopup.Open(this);
		JoinPartyReqHelper = new JoinPartyReqHelperClass
		{
			InvitorAccountId = invitorId,
			SendJoinPartyReqTime = utcNow
		};
	}

	public void SendJoinPartyRequest_Reject(long invitorId, JoinPartyReq.ResultType rejectReason = JoinPartyReq.ResultType.Fail)
	{
		JoinPartyReq joinPartyReq = new JoinPartyReq();
		joinPartyReq.Accepted = false;
		joinPartyReq.Invitee = CreateMyMemberInfo(isPartyMaster: false);
		joinPartyReq.Result = rejectReason;
		SendToMember(invitorId, joinPartyReq);
	}

	public void SendJoinPartyReqCancel(long accountId, JoinPartyReqCancel.ResultType resultType)
	{
		JoinPartyReqCancel data = new JoinPartyReqCancel
		{
			Result = resultType
		};
		SendToMember(accountId, data);
	}

	public void SendPingToManster()
	{
		if (IsValidParty() && !IsPartyRoomMaster() && !IsBattleStarted())
		{
			PartyMemberPing data = new PartyMemberPing();
			long masterId = PartyInfo.GetMasterId();
			SendToMember(masterId, data);
		}
	}

	public void SendLeaveParty(bool isDestroyRoom = false)
	{
		if (PartyInfo.PartyRoom != null)
		{
			if (PartyInfo.RaidLevel != 0 && PartyInfo.RaidIndex != 0)
			{
				NShared.LeaveRaidRoom.Request request = new NShared.LeaveRaidRoom.Request
				{
					RoomNo = PartyInfo.PartyRoom.RoomNo,
					AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id,
					RaidRoomDestroyed = isDestroyRoom
				};
				SingletonUpdater<WebServiceManager>.instance.Requester.Request<NShared.LeaveRaidRoom.Request, NShared.LeaveRaidRoom.Response>(request, delegate
				{
				});
			}
			else if (PartyInfo.ChapterIndex != 0 && PartyInfo.DungeonIndex != 0)
			{
				NShared.LeavePartyDungeonRoom.Request request2 = new NShared.LeavePartyDungeonRoom.Request
				{
					RoomNo = PartyInfo.PartyRoom.RoomNo,
					PartyDungeonRoomDestroyed = isDestroyRoom
				};
				SingletonUpdater<WebServiceManager>.instance.Requester.Request<NShared.LeavePartyDungeonRoom.Request, NShared.LeavePartyDungeonRoom.Response>(request2, delegate
				{
				});
			}
			ResetParty(isDestroyRoom);
		}
	}

	public void SendPartyMemberBanned(long bannedId)
	{
		PartyMemberBanned partyMemberBanned = new PartyMemberBanned();
		partyMemberBanned.BannedId = bannedId;
		SendToMember(bannedId, partyMemberBanned);
	}

	public void SendPartyMemberCostumeInfo(int heroIndex)
	{
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex);
		if (cachedHeroInfo != null && IsContainHero(heroIndex))
		{
			PartyDeckCostumeInfo partyDeckCostumeInfo = new PartyDeckCostumeInfo();
			partyDeckCostumeInfo.HeroIndex = heroIndex;
			partyDeckCostumeInfo.CostumeInfo = cachedHeroInfo.GetCostumeInfo();
			SendToAllMembers(partyDeckCostumeInfo);
		}
	}

	public void SendMultiRoomPing()
	{
		if (PartyInfo.RaidIndex == 0 || PartyInfo.RaidLevel == 0)
		{
			NShared.PingPartyDungeonRoom.Request request = new NShared.PingPartyDungeonRoom.Request
			{
				RoomNo = PartyInfo.PartyRoom.RoomNo
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request<NShared.PingPartyDungeonRoom.Request, NShared.PingPartyDungeonRoom.Response>(request, WebRequestFlag.Ping, delegate
			{
			});
		}
	}

	public void SendRaidRoomPing()
	{
		if (PartyInfo.RaidIndex != 0 && PartyInfo.RaidLevel != 0)
		{
			NShared.PingRaidRoom.Request request = new NShared.PingRaidRoom.Request
			{
				RoomNo = PartyInfo.PartyRoom.RoomNo
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request<NShared.PingRaidRoom.Request, NShared.PingRaidRoom.Response>(request, WebRequestFlag.Ping, delegate
			{
			});
		}
	}

	public void AddDeckHero(MemberInfo myMemberInfo, int heroIndex, CachedHeroInfo heroInfo, bool isMain)
	{
		if (IsContainHero(heroIndex, isMain))
		{
			CenterMessage.Open(LocalizationManager.GetString(PartyBattle.HERO_DUPLICATED_NOT_ALLOWED));
			_partyListenerList.ForEach(delegate(IPartyListener e)
			{
				e.OnPartyMemberDeckHeroChanged(myMemberInfo, heroIndex, null, isMain);
			});
		}
		else
		{
			myMemberInfo.SetDeckHero(heroIndex, heroInfo, isMain);
		}
	}

	public void RemoveDeckHero(MemberInfo myMemberInfo, int heroIndex, bool isMain)
	{
		myMemberInfo.SetDeckHero(heroIndex, null, isMain);
	}

	public void SendPartyDeckHeroInfo(int heroIndex, CachedHeroInfo heroInfo, bool isMain)
	{
		MemberInfo myPartyMemberInfo = GetMyPartyMemberInfo();
		if (myPartyMemberInfo != null)
		{
			if (heroInfo == null)
			{
				RemoveDeckHero(myPartyMemberInfo, heroIndex, isMain);
			}
			else
			{
				AddDeckHero(myPartyMemberInfo, heroIndex, heroInfo, isMain);
			}
			PartyDeckHeroInfo partyDeckHeroInfo = new PartyDeckHeroInfo();
			partyDeckHeroInfo.HeroIndex = heroIndex;
			partyDeckHeroInfo.HeroInfo = heroInfo;
			partyDeckHeroInfo.DeckIndex = ((!isMain) ? 1 : 0);
			SendToAllMembers(partyDeckHeroInfo);
		}
	}

	public bool IsContainHero(int heroIndex, bool isMain = true)
	{
		if (!IsValidParty())
		{
			return false;
		}
		foreach (MemberInfo partyMember in PartyInfo.PartyMembers)
		{
			foreach (CachedHeroInfo value in (isMain ? partyMember.DeckHeros : partyMember.SubDeckHeros).Values)
			{
				if (value.HeroIndex == heroIndex)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SendRoomRepeat(bool IsRepeat, PartyRoomRepeat.ResultType resultType = PartyRoomRepeat.ResultType.NONE)
	{
		PartyInfo.SetRepeatBattle(IsRepeat);
		PartyRoomRepeat partyRoomRepeat = new PartyRoomRepeat();
		partyRoomRepeat.IsRepeat = IsRepeat;
		partyRoomRepeat.Result = resultType;
		SendToAllMembers(partyRoomRepeat);
		_partyListenerList.ForEach(delegate(IPartyListener e)
		{
			e.OnPartyRoomSetRepeat(IsRepeat);
		});
	}

	public void SendRoomReady(bool isReady)
	{
		MemberInfo myMemberInfo = GetMyPartyMemberInfo();
		if (myMemberInfo != null)
		{
			myMemberInfo.SetBattleReady(isReady);
			PartyRoomReady partyRoomReady = new PartyRoomReady();
			partyRoomReady.IsReady = isReady;
			SendToAllMembers(partyRoomReady);
			_partyListenerList.ForEach(delegate(IPartyListener e)
			{
				e.OnPartyMemberReady(myMemberInfo, isReady);
			});
		}
	}

	public void SendSelectRewardItem(int rewardItemNo)
	{
		MemberInfo myMemberInfo = GetMyPartyMemberInfo();
		if (myMemberInfo != null)
		{
			myMemberInfo.SelectedRewardItemNo = rewardItemNo;
			PartyMemberSelectReward partyMemberSelectReward = new PartyMemberSelectReward();
			partyMemberSelectReward.RewardItemNo = rewardItemNo;
			SendToAllMembers(partyMemberSelectReward);
			_rewardListenerList.ForEach(delegate(IRewardListener e)
			{
				e.OnSelectRewardItem(myMemberInfo);
			});
		}
	}

	public void SendPartyMemberDungeonInfoReq(long accountId, Action<long, PartyMemberDungeonInfoRes> onInfo)
	{
		_otherDungeonInfoAction = onInfo;
		PartyMemberDungeonInfoReq data = new PartyMemberDungeonInfoReq();
		SendToMember(accountId, data);
	}

	public void ClearPartyMemberDungeonInfoReq()
	{
		_otherDungeonInfoAction = null;
	}

	public void SendChagneMultiRoomSetting()
	{
	}

	public void SendMultiBattleEndToServer()
	{
		if (PartyInfo != null && PartyInfo.PartyRoom != null)
		{
			PartyDungeonType dungeonType = PartyInfo.PartyRoom.DungeonType;
			int chapterIndex = PartyInfo.ChapterIndex;
			int dungeonIndex = PartyInfo.DungeonIndex;
			PartyDungeonDifficultyType difficulty = PartyInfo.Difficulty;
			SendChangeMultiRoomInfo(dungeonType, chapterIndex, dungeonIndex, difficulty, isResetSquard: false);
		}
	}

	public void SendChangeMultiRoomInfo(PartyDungeonType type, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty, bool isResetSquard)
	{
		NShared.ChangePartyDungeonRoomInfo.Request request = new NShared.ChangePartyDungeonRoomInfo.Request
		{
			RoomNo = PartyInfo.PartyRoom.RoomNo,
			DungeonType = type,
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			DungeonDifficulty = difficulty,
			Status = 0
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request<NShared.ChangePartyDungeonRoomInfo.Request, NShared.ChangePartyDungeonRoomInfo.Response>(request, delegate
		{
		});
		ChangePartyDungeonRoomInfo changePartyDungeonRoomInfo = new ChangePartyDungeonRoomInfo();
		changePartyDungeonRoomInfo.ChapterIndex = chapterIndex;
		changePartyDungeonRoomInfo.DungeonIndex = dungeonIndex;
		changePartyDungeonRoomInfo.Difficulty = difficulty;
		changePartyDungeonRoomInfo.DungeonType = type;
		changePartyDungeonRoomInfo.IsResetSquard = isResetSquard;
		SendToAllMembers(changePartyDungeonRoomInfo);
		Handle(GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id, changePartyDungeonRoomInfo);
	}

	public void SendChagneRaidRoomSetting(int raidLevel, byte goalIndex, int[] affixIndices, bool isViolateBanRule = false)
	{
		SendChangeRaidRoomInfo(raidLevel, goalIndex, 0, affixIndices, isViolateBanRule);
	}

	public void SendRaidBattleEndToServer()
	{
		if (PartyInfo != null && PartyInfo.PartyRoom != null)
		{
			int raidLevel = PartyInfo.RaidLevel;
			byte goalIndex = PartyInfo.PartyRoom.GoalIndex;
			int[] affixIndices = PartyInfo.GetAffixIndices();
			SendChangeRaidRoomInfo(raidLevel, goalIndex, 0, affixIndices);
		}
	}

	public void SendChangeRaidRoomInfo(int raidLevel, byte goalIndex, byte status, int[] affixIndices, bool isViolateBanRule = false)
	{
		int raidBossLevel = RaidHelper.GetRaidBossLevel(PartyInfo.RaidIndex, raidLevel);
		NShared.ChangeRaidRoomInfo.Request request = new NShared.ChangeRaidRoomInfo.Request
		{
			RoomNo = PartyInfo.PartyRoom.RoomNo,
			RaidLevel = raidLevel,
			RaidMonsterLevel = raidBossLevel,
			GoalIndex = goalIndex,
			Status = status,
			AffixIndices = affixIndices
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request<NShared.ChangeRaidRoomInfo.Request, NShared.ChangeRaidRoomInfo.Response>(request, delegate
		{
		});
		ChangePartyRoomInfo data = new ChangePartyRoomInfo
		{
			RaidLevel = raidLevel,
			GoalIndex = goalIndex,
			IsViolateBanRule = isViolateBanRule,
			AffixIndices = affixIndices
		};
		SendToAllMembers(data);
		SendRoomRepeat(IsRepeat: false);
		Handle(GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id, data);
	}

	public void SendNewPartyRoomMaster(long newMasterAccountId)
	{
		if (PartyInfo != null)
		{
			NShared.DelegateRaidRoomMaster.Request request = new NShared.DelegateRaidRoomMaster.Request
			{
				RoomNo = PartyInfo.PartyRoom.RoomNo,
				CurMasterId = newMasterAccountId
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request<NShared.DelegateRaidRoomMaster.Request, NShared.DelegateRaidRoomMaster.Response>(request, delegate
			{
			});
			ChangePartyRoomMaster data = new ChangePartyRoomMaster
			{
				NewMasterAccountId = newMasterAccountId
			};
			SendToAllMembers(data);
			ChangePartyMaster(newMasterAccountId);
		}
	}

	public static bool IsOpenedRaid()
	{
		return SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.Raid);
	}

	private void Handle(long senderId, InvitePartyReq data)
	{
		if (!InviteNotifyManager.IsOpenableNoty(senderId))
		{
			SendJoinPartyRequest_Reject(senderId);
			return;
		}
		if (data.BannedIds != null && TypeIterator.Contains(data.BannedIds, GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId))
		{
			SendJoinPartyRequest_Reject(senderId);
			return;
		}
		JoinPartyReq.ResultType resultType = CanJoinInvitedParty(data);
		if (resultType != JoinPartyReq.ResultType.Success)
		{
			SendJoinPartyRequest_Reject(senderId, resultType);
			return;
		}
		_newPartyListenerList.ForEach(delegate(INewPartyListener e)
		{
			e.OnPartyInvited(senderId, data);
		});
	}

	private JoinPartyReq.ResultType CanJoinInvitedParty(InvitePartyReq data)
	{
		if (data.IsRaid())
		{
			return CanJoinRaid(data);
		}
		if (data.IsPartyDungeon())
		{
			return CanJoinPartyDungeon(data);
		}
		return JoinPartyReq.ResultType.Fail;
	}

	private JoinPartyReq.ResultType CanJoinRaid(InvitePartyReq data)
	{
		RaidData data2 = GameRoot.Instance.RaidDataContainer.GetData(data.RaidIndex, data.RaidLevel);
		if (data2 == null)
		{
			return JoinPartyReq.ResultType.Fail;
		}
		if (data2.Type == RaidType.GuildSuppress)
		{
			if (GuildManager.GuildValidGuildPenaltyType() != GuildPenaltyType.None)
			{
				return JoinPartyReq.ResultType.Fail;
			}
			return JoinPartyReq.ResultType.Success;
		}
		if (!IsOpenedRaid())
		{
			return JoinPartyReq.ResultType.Fail;
		}
		if (data2.Type == RaidType.DragonHard)
		{
			if (!RaidHelper.CanEnterHardMode())
			{
				JoinPartyReq.ResultType result = JoinPartyReq.ResultType.Fail;
				if (data.IsSendOne)
				{
					result = JoinPartyReq.ResultType.JoinPartyReq_NotOpenedHardMode;
				}
				return result;
			}
		}
		else if (data2.Type == RaidType.TechnoMagic)
		{
			if (!RaidHelper.CanEnterTechnoMagicRaid())
			{
				JoinPartyReq.ResultType result2 = JoinPartyReq.ResultType.Fail;
				if (data.IsSendOne)
				{
					result2 = JoinPartyReq.ResultType.JoinPartyReq_NotOpenedTechnoMagic;
				}
				return result2;
			}
		}
		else if (data2.Type == RaidType.FieldRaid && !RaidHelper.CanEnterFieldRaid())
		{
			JoinPartyReq.ResultType result3 = JoinPartyReq.ResultType.Fail;
			if (data.IsSendOne)
			{
				result3 = JoinPartyReq.ResultType.JoinPartyReq_NotOpenedFieldRaid;
			}
			return result3;
		}
		return JoinPartyReq.ResultType.Success;
	}

	private JoinPartyReq.ResultType CanJoinPartyDungeon(InvitePartyReq data)
	{
		if (!IsOpenedRaid())
		{
			return JoinPartyReq.ResultType.Fail;
		}
		return JoinPartyReq.ResultType.Success;
	}

	private void Handle(long senderId, JoinPartyReq data)
	{
		if (!data.Accepted)
		{
			switch (data.Result)
			{
			case JoinPartyReq.ResultType.JoinPartyReq_NotOpenedHardMode:
				CenterMessage.OpenError(Party.PARTY_NOT_OPENED_HARD_MODE);
				break;
			case JoinPartyReq.ResultType.JoinPartyReq_NotOpenedChallengeRaidDiff:
				CenterMessage.OpenError(Party.PARTY_CHALLENGERAID_NOT_OPENED_DIFFICULTY);
				break;
			case JoinPartyReq.ResultType.JoinPartyReq_NotOpenedTechnoMagic:
			case JoinPartyReq.ResultType.JoinPartyReq_NotOpenedFieldRaid:
				CenterMessage.OpenError(Party.PARTY_NOT_OPENED_RAID);
				break;
			}
			return;
		}
		JoinPartyRes joinPartyRes = new JoinPartyRes();
		joinPartyRes.SendKey = data.SendKey;
		if (!PartyInfo.IsValid() || IsBattleStarted() || PartyInfo.PartyRoom.RoomNo != data.RoomNo)
		{
			joinPartyRes.Result = JoinPartyRes.ResultType.InvalidParty;
			VespaDebug.LogError("JoinPartyReq received but party is invalid.");
			SendToMember(senderId, joinPartyRes);
			return;
		}
		if (GetPartyMemberInfo(data.Invitee.AccountId) != null)
		{
			joinPartyRes.Result = JoinPartyRes.ResultType.AlreadyJoined;
			VespaDebug.LogError("JoinPartyReq received but AlreadyJoined.");
			SendToMember(senderId, joinPartyRes);
			return;
		}
		if (PartyInfo.Bannedids != null && TypeIterator.Contains(PartyInfo.Bannedids, senderId))
		{
			joinPartyRes.Result = JoinPartyRes.ResultType.Banned;
			VespaDebug.LogError("JoinPartyReq received but banned member.");
			SendToMember(senderId, joinPartyRes);
			return;
		}
		if (!CanInviteSomeone())
		{
			joinPartyRes.Result = JoinPartyRes.ResultType.PartyFull;
			VespaDebug.LogError("JoinPartyReq received but party is full.");
			SendToMember(senderId, joinPartyRes);
			return;
		}
		data.Invitee.MemberIndex = SetJoinPartyMemberIndex(PartyInfo);
		AddPartyMember(data.Invitee);
		CenterMessage.Open(LocalizationManager.GetStringDirect("RAID_JOIN_NEW_USER_CENTER_MSG", data.Invitee.Nick));
		joinPartyRes.Result = JoinPartyRes.ResultType.Success;
		joinPartyRes.PartyInfo = PartyInfo;
		SendToMember(senderId, joinPartyRes);
		JoinPartyNotice data2 = new JoinPartyNotice
		{
			PartyInfo = PartyInfo
		};
		List<MemberInfo> partyMembers = PartyInfo.PartyMembers;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(partyMembers); i++)
		{
			if (partyMembers[i].AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId && partyMembers[i].AccountId != data.Invitee.AccountId)
			{
				SendToMember(partyMembers[i].AccountId, data2);
			}
		}
	}

	private void Handle(long senderId, JoinPartyRes data)
	{
		if (JoinPartyReqHelper == null || JoinPartyReqHelper.SendJoinPartyReqTime.Ticks != data.SendKey)
		{
			return;
		}
		JoinPartyReqHelper = null;
		bool flag = true;
		if (data.Result != JoinPartyRes.ResultType.Success)
		{
			string text = "";
			if (data.Result == JoinPartyRes.ResultType.Banned)
			{
				text = LocalizationManager.GetString(Party.PARTY_JOIN_BANNED_ROOM);
			}
			else if (data.Result != JoinPartyRes.ResultType.AlreadyJoined)
			{
				CheckRetryAutoSearch();
				text = LocalizationManager.GetString(data.Result);
			}
			if (!String_Extension_Method.IsNullOrEmpty(text))
			{
				CenterMessage.Open(text);
			}
			VespaDebug.LogError("JoinPartyRes : Result = " + data.Result);
			flag = false;
		}
		else if (data.PartyInfo == null)
		{
			CenterMessage.Open(LocalizationManager.GetString(Party.PARTY_CANNOT_JOIN));
			VespaDebug.LogError("JoinPartyRes : PartyInfo is null");
			flag = false;
		}
		else if (data.PartyInfo.RaidIndex > 0 && data.PartyInfo.RaidLevel > 0 && !RaidHelper.CanEnterRaid(GameRoot.Instance.RaidDataContainer.GetData(data.PartyInfo.RaidIndex, data.PartyInfo.RaidLevel)))
		{
			CenterMessage.Open(LocalizationManager.GetString(Party.PARTY_CHALLENGERAID_NOT_OPENED_DIFFICULTY));
			flag = false;
		}
		if (!flag)
		{
			WaitingPopup.Close(this);
			return;
		}
		bool num = data.PartyInfo.RaidIndex > 0 && data.PartyInfo.RaidLevel > 0;
		bool flag2 = data.PartyInfo.ChapterIndex > 0 && data.PartyInfo.DungeonIndex > 0;
		PartyInfo = data.PartyInfo;
		WaitingPopup.Close(this);
		if (num)
		{
			_newPartyListenerList.ForEach(delegate(INewPartyListener e)
			{
				e.OnPartyCreated();
			});
			NShared.JoinRaidRoom.Request request = new NShared.JoinRaidRoom.Request
			{
				RoomNo = data.PartyInfo.PartyRoom.RoomNo
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.JoinRaidRoom.Response response)
			{
				if (response.Result != NShared.JoinRaidRoom.ResultType.Success)
				{
					long masterId = data.PartyInfo.GetMasterId();
					SendJoinPartyReqCancel(masterId, JoinPartyReqCancel.ResultType.ServerFailed);
					CenterMessage.Open(LocalizationManager.GetString(response.Result));
					ResetParty();
				}
			});
		}
		else
		{
			if (!flag2)
			{
				return;
			}
			VespaCoroutine.Start(coCheckOpenPartyPlay(delegate
			{
				_newPartyListenerList.ForEach(delegate(INewPartyListener e)
				{
					e.OnPartyCreated();
				});
			}));
			NShared.JoinPartyDungeonRoom.Request request2 = new NShared.JoinPartyDungeonRoom.Request
			{
				RoomNo = data.PartyInfo.PartyRoom.RoomNo
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request2, delegate(NShared.JoinPartyDungeonRoom.Response response)
			{
				if (response.Result != NShared.JoinPartyDungeonRoom.ResultType.Success)
				{
					long masterId = data.PartyInfo.GetMasterId();
					SendJoinPartyReqCancel(masterId, JoinPartyReqCancel.ResultType.ServerFailed);
					CenterMessage.Open(LocalizationManager.GetString(response.Result));
					ResetParty();
				}
			});
		}
	}

	private void Handle(long senderId, JoinPartyNotice data)
	{
		PartyInfo partyInfo = data.PartyInfo;
		if (partyInfo == null || IsPartyRoomMaster())
		{
			return;
		}
		List<MemberInfo> partyMembers = partyInfo.PartyMembers;
		if (partyMembers == null)
		{
			return;
		}
		foreach (MemberInfo item in partyMembers)
		{
			if (PartyInfo.GetPartyMemberInfo(item.AccountId) == null)
			{
				AddPartyMember(item);
				CenterMessage.Open(LocalizationManager.GetStringDirect("RAID_JOIN_NEW_USER_CENTER_MSG", item.Nick));
			}
		}
	}

	private void Handle(long senderId, LeaveParty data)
	{
		ProcessPartyMemberLeave(senderId);
	}

	private void Handle(long senderId, PartyMemberBanned data)
	{
		SendLeaveParty();
		if (data.BannedId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id)
		{
			CenterMessage.Open(LocalizationManager.GetString(Party.PARTY_BANNED_MSG));
		}
	}

	private void Handle(long senderId, PartyMemberChangeAvatar data)
	{
		MemberInfo member = GetPartyMemberInfo(senderId);
		if (member != null)
		{
			member.AvatarHeroIndex = data.HeroIndex;
			_partyListenerList.ForEach(delegate(IPartyListener e)
			{
				e.OnPartyMemberChangeAvatar(member);
			});
		}
	}

	private void Handle(long senderId, PartyMemberSelectReward data)
	{
		MemberInfo member = GetPartyMemberInfo(senderId);
		if (member != null)
		{
			member.SelectedRewardItemNo = data.RewardItemNo;
			_rewardListenerList.ForEach(delegate(IRewardListener e)
			{
				e.OnSelectRewardItem(member);
			});
		}
	}

	private void Handle(long senderId, PartyDeckCostumeInfo data)
	{
		if (data == null || !PartyInfo.IsValid())
		{
			return;
		}
		MemberInfo member = GetPartyMemberInfo(senderId);
		if (member != null)
		{
			int heroIndex = data.HeroIndex;
			CachedHeroCostumeInfo costumeInfo = data.CostumeInfo;
			CachedHeroInfo cachedHeroInfo = member.SetDeckHeroCostume(heroIndex, costumeInfo);
			_partyListenerList.ForEach(delegate(IPartyListener e)
			{
				e.OnPartyMemberDeckCostumeChanged(member.AccountId, heroIndex, cachedHeroInfo);
			});
		}
	}

	private void Handle(long senderId, PartyDeckHeroInfo data)
	{
		if (!PartyInfo.IsValid())
		{
			return;
		}
		MemberInfo member = GetPartyMemberInfo(senderId);
		if (member == null)
		{
			return;
		}
		MemberInfo myPartyMemberInfo = GetMyPartyMemberInfo();
		if (myPartyMemberInfo == null)
		{
			return;
		}
		if (myPartyMemberInfo.IsMaster && data.HeroIndex > 0 && data.IsAdd)
		{
			PartyDeckHeroCancel.ResultType resultType = PartyDeckHeroCancel.ResultType.DECK_CANCEL_NONE;
			if (IsFullHero())
			{
				resultType = PartyDeckHeroCancel.ResultType.DECK_CANCEL_FULL;
			}
			else if (IsContainHero(data.HeroIndex))
			{
				resultType = PartyDeckHeroCancel.ResultType.DECK_CANCEL_CONTAIN;
			}
			if (resultType != PartyDeckHeroCancel.ResultType.DECK_CANCEL_NONE)
			{
				PartyDeckHeroCancel partyDeckHeroCancel = new PartyDeckHeroCancel();
				partyDeckHeroCancel.MemberId = senderId;
				partyDeckHeroCancel.HeroIndex = data.HeroIndex;
				partyDeckHeroCancel.Result = resultType;
				partyDeckHeroCancel.DeckIndex = data.DeckIndex;
				SendToAllMembers(partyDeckHeroCancel);
				return;
			}
		}
		CachedHeroInfo heroInfo = data.HeroInfo;
		int heroIndex = data.HeroIndex;
		bool isMain = data.DeckIndex == 0;
		if (data.IsAdd)
		{
			member.SetDeckHero(heroIndex, heroInfo, isMain);
			SetEventDeckInsert(heroInfo, isMain);
		}
		else
		{
			member.SetDeckHero(heroIndex, null, isMain);
			SetEventDeckRemove(heroIndex);
		}
		_partyListenerList.ForEach(delegate(IPartyListener e)
		{
			e.OnPartyMemberDeckHeroChanged(member, heroIndex, heroInfo, isMain);
		});
	}

	public void SetEventDeckInsert(CachedHeroInfo insertHeroInfo, bool isMain)
	{
		IEvent triggerEvent = new BattleInsertDeck
		{
			IsMain = isMain,
			InsertHeroInfo = insertHeroInfo
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	public void SetEventDeckRemove(int removeHeroIndex)
	{
		IEvent triggerEvent = new BattleRemoveDeck
		{
			RemoveHeroIndex = removeHeroIndex
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	private void Handle(long senderId, PartyDeckHeroCancel data)
	{
		if (!PartyInfo.IsValid())
		{
			return;
		}
		MemberInfo member = GetPartyMemberInfo(data.MemberId);
		if (member != null)
		{
			if (data.MemberId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id && data.Result == PartyDeckHeroCancel.ResultType.DECK_CANCEL_CONTAIN)
			{
				CenterMessage.Open(LocalizationManager.GetString(PartyBattle.HERO_DUPLICATED_NOT_ALLOWED));
			}
			bool isMain = data.DeckIndex == 0;
			member.SetDeckHero(data.HeroIndex, null, isMain);
			_partyListenerList.ForEach(delegate(IPartyListener e)
			{
				e.OnPartyMemberDeckHeroChanged(member, data.HeroIndex, null, isMain);
			});
		}
	}

	private void Handle(long senderId, PartyRoomReady data)
	{
		if (!PartyInfo.IsValid())
		{
			return;
		}
		MemberInfo member = GetPartyMemberInfo(senderId);
		if (member != null)
		{
			member.SetBattleReady(data.IsReady);
			_partyListenerList.ForEach(delegate(IPartyListener e)
			{
				e.OnPartyMemberReady(member, data.IsReady);
			});
		}
	}

	private void Handle(long senderId, PartyBattleStart data)
	{
		if (!PartyInfo.IsValid())
		{
			return;
		}
		PartyInfo.SetCampaignInfo(data.MasterServerAddress, data.MasterServerPort, data.RewardItems);
		string battleServerAddress = data.BattleServerAddress;
		int battleServerPort = data.BattleServerPort;
		GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
		VespaCoroutine.Start(this, coRequestBeginMultiplayCampaign(battleServerAddress, battleServerPort, data.MasterServerAddress, data.MasterServerPort, data.RaidIndex, data.RaidLevel, SingletonAuto<CampaignContext>.instance.OnetimeBoosterCount, delegate(StartBattleResult result)
		{
			if (result.Error == StartBattleResult.ErrorCode.None)
			{
				SingletonAuto<CampaignContext>.instance.SetRewardItems(data.RewardItems);
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(data.AccountIds) && !__Extension_Method_ICollection__.IsNullOrEmpty(data.HeroIndices))
				{
					SetPartyInfoForEndBattle(data.AccountIds, data.HeroIndices);
				}
			}
			else
			{
				ProcessStartBattleResult(result, isMaster: false);
			}
		}));
	}

	private void Handle(long senderId, PartyMemberDungeonInfoReq data)
	{
		bool flag = IsValidParty();
		bool flag2 = CanInviteSomeone();
		PartyMemberDungeonInfoRes partyMemberDungeonInfoRes = new PartyMemberDungeonInfoRes();
		partyMemberDungeonInfoRes.CanJoin = flag && flag2;
		if (flag)
		{
			partyMemberDungeonInfoRes.RoomNo = PartyInfo.PartyRoom.RoomNo;
			partyMemberDungeonInfoRes.RaidIndex = PartyInfo.RaidIndex;
			partyMemberDungeonInfoRes.RaidLevel = PartyInfo.RaidLevel;
			partyMemberDungeonInfoRes.ChapterIndex = PartyInfo.ChapterIndex;
			partyMemberDungeonInfoRes.DungeonIndex = PartyInfo.DungeonIndex;
		}
		SendToMember(senderId, partyMemberDungeonInfoRes);
	}

	private void Handle(long senderId, PartyMemberDungeonInfoRes data)
	{
		if (_otherDungeonInfoAction != null)
		{
			Action<long, PartyMemberDungeonInfoRes> otherDungeonInfoAction = _otherDungeonInfoAction;
			_otherDungeonInfoAction = null;
			Vespa_System_Action_Extension_Method.fire(otherDungeonInfoAction, senderId, data);
		}
	}

	private void Handle(long senderId, ChangePartyMemberStatus data)
	{
		MemberInfo partyMember = GetPartyMemberInfo(senderId);
		if (partyMember != null)
		{
			partyMember.Status = data.Status;
			_partyListenerList.ForEach(delegate(IPartyListener e)
			{
				e.OnChangePartyMemberStatus(partyMember);
			});
		}
	}

	private void Handle(long senderId, ChangePartyDungeonRoomInfo data)
	{
		if (PartyInfo == null)
		{
			return;
		}
		bool flag = false;
		if (PartyInfo.ChapterIndex != data.ChapterIndex || PartyInfo.DungeonIndex != data.DungeonIndex || PartyInfo.Difficulty != data.Difficulty)
		{
			flag = true;
		}
		PartyInfo.ChapterIndex = data.ChapterIndex;
		PartyInfo.DungeonIndex = data.DungeonIndex;
		PartyInfo.Difficulty = data.Difficulty;
		if (PartyInfo.PartyRoom != null)
		{
			PartyInfo.PartyRoom.DungeonType = data.DungeonType;
			PartyInfo.PartyRoom.ChapterIndex = data.ChapterIndex;
			PartyInfo.PartyRoom.DungeonIndex = data.DungeonIndex;
			PartyInfo.PartyRoom.DungeonDifficulty = data.Difficulty;
		}
		if (flag)
		{
			PartyRoomReady data2 = new PartyRoomReady
			{
				IsReady = false
			};
			foreach (MemberInfo partyMember in PartyInfo.PartyMembers)
			{
				if (!partyMember.IsMaster)
				{
					Handle(partyMember.AccountId, data2);
				}
			}
		}
		if (data.IsResetSquard)
		{
			foreach (MemberInfo member in PartyInfo.PartyMembers)
			{
				if (__Extension_Method_ICollection__.COUNT(member.DeckHeros) < 0 && __Extension_Method_ICollection__.COUNT(member.SubDeckHeros) < 0)
				{
					return;
				}
				foreach (KeyValuePair<int, CachedHeroInfo> squardHero in member.DeckHeros)
				{
					_partyListenerList.ForEach(delegate(IPartyListener e)
					{
						e.OnPartyMemberDeckHeroChanged(member, squardHero.Value.HeroIndex, null);
					});
				}
				foreach (KeyValuePair<int, CachedHeroInfo> squardHero2 in member.SubDeckHeros)
				{
					_partyListenerList.ForEach(delegate(IPartyListener e)
					{
						e.OnPartyMemberDeckHeroChanged(member, squardHero2.Value.HeroIndex, null);
					});
				}
				member.DeckHeros.Clear();
				member.SubDeckHeros.Clear();
			}
		}
		_partyListenerList.ForEach(delegate(IPartyListener e)
		{
			e.OnPartyRoomInfoUpdated(PartyInfo.PartyRoom, isViolateBanRule: false);
		});
	}

	private void Handle(long senderId, ChangePartyRoomInfo data)
	{
		if (PartyInfo == null)
		{
			return;
		}
		int[] affixIndices = PartyInfo.GetAffixIndices();
		int[] affixIndices2 = data.AffixIndices;
		bool flag = false;
		if (PartyInfo.RaidLevel != data.RaidLevel)
		{
			flag = true;
		}
		if (!AffixHelper.IsEqualAffixIndices(affixIndices, affixIndices2))
		{
			flag = true;
		}
		SetRaid(PartyInfo.RaidIndex, data.RaidLevel);
		if (PartyInfo.PartyRoom != null)
		{
			int raidBossLevel = RaidHelper.GetRaidBossLevel(PartyInfo.RaidIndex, data.RaidLevel);
			PartyInfo.PartyRoom.RaidLevel = data.RaidLevel;
			PartyInfo.PartyRoom.RaidMonsterLevel = raidBossLevel;
			PartyInfo.PartyRoom.GoalIndex = data.GoalIndex;
			PartyInfo.PartyRoom.AffixIndices = affixIndices2;
		}
		if (flag)
		{
			PartyRoomReady data2 = new PartyRoomReady
			{
				IsReady = false
			};
			foreach (MemberInfo partyMember in PartyInfo.PartyMembers)
			{
				if (!partyMember.IsMaster)
				{
					Handle(partyMember.AccountId, data2);
				}
			}
		}
		_partyListenerList.ForEach(delegate(IPartyListener e)
		{
			e.OnPartyRoomInfoUpdated(PartyInfo.PartyRoom, data.IsViolateBanRule);
		});
	}

	private void Handle(long senderId, ChangePartyRoomMaster data)
	{
		if (PartyInfo != null)
		{
			ChangePartyMaster(data.NewMasterAccountId);
		}
	}

	private void Handle(long senderId, JoinPartyReqCancel data)
	{
		if (PartyInfo.GetPartyMemberInfo(senderId) != null)
		{
			ProcessPartyMemberLeave(senderId);
			KickedRaidMemberRes kickedRaidMemberRes = new KickedRaidMemberRes();
			kickedRaidMemberRes.KickedAccountIds = new long[1] { senderId };
			KickedRaidMemberRes data2 = kickedRaidMemberRes;
			SendToAllMembers(data2);
		}
	}

	private void Handle(long senderId, KickedRaidMemberRes data)
	{
		long accountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		if (TypeIterator.Contains(data.KickedAccountIds, accountId))
		{
			ResetParty();
			return;
		}
		long[] kickedAccountIds = data.KickedAccountIds;
		foreach (long num in kickedAccountIds)
		{
			MemberInfo partyMemberInfo = GetPartyMemberInfo(num);
			if (partyMemberInfo != null)
			{
				if (partyMemberInfo.IsMaster && data.ReplaceMasterAccountId != 0L)
				{
					ChangePartyMaster(data.ReplaceMasterAccountId);
				}
				ProcessPartyMemberLeave(num);
			}
		}
	}

	private void Handle(long senderId, InvalidPartyMember data)
	{
		SendLeaveParty();
	}

	private void Handle(long senderId, RaidRoomDestroyed data)
	{
		SendLeaveParty(isDestroyRoom: true);
	}

	private void Handle(long senderId, MemberResultsRes data)
	{
		MemberInfo member = GetPartyMemberInfo(senderId);
		if (member != null)
		{
			member.SetMemberResults(data);
			_partyListenerList.ForEach(delegate(IPartyListener e)
			{
				e.OnPartyMemberResults(member);
			});
		}
	}

	private void Handle(long senderId, PartyRoomRepeat data)
	{
		PartyInfo.SetRepeatBattle(data.IsRepeat);
		_partyListenerList.ForEach(delegate(IPartyListener e)
		{
			e.OnPartyRoomSetRepeat(data.IsRepeat);
		});
		SendRoomReady(isReady: false);
		GetMyPartyMemberInfo();
		if (data.IsRepeat || !NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
		{
			return;
		}
		VespaCoroutine.Stop(SceneAutoBehaviour<EndCampaign>.instance.coCheckPartyReadyAll);
		VespaCoroutine.Stop(SceneAutoBehaviour<EndCampaign>.instance.coWaitPartyMember);
		MemberInfo partyMemberInfo = GetPartyMemberInfo(senderId);
		if (partyMemberInfo != null)
		{
			string empty = string.Empty;
			bool flag = false;
			switch (data.Result)
			{
			case PartyRoomRepeat.ResultType.NONE:
				flag = true;
				empty = LocalizationManager.GetString(NShared.NLocalization.AutoRepeatBattle.AUTO_REPEAT_BATTLE_CANCELED);
				break;
			case PartyRoomRepeat.ResultType.CANCEL_MASTER:
				flag = false;
				empty = LocalizationManager.GetString(NShared.NLocalization.AutoRepeatBattle.AUTO_REPEAT_BATTLE_CANCELED);
				break;
			case PartyRoomRepeat.ResultType.CANCEL_STAMINA:
				flag = true;
				empty = LocalizationManager.GetStringDirect("REPEATBATTLE_MULTI_CANCEL", partyMemberInfo.Nick);
				break;
			case PartyRoomRepeat.ResultType.CANCEL_IVENTORY:
				flag = true;
				empty = LocalizationManager.GetStringDirect("REPEATBATTLE_MULTI_CANCEL", partyMemberInfo.Nick);
				break;
			case PartyRoomRepeat.ResultType.CANCEL_LEAVE_PARTY:
				flag = true;
				empty = string.Format("{0}\n{1}", LocalizationManager.GetStringDirect("RAID_USER_OUT_CENTER_MSG", partyMemberInfo.Nick), LocalizationManager.GetString(NShared.NLocalization.AutoRepeatBattle.AUTO_REPEAT_BATTLE_CANCELED));
				break;
			default:
				flag = true;
				empty = LocalizationManager.GetString(NShared.NLocalization.AutoRepeatBattle.AUTO_REPEAT_BATTLE_CANCELED);
				break;
			}
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.StopAutoRepeatPartyPlay(empty, flag);
		}
	}

	public NShared.SearchPartyDungeonRoom.Request PveSearchPartyCreatePacket(int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty, PartyDungeonType dungeonType)
	{
		return new NShared.SearchPartyDungeonRoom.Request
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			DungeonDifficulty = difficulty,
			DungeonType = dungeonType
		};
	}

	public NShared.WaitPartyDungeonRoom.Request PveWaitPartyCreatePacket(int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty, PartyDungeonType dungeonType)
	{
		return new NShared.WaitPartyDungeonRoom.Request
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			DungeonDifficulty = difficulty,
			DungeonType = dungeonType
		};
	}

	public void PveSearchPartyStart(PartyDungeonType type, int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty)
	{
		NShared.SearchPartyDungeonRoom.Request req = PveSearchPartyCreatePacket(chapterIndex, dungeonIndex, difficulty, type);
		InitSearch(req);
	}

	public void PveSearchPartyStart(int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty)
	{
		PveSearchPartyStart(GameRoot.Instance.PartyDungeonDataContainer.GetData(chapterIndex, dungeonIndex)?.DungeonType ?? PartyDungeonType.Campaign, chapterIndex, dungeonIndex, difficulty);
	}

	private void InitSearch(NShared.SearchPartyDungeonRoom.Request req)
	{
		GlobalAutoBehaviour<PartyAutoSearchManager>.instance.InitSearch(req, delegate(NShared.SearchPartyDungeonRoom.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				CheckRetryAutoSearch();
			}
			else if (res.Result != NShared.SearchPartyDungeonRoom.ResultType.Success)
			{
				CheckRetryAutoSearch();
			}
			else
			{
				int num = GameRoot.Instance.ConstantDataContainer.GetValueInt("PartySearchMinimumRoom");
				if (num < 1)
				{
					num = 1;
				}
				if (__Extension_Method_ICollection__.COUNT(res.RoomInfos) < num)
				{
					GlobalAutoBehaviour<PartyAutoSearchManager>.instance.SetInterval(res.RepeatCoolTime);
				}
				else
				{
					GlobalAutoBehaviour<PartyAutoSearchManager>.instance.PauseSearch();
					string stringDirect = LocalizationManager.GetStringDirect("PARTY_SEARCH");
					string stringDirect2 = LocalizationManager.GetStringDirect("PARTY_SEARCH_SUCCESS");
					MessagePopup3Button.OpenGlobal(stringDirect, stringDirect2, delegate
					{
						PveRequestWaitPartyDungeon(req.ChapterIndex, req.DungeonIndex, req.DungeonDifficulty, req.DungeonType);
					}, delegate
					{
						GlobalAutoBehaviour<PartyAutoSearchManager>.instance.CloseSearch();
					}, NShared.NLocalization.Common.COMMON_CONFIRM, NShared.NLocalization.Common.COMMON_CANCEL);
				}
			}
		});
	}

	public void PveRequestWaitPartyDungeon(int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty, PartyDungeonType dungeonType)
	{
		NShared.WaitPartyDungeonRoom.Request request = PveWaitPartyCreatePacket(chapterIndex, dungeonIndex, difficulty, dungeonType);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.WaitPartyDungeonRoom.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				CheckRetryAutoSearch();
			}
			else if (res.Result != NShared.WaitPartyDungeonRoom.ResultType.Success)
			{
				CheckRetryAutoSearch();
			}
			else if (res.JoinedRaidRoomInfo == null || res.JoinedRaidRoomInfo.RoomNo == 0L)
			{
				CheckRetryAutoSearch();
			}
			else
			{
				SendJoinPartyRequest_Accept(res.JoinedRaidRoomInfo.MasterAccountId, res.JoinedRaidRoomInfo.RoomNo);
			}
		});
	}

	public void CheckRetryAutoSearch()
	{
		if (GlobalAutoBehaviour<PartyAutoSearchManager>.instance.DoAutoSearch)
		{
			GlobalAutoBehaviour<PartyAutoSearchManager>.instance.ShowCenterMessageJoinRoomFail();
			GlobalAutoBehaviour<PartyAutoSearchManager>.instance.ResumeSearch();
		}
	}

	private IEnumerator coCheckOpenPartyPlay(Action action)
	{
		GlobalAutoBehaviour<PartyAutoSearchManager>.instance.CloseSearch();
		if (AutoNextBattle.IsValidInstance)
		{
			AutoNextBattle.instance.OnClickStop();
		}
		if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsValidInstance)
		{
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.instance.OnClickStop(sendRoomRepeat: true);
		}
		if (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			while (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
			{
				yield return null;
			}
		}
		switch (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType)
		{
		case LoadingManager.ESceneType.Lobby:
			Vespa_System_Action_Extension_Method.fire(action);
			break;
		case LoadingManager.ESceneType.Battle2:
			SingletonAuto<BattleContext>.instance.MoveSceneToLobby(delegate
			{
				Vespa_System_Action_Extension_Method.fire(action);
			}, EBoolCheck.DontCare);
			break;
		}
	}

	public static bool IsOpenedPartyDungeon()
	{
		return SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.PartyPlayDungeon);
	}
}
