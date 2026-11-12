using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NContentsDefine;
using NGame2.NGuild;
using NGame2.NParty;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NGuild;
using NGame2.NUI.NWindow;
using NShared;
using NShared.ApplyGuildSuppress;
using NShared.GetGuildSuppressGroupRankerList;
using NShared.GetGuildSuppressMemberScorelist;
using NShared.GetGuildSuppressSessionInfo;
using NShared.GetGuildSuppressStatusBoard;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager.NLobby;

public class GuildSuppressManagement : BaseManagement<GuildSuppressManagement>, GuildSuppressRanking.Listener, GuildSuppressDungeonList.Listener, GuildSuppressModePopup.Listener, GuildSuppressStatusBoard.IListener, GuildSuppressStatusBoardBossComponent.IClickListener
{
	public const int RankerPerPage = 10;

	private float _battleRequestedTime;

	public static int TotalRankerCount;

	private float _cachedRankerTime;

	public GuildSuppressSessionData GuildSuppressSessionData { get; private set; }

	public GuildSuppressGroupRankerInfo[] GroupRankers { get; private set; }

	public GuildSuppressMemberScoreInfo[] MemberRankers { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<GuildSuppressRanking>("Resources/Prefab2/Window/GuildSuppress/GuildSuppressRanking");
		AddInstance<GuildSuppressDungeonList>("Resources/Prefab2/Window/GuildSuppress/GuildSuppressDungeonList");
		AddInstance<GuildSuppressInfoPopup>("Resources/Prefab2/Window/GuildSuppress/GuildSuppressInfoPopup");
		AddInstance<GuildSuppressBossInfo>("Resources/Prefab2/Window/GuildSuppress/GuildSuppressBossInfo");
		AddInstance<GuildSuppressModePopup>("Resources/Prefab2/Window/GuildSuppress/GuildSuppressModePopup");
		AddInstance<GuildSuppressBanList>("Resources/Prefab2/Window/GuildSuppress/GuildSuppressBanList");
		AddInstance<GuildSuppressRankingRewardPopup>("Resources/Prefab2/Window/GuildSuppress/GuildSuppressRankingRewardPopup");
		AddInstance<GuildSuppressStatusBoard>("Resources/Prefab2/Window/GuildSuppress/GuildSuppressStatusBoard");
	}

	private void Start()
	{
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
		}
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
	}

	public void OpenGuildStatusBoard()
	{
		GuildSuppressStatusBoard window = GetInstance<GuildSuppressStatusBoard>();
		if (window == null || window.IsOpen())
		{
			return;
		}
		GuildSuppressHelper.RequestGuildSuppressStatusBoard(delegate(NShared.GetGuildSuppressStatusBoard.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.GetGuildSuppressStatusBoard.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				window.Open(res, this);
			}
		});
	}

	public void OpenGuildSuppressBanList()
	{
		InternalRequestGuildSuppressSessionInfo(delegate
		{
			GuildSuppressBanInfo guildSuppressBanInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressBanInfo();
			if (guildSuppressBanInfo == null)
			{
				string stringDirect = LocalizationManager.GetStringDirect("BASE_RESULT_TYPE.SEQUENCE_ERROR");
				MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), stringDirect);
			}
			else
			{
				GetInstance<GuildSuppressBanList>().Open(guildSuppressBanInfo);
			}
		});
	}

	public void OpenGuildSuppressRanking(bool raiseEvent, Action onOpen = null)
	{
		if (raiseEvent)
		{
			Vespa_System_Action_Extension_Method.fire(onOpen);
			return;
		}
		if (GuildManager.InGuildPenalty(GuildContents.Suppress))
		{
			Vespa_System_Action_Extension_Method.fire(onOpen);
			return;
		}
		RequestGuildSuppressSessionInfo(onOpen, delegate
		{
			SceneAutoBehaviour<ContentsNoticePopupManagement>.instance.OpenContentsResultPopup(ContentsNoticeType.GuildSuppress);
		});
	}

	private void RequestGuildSuppressSessionInfo(Action openAction = null, Action onComplete = null)
	{
		InternalRequestGuildSuppressSessionInfo(delegate
		{
			if (GuildSuppressSessionData == null)
			{
				string stringDirect = LocalizationManager.GetStringDirect("PARTY_SEARCH_ENTER_FAILURE");
				MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), stringDirect);
			}
			else
			{
				GuildSuppressSessionData.SuppressDungeonIndex.First();
				GetInstance<GuildSuppressRanking>().Open(this, GuildSuppressSessionData.RaidIndex, openAction);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	public void InternalRequestGuildSuppressSessionInfo(Action OnSuccess = null, bool enableErrorPopup = true)
	{
		NShared.GetGuildSuppressSessionInfo.Request request = new NShared.GetGuildSuppressSessionInfo.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildSuppressSessionInfo.Response response)
		{
			if (response.Result != NShared.GetGuildSuppressSessionInfo.ResultType.Success)
			{
				if (enableErrorPopup)
				{
					MessagePopup.OpenError(response.Result);
				}
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				if (enableErrorPopup)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.ApplyBanInfo(response.GuildSuppressBanInfo);
				if (response.GuildSuppressInfo == null || response.GuildSuppressSessionInfo == null || __Extension_Method_ICollection__.COUNT(response.GuildSuppressPlayInfos) == 0)
				{
					if (enableErrorPopup)
					{
						MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), LocalizationManager.GetStringDirect("COMMON_CONTENT_NOT_ENTER"), null);
					}
				}
				else
				{
					GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.ApplySessionInfo(response.GuildSuppressSessionInfo, response.GuildSuppressApplied, response.AutoApplySuppress);
					GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.Apply(response.GuildSuppressInfo);
					GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.Apply(response.GuildSuppressPlayInfos);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
					GuildSuppressSessionInfo guildSuppressSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressSessionInfo();
					if (guildSuppressSessionInfo == null)
					{
						if (enableErrorPopup)
						{
							string stringDirect = LocalizationManager.GetStringDirect("PARTY_SEARCH_ENTER_FAILURE");
							MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), stringDirect);
						}
					}
					else
					{
						GuildSuppressSessionData = RestoredPortal.ConquestSession(GameRoot.Instance.GuildSuppressSessionDataContainer, (int)guildSuppressSessionInfo.SessionIndex);
						Vespa_System_Action_Extension_Method.fire(OnSuccess);
					}
				}
			}
		});
	}

	private void RequestGuildSuppressApply(Action callback = null)
	{
		if (GuildSuppressSessionData == null)
		{
			return;
		}
		NShared.ApplyGuildSuppress.Request request = new NShared.ApplyGuildSuppress.Request
		{
			SessionIndex = GuildSuppressSessionData.SessionIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ApplyGuildSuppress.Response response)
		{
			if (response.Result != NShared.ApplyGuildSuppress.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(callback);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(callback);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.Applied = true;
				Vespa_System_Action_Extension_Method.fire(callback);
			}
		});
	}

	public void OpenGuildSuppressDungeonList(int raidIndex, bool dungeonCleared, GuildSuppressPlayInfo clearedSuppressPlayInfo, Action onComplete = null)
	{
		Action action = delegate
		{
			GuildSuppressDungeonList guildSuppressDungeonList = GetInstance<GuildSuppressDungeonList>();
			if (dungeonCleared && clearedSuppressPlayInfo != null)
			{
				int dungeonIndexByLevel = GameRoot.Instance.GuildSuppressSessionDataContainer.GetDungeonIndexByLevel(clearedSuppressPlayInfo.SessionIndex, clearedSuppressPlayInfo.Level);
				guildSuppressDungeonList.Open(this, raidIndex, dungeonCleared, dungeonIndexByLevel, clearedSuppressPlayInfo.Level);
			}
			else
			{
				guildSuppressDungeonList.Open(this, raidIndex, dungeonCleared, 0, 0);
			}
		};
		if (GetInstance<GuildSuppressRanking>().IsOpen())
		{
			Vespa_System_Action_Extension_Method.fire(action);
		}
		else
		{
			RequestGuildSuppressSessionInfo(action, onComplete);
		}
	}

	public void OpenGuildSuppressPartySetting(GuildSuppressDungeonData guildSuppressDungeonData, int raidIndex, int raidLevel, Action<int, bool, GuildSuppressPlayInfo, Action> openAction = null)
	{
		if (openAction != null)
		{
			Vespa_System_Action_Extension_Method.fire(openAction, raidIndex, i_argB: false, null, delegate
			{
				OpenGuildSuppressPartySetting(guildSuppressDungeonData, raidIndex, raidLevel);
			});
		}
		else
		{
			OpenGuildSuppressPartySetting(guildSuppressDungeonData, raidIndex, raidLevel);
		}
	}

	private void OpenGuildSuppressPartySetting(GuildSuppressDungeonData guildSuppressDungeonData, int raidIndex, int raidLevel)
	{
		if (!GlobalAutoBehaviour<PartyManager>.isValidInstance || !GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
		{
			VespaDebug.LogError("Invalid party!");
			return;
		}
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel);
		if (data == null)
		{
			VespaDebug.LogError("RaidData not found : raid=" + data);
			return;
		}
		UIHelper.CloseWindowsBeforeRaidPartySetting();
		if (data.IsOnlineSingle)
		{
			SceneAutoBehaviour<RaidManagement>.instance.OpenSingleRaidPartySetting(data.Index, data.Level, null);
		}
		else
		{
			SceneAutoBehaviour<RaidManagement>.instance.OpenGuilSuppressPartySetting(data, guildSuppressDungeonData);
		}
	}

	void GuildSuppressRanking.Listener.GetGuildRankerList(Action<GuildSuppressGroupRankerInfo[]> onList)
	{
		GuildSuppressSessionInfo guildSuppressSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressSessionInfo();
		GuildSuppressInfo guildSuppressInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressInfo();
		if (guildSuppressSessionInfo == null || guildSuppressInfo == null)
		{
			Vespa_System_Action_Extension_Method.fire(onList, null);
			return;
		}
		if (guildSuppressInfo.GroupIndex == 0)
		{
			Vespa_System_Action_Extension_Method.fire(onList, null);
			return;
		}
		if (GroupRankers != null && Time.time - _cachedRankerTime > 10f)
		{
			GroupRankers = null;
			_cachedRankerTime = 0f;
		}
		if (GroupRankers != null)
		{
			Vespa_System_Action_Extension_Method.fire(onList, GroupRankers);
			return;
		}
		NShared.GetGuildSuppressGroupRankerList.Request request = new NShared.GetGuildSuppressGroupRankerList.Request();
		request.SessionIndex = (int)guildSuppressSessionInfo.SessionIndex;
		request.GroupIndex = guildSuppressInfo.GroupIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildSuppressGroupRankerList.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else if (response.Result != NShared.GetGuildSuppressGroupRankerList.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else
			{
				GuildSuppressGroupRankerInfo[] guildSuppressGroupRankerInfos = response.GuildSuppressGroupRankerInfos;
				GroupRankers = guildSuppressGroupRankerInfos;
				Vespa_System_Action_Extension_Method.fire(onList, guildSuppressGroupRankerInfos);
			}
		});
	}

	void GuildSuppressRanking.Listener.GetGuildMemberRankerList(Action<GuildSuppressMemberScoreInfo[]> onList)
	{
		GuildSuppressSessionInfo guildSuppressSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressSessionInfo();
		if (guildSuppressSessionInfo == null)
		{
			Vespa_System_Action_Extension_Method.fire(onList, null);
			return;
		}
		if (MemberRankers != null && Time.time - _cachedRankerTime > 10f)
		{
			MemberRankers = null;
			_cachedRankerTime = 0f;
		}
		if (MemberRankers != null)
		{
			Vespa_System_Action_Extension_Method.fire(onList, MemberRankers);
			return;
		}
		NShared.GetGuildSuppressMemberScorelist.Request request = new NShared.GetGuildSuppressMemberScorelist.Request();
		request.SessionIndex = (int)guildSuppressSessionInfo.SessionIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildSuppressMemberScorelist.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else if (response.Result != NShared.GetGuildSuppressMemberScorelist.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else
			{
				GuildSuppressMemberScoreInfo[] guildSuppressMemberScores = response.GuildSuppressMemberScores;
				MemberRankers = guildSuppressMemberScores;
				Vespa_System_Action_Extension_Method.fire(onList, MemberRankers);
			}
		});
	}

	void GuildSuppressRanking.Listener.OnClickRankingRewardPopup()
	{
		GetInstance<GuildSuppressRankingRewardPopup>().Open();
	}

	void GuildSuppressRanking.Listener.OnClickInfoPopup()
	{
		GetInstance<GuildSuppressInfoPopup>().Open();
	}

	void GuildSuppressRanking.Listener.OnClickApplyGuildSuppress()
	{
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo != null && (myMemberInfo.Rank == 1 || myMemberInfo.Rank == 2))
		{
			RequestGuildSuppressApply(delegate
			{
				GetInstance<GuildSuppressRanking>().SetButtonState();
				GetInstance<GuildSuppressRanking>().CompleteApply = true;
			});
		}
	}

	void GuildSuppressRanking.Listener.OnClickReadyGuildSuppress()
	{
		if (GuildSuppressSessionData != null)
		{
			OpenGuildSuppressDungeonList(GuildSuppressSessionData.RaidIndex, dungeonCleared: false, null);
		}
	}

	void GuildSuppressDungeonList.Listener.OnClickEnter(int raidIndex, int raidLevel)
	{
		CurrencyMaxCap.WarningOrExecute(CurrencyType.GuildPoint, delegate
		{
			GetInstance<GuildSuppressModePopup>().Open(raidIndex, raidLevel, this);
		});
	}

	void GuildSuppressDungeonList.Listener.OnClickInfo(GuildSuppressDungeonData guildSuppressDungeonData, int level)
	{
		GetInstance<GuildSuppressBossInfo>().Open(null, guildSuppressDungeonData, level);
	}

	void GuildSuppressModePopup.Listener.OnSelect(bool isMulti, int raidIndex, int raidLevel)
	{
		PartyManager.IsSingle = !isMulti;
		if (isMulti)
		{
			RaidManagement.CreateParty(raidIndex, raidLevel, isPublic: true, RaidRoomType.Help, null);
			return;
		}
		GuildSuppressPlayInfo guildSuppressPlayInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildSuppressManager.GetGuildSuppressPlayInfo(raidIndex, raidLevel);
		if (guildSuppressPlayInfo != null)
		{
			int singleLevel = GameRoot.Instance.GuildSuppressSessionDataContainer.GetSingleLevel(guildSuppressPlayInfo.Level);
			int dungeonIndexByLevel = GameRoot.Instance.GuildSuppressSessionDataContainer.GetDungeonIndexByLevel(guildSuppressPlayInfo.SessionIndex, guildSuppressPlayInfo.Level);
			GlobalAutoBehaviour<PartyManager>.instance.SetGuildSuppress(dungeonIndexByLevel, guildSuppressPlayInfo.RaidIndex, singleLevel);
			RaidManagement.CreateParty(guildSuppressPlayInfo.RaidIndex, singleLevel, isPublic: true, RaidRoomType.Help, null);
		}
	}

	void GuildSuppressStatusBoard.IListener.OnCilickRefresh()
	{
	}

	void GuildSuppressStatusBoardBossComponent.IClickListener.OnClickBossInfo(GuildSuppressDungeonData suppressDungeonData, int raidIndex, int level)
	{
		GetInstance<GuildSuppressBossInfo>().Open(null, suppressDungeonData, raidIndex, level);
	}

	public void OpenGuildSuppressRanking(int raidIndex, Action callback)
	{
		GetInstance<GuildSuppressRanking>().Open(this, raidIndex, callback);
		GetInstance<GuildSuppressRanking>().Tutorial_InitUi();
	}

	public void ChangeSuppressButton(Action callback)
	{
		GetInstance<GuildSuppressRanking>().Tutorial_ChangeUiButton();
	}

	public void OpenSuppressList(int raidIndex, int raidLevel, List<int> banList, Action callback)
	{
		GetInstance<GuildSuppressDungeonList>().Open(this, raidIndex, dungeonCleared: false, 0, 0, callback);
		GetInstance<GuildSuppressDungeonList>().Tutorial_InitUi(raidIndex, raidLevel, banList.ToArray());
	}
}
