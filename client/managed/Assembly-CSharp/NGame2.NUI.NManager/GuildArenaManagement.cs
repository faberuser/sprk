using System;
using System.Collections.Generic;
using MiniJSON;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NGuild;
using NGame2.NMatch;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NShared;
using NShared.ApplyGuildArena;
using NShared.GetGuildArenaDeck;
using NShared.GetGuildArenaGroup;
using NShared.GetGuildArenaMemberDeck;
using NShared.GetGuildArenaMemberRecord;
using NShared.GetGuildArenaMemberScore;
using NShared.GetGuildArenaRanker;
using NShared.GetGuildArenaRankerRecord;
using NShared.MatchGuildArena;
using NShared.SetGuildArenaDeck;
using NShared.SetGuildArenaDeckSkill;
using NShared.SetGuildArenaMatchResult;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NUtil;

namespace NGame2.NUI.NManager;

public class GuildArenaManagement : BaseManagement<GuildArenaManagement>, GuildArena.Listener, GuildArenaRankingInfo.Listener, GuildArenaSetting.Listener, GuildArenaSettingInfo.Listener, GuildArenaSkillSetting.Listener, GuildArenaMatchList.Listener, GuildArenaBattlePartySetting.Listener, GuildArenaDeckDetailPopup.Listener
{
	public const int HeroSlotCount = 3;

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<GuildArena>("Resources/Prefab2/Window/GuildArena/GuildArena");
		AddInstance<GuildArenaSetting>("Resources/Prefab2/Window/GuildArena/GuildArenaSetting");
		AddInstance<GuildArenaSettingInfo>("Resources/Prefab2/Window/GuildArena/GuildArenaSettingInfo");
		AddInstance<GuildArenaTotalInfo>("Resources/Prefab2/Window/GuildArena/GuildArenaTotalInfo");
		AddInstance<GuildArenaRankingInfo>("Resources/Prefab2/Window/GuildArena/GuildArenaRankingInfo");
		AddInstance<GuildArenaMatchList>("Resources/Prefab2/Window/GuildArena/GuildArenaMatchList");
		AddInstance<GuildArenaDeckListPopup>("Resources/Prefab2/Window/GuildArena/GuildArenaDeckListPopup");
		AddInstance<GuildArenaBattlePartySetting>("Resources/Prefab2/Window/GuildArena/GuildArenaBattlePartySetting");
		AddInstance<GuildArenaDeckDetailPopup>("Resources/Prefab2/Window/GuildArena/GuildArenaDeckDetailPopup");
		AddInstance<GuildArenaSkillSetting>("Resources/Prefab2/Window/GuildArena/GuildArenaSkillSetting");
		AddInstance<GuildArenaSessionResult>("Resources/Prefab2/Window/GuildArena/GuildArenaSessionResult");
		AddInstance<GuildArenaSeasonResult>("Resources/Prefab2/Window/GuildArena/GuildArenaSeasonResult");
		AddInstance<GuildArenaHistory>("Resources/Prefab2/Window/GuildArena/GuildArenaHistory");
	}

	public void GuildArenaReturnStateFunction()
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager == null)
		{
			return;
		}
		GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.RequestGuildArenaSessionInfo(delegate
		{
			GetInstance<GuildArena>().Open(this);
			GetInstance<GuildArena>().SetPlayEffectSound(enable: false);
			OpenGuildArenaMatchList(delegate
			{
				GetInstance<GuildArenaMatchList>().OpenLastOpenedEnemyMember();
			});
		});
	}

	public void OpenGuildArena()
	{
		if (!GuildManager.InGuildPenalty(GuildContents.Arena) && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager != null)
		{
			GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.RequestGuildArenaSessionInfo(delegate
			{
				GetInstance<GuildArena>().Open(this);
			});
		}
	}

	public void OpenGuildArenaMatchList(Action OnOpen)
	{
		RequestGetGuildArenaGroup(delegate(NShared.GetGuildArenaGroup.Response response)
		{
			GetInstance<GuildArenaMatchList>().Open(this, response);
			Vespa_System_Action_Extension_Method.fire(OnOpen);
		});
	}

	public void OpenGuildArenaSetting()
	{
		RequestGetMyGuildArenaDeck(delegate(GuildArenaDeckInfo[] deckInfos)
		{
			GetInstance<GuildArenaSetting>().Open(this, deckInfos);
		});
	}

	public void OpenGuildArenaBattlePartySetting(GuildArenaDeckInfo deckInfo, GuildArenaMemberDeckInfo memberInfo, CachedHeroInfo[] heroInfos)
	{
		GetInstance<GuildArenaBattlePartySetting>().Open(this, deckInfo, memberInfo, heroInfos);
	}

	public void OpenGuildArenaDeckDetailPopup(CachedHeroInfo[] heroInfos, string titleName)
	{
		GetInstance<GuildArenaDeckDetailPopup>().Open(this, heroInfos, titleName);
	}

	public void OpenGuildArenaSessionResult()
	{
		GetInstance<GuildArenaSessionResult>().Open(null);
	}

	public void OpenGuildArenaSeasonResult()
	{
		GetInstance<GuildArenaSeasonResult>().Open(null);
	}

	public void OpenGuildArenaHistory()
	{
		GetInstance<GuildArenaHistory>().Open();
	}

	private void RequestGuildArenaHistory(Action<NShared.GetGuildArenaMemberRecord.Response> onComplete)
	{
		GuildArenaManager guildArenaManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager;
		GuildArenaRankerInfo myGuildInfo = guildArenaManager.GetMyGuildInfo();
		NShared.GetGuildArenaMemberRecord.Request request = new NShared.GetGuildArenaMemberRecord.Request
		{
			SessionIndex = guildArenaManager.GetGuildArenaSessionInfo().SessionIndex,
			ServerGroup = ((myGuildInfo != null) ? myGuildInfo.ServerGroup : string.Empty),
			GuildId = (myGuildInfo?.GuildId ?? 0),
			RecordType = GuildArenaRecordType.MyRecord
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildArenaMemberRecord.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetGuildArenaMemberRecord.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, response);
			}
		});
	}

	private void RequestGetGuildArenaGroup(Action<NShared.GetGuildArenaGroup.Response> onComplete)
	{
		NShared.GetGuildArenaGroup.Request request = new NShared.GetGuildArenaGroup.Request();
		request.SessionIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSessionInfo().SessionIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildArenaGroup.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetGuildArenaGroup.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.ApplyBattledHeroIndices(response.BattledHeroIndices);
				Vespa_System_Action_Extension_Method.fire(onComplete, response);
			}
		});
	}

	private void RequestSetGuildArenaDeck(List<int[]> heroIndices, Action onComplete)
	{
		NShared.SetGuildArenaDeck.Request request = new NShared.SetGuildArenaDeck.Request();
		if (heroIndices.Count > 0)
		{
			request.HeroIndices1 = heroIndices[0];
		}
		if (heroIndices.Count > 1)
		{
			request.HeroIndices2 = heroIndices[1];
		}
		if (heroIndices.Count > 2)
		{
			request.HeroIndices3 = heroIndices[2];
		}
		if (heroIndices.Count > 3)
		{
			request.HeroIndices4 = heroIndices[3];
		}
		if (heroIndices.Count > 4)
		{
			request.HeroIndices5 = heroIndices[4];
		}
		request.SeasonIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSeasonIndex();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetGuildArenaDeck.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.SetGuildArenaDeck.ResultType.Success)
			{
				if (response.Result == NShared.SetGuildArenaDeck.ResultType.GuildArenaNotApplySetDeck)
				{
					MessagePopup.OpenError(response.Result, delegate
					{
						if (GetInstance<GuildArenaSetting>() != null)
						{
							GetInstance<GuildArenaSetting>().NotApplyAndClose();
						}
					});
				}
				else
				{
					MessagePopup.OpenError(response.Result);
				}
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.SetSavedDeckInfos(response.DeckInfos);
				GetInstance<GuildArenaSetting>().ResetDeck();
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	public void RequestGetMyGuildArenaDeck(Action<GuildArenaDeckInfo[]> onComplete)
	{
		NShared.GetGuildArenaDeck.Request request = new NShared.GetGuildArenaDeck.Request();
		request.SeasonIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSeasonIndex();
		request.MemberId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildArenaDeck.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetGuildArenaDeck.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, response.DeckInfos);
			}
		});
	}

	private void RequestSetGuildArenaDeckSkill(GuildArenaDeckInfo[] deckInfos, Action onComplete)
	{
		NShared.SetGuildArenaDeckSkill.Request request = new NShared.SetGuildArenaDeckSkill.Request();
		request.SeasonIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSeasonIndex();
		if (deckInfos.Length != 0)
		{
			request.SkillSlotIndices1 = deckInfos[0].SkillSlotIndices;
		}
		if (deckInfos.Length > 1)
		{
			request.SkillSlotIndices2 = deckInfos[1].SkillSlotIndices;
		}
		if (deckInfos.Length > 2)
		{
			request.SkillSlotIndices3 = deckInfos[2].SkillSlotIndices;
		}
		if (deckInfos.Length > 3)
		{
			request.SkillSlotIndices4 = deckInfos[3].SkillSlotIndices;
		}
		if (deckInfos.Length > 4)
		{
			request.SkillSlotIndices5 = deckInfos[4].SkillSlotIndices;
		}
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetGuildArenaDeckSkill.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.SetGuildArenaDeckSkill.ResultType.Success)
			{
				if (response.Result == NShared.SetGuildArenaDeckSkill.ResultType.GuildArenaNotApplySetDeckSkill)
				{
					MessagePopup.OpenError(response.Result, delegate
					{
						if (GetInstance<GuildArenaSkillSetting>() != null)
						{
							GetInstance<GuildArenaSkillSetting>().NotApplyAndClose();
						}
						if (GetInstance<GuildArenaSetting>() != null)
						{
							GetInstance<GuildArenaSetting>().NotApplyAndClose();
						}
					});
				}
				else
				{
					MessagePopup.OpenError(response.Result);
				}
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.SetSavedDeckInfos(deckInfos);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	public void RequestGetGuildArenaDeck(string enemyServerGroup, long enemyGuildId, long enemyAccountId, long memberId, Action<GuildArenaDeckInfo[]> onComplete)
	{
		NShared.GetGuildArenaDeck.Request request = new NShared.GetGuildArenaDeck.Request();
		request.SeasonIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSeasonIndex();
		request.EnemyServerGroup = enemyServerGroup;
		request.EnemyGuildId = enemyGuildId;
		request.EnemyAccountId = enemyAccountId;
		request.MemberId = memberId;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildArenaDeck.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetGuildArenaDeck.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result, delegate
				{
					if (GetInstance<GuildArenaSetting>() != null)
					{
						GetInstance<GuildArenaSetting>().NotApplyAndClose();
					}
				});
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, response.DeckInfos);
			}
		});
	}

	public void RequestGetGuildArenaRankerRecord(int pageNo, Action<NShared.GetGuildArenaRankerRecord.Response> onComplete)
	{
		NShared.GetGuildArenaRankerRecord.Request request = new NShared.GetGuildArenaRankerRecord.Request();
		request.SeasonIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSeasonIndex();
		request.PageNo = pageNo;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildArenaRankerRecord.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			if (response.Result != NShared.GetGuildArenaRankerRecord.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			Vespa_System_Action_Extension_Method.fire(onComplete, response);
		});
	}

	public void RequestGetGuildArenaRanker(int pageNo, Action<NShared.GetGuildArenaRanker.Response> onComplete)
	{
		NShared.GetGuildArenaRanker.Request request = new NShared.GetGuildArenaRanker.Request();
		request.SeasonIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSeasonIndex();
		request.PageNo = pageNo;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildArenaRanker.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			if (response.Result != NShared.GetGuildArenaRanker.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			Vespa_System_Action_Extension_Method.fire(onComplete, response);
		});
	}

	public void RequestGetGuildArenaMemberScore(int pageNo, Action<NShared.GetGuildArenaMemberScore.Response> onComplete)
	{
		NShared.GetGuildArenaMemberScore.Request request = new NShared.GetGuildArenaMemberScore.Request();
		request.SessionIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSessionInfo().SessionIndex;
		request.PageNo = pageNo;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildArenaMemberScore.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			if (response.Result != NShared.GetGuildArenaMemberScore.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			Vespa_System_Action_Extension_Method.fire(onComplete, response);
		});
	}

	public void RequestGetGuildArenaMyRecord(GuildArenaRecordType recordType, Action<NShared.GetGuildArenaMemberRecord.Response> onComplete)
	{
		GuildArenaManager guildArenaManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager;
		GuildArenaRankerInfo myGuildInfo = guildArenaManager.GetMyGuildInfo();
		NShared.GetGuildArenaMemberRecord.Request request = new NShared.GetGuildArenaMemberRecord.Request
		{
			SessionIndex = guildArenaManager.GetGuildArenaSessionInfo().SessionIndex,
			ServerGroup = ((myGuildInfo != null) ? myGuildInfo.ServerGroup : string.Empty),
			GuildId = (myGuildInfo?.GuildId ?? 0),
			RecordType = recordType
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildArenaMemberRecord.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetGuildArenaMemberRecord.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			Vespa_System_Action_Extension_Method.fire(onComplete, response);
		});
	}

	private void RequestMatchGuildArena(int deckIndex, GuildArenaMemberDeckInfo memberInfo, CachedHeroInfo[] myHeroInfos, Action onComplete)
	{
		if (memberInfo == null || myHeroInfos == null)
		{
			return;
		}
		NShared.MatchGuildArena.Request request = new NShared.MatchGuildArena.Request();
		List<int> list = new List<int>();
		foreach (CachedHeroInfo cachedHeroInfo in myHeroInfos)
		{
			if (cachedHeroInfo != null && cachedHeroInfo.HeroIndex != 0)
			{
				list.Add(cachedHeroInfo.HeroIndex);
			}
		}
		request.HeroIndices = list.ToArray();
		request.EnemyAccountId = memberInfo.AccountId;
		request.EnemyDeckIndex = deckIndex;
		request.EnemyGuildId = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetEnemyGuildInfo().GuildId;
		request.EnemyServerGroup = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetEnemyGuildInfo().ServerGroup;
		request.SessionIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSessionInfo().SessionIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.MatchGuildArena.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.MatchGuildArena.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.HeroIndices == null || response.AccountInfo == null || response.MatchedNpcInfo == null)
			{
				MessagePopup.OpenError(NShared.MatchGuildArena.ResultType.Fail);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
				LobbyScene2.ListReturnState.Clear();
				LobbyScene2.ListReturnState.Add(new GuildArenaReturnState());
				StartOfflineGuildArena(response.HeroIndices, response.ChapterIndex, response.DungeonIndex, response.AccountInfo, response.MatchedNpcInfo, response.MyScoreInfo, response.EnemyScoreInfo);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	public void RequestGetGuildArenaMemberDeck(Action<NShared.GetGuildArenaMemberDeck.Response> onComplete)
	{
		NShared.GetGuildArenaMemberDeck.Request request = new NShared.GetGuildArenaMemberDeck.Request
		{
			SessionIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSessionInfo().SessionIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetGuildArenaMemberDeck.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetGuildArenaMemberDeck.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result, delegate
				{
					if (GetInstance<GuildArenaSetting>() != null)
					{
						GetInstance<GuildArenaSetting>().NotApplyAndClose();
					}
				});
			}
			Vespa_System_Action_Extension_Method.fire(onComplete, response);
		});
	}

	private void StartOfflineGuildArena(int[] heroIndices, int chapterIndex, int dungeonIndex, MatchedAccountInfo myMatchInfo, MatchedAccountInfo matchedNpcInfo, GuildArenaMemberScoreInfo myScoreInfo, GuildArenaMemberScoreInfo enemyScoreInfo)
	{
		SingletonUpdater<MatchManager>.instance.OfflinePlaying = true;
		SingletonUpdater<MatchManager>.instance.SetMatchArenaType(MatchArenaType.GuildArena);
		SingletonUpdater<MatchManager>.instance.SetMatchDuelType(MatchDuelType.Arena);
		myMatchInfo.ArenaType = MatchArenaType.GuildArena;
		matchedNpcInfo.ArenaType = MatchArenaType.GuildArena;
		SingletonAuto<BattleContext>.instance.SetOfflineArena(heroIndices, 1, matchedNpcInfo, chapterIndex, dungeonIndex, myMatchInfo);
		GuildArenaRankerInfo myGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo();
		GuildArenaRankerInfo enemyGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetEnemyGuildInfo();
		SingletonUpdater<MatchManager>.instance.InitVsLoadingPlayer(myMatchInfo, isLeft: true, isOnline: false, myGuildInfo, myScoreInfo);
		SingletonUpdater<MatchManager>.instance.InitVsLoadingPlayer(matchedNpcInfo, isLeft: false, isOnline: false, enemyGuildInfo, enemyScoreInfo);
		SingletonUpdater<Executer>.instance.DelayRun(ContentsDefine.MatchArena.DelayTime_for_EnterBattleScene, this, SingletonUpdater<MatchManager>.instance.MoveToBattleScene);
	}

	public void RequestSetGuildArenaMatchResult(bool Win, int PlayTime, int[] AliveHeroIndices, string BattleLogString, IList<ResultCreatureInfo> creatureInfos, Action onComplete)
	{
		NShared.SetGuildArenaMatchResult.Request request = new NShared.SetGuildArenaMatchResult.Request();
		request.AliveHeroIndices = AliveHeroIndices;
		request.BattleLogString = BattleLogString;
		request.PlayTime = PlayTime;
		request.Win = (Win ? 1 : 0);
		if (creatureInfos != null)
		{
			string creatureInfoString = Json.Serialize(BaseJsonMarshaler.EncodeArray(creatureInfos, JsonMarshaler.EncodeJsonObject));
			request.CreatureInfoString = creatureInfoString;
		}
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetGuildArenaMatchResult.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail, ReturnToLobby);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult, ReturnToLobby);
			}
			else if (response.Result != NShared.SetGuildArenaMatchResult.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result, ReturnToLobby);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.RewardItems);
				SceneAutoBehaviour<ArenaManagement>.instance.OpenGuildArenaMatchResult(response);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	private void ReturnToLobby()
	{
		SingletonAuto<BattleContext>.instance.ReturnToLobby();
	}

	private void RequestApplyGuildArena(Action callback)
	{
		NShared.ApplyGuildArena.Request request = new NShared.ApplyGuildArena.Request();
		request.SessionIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSessionInfo().SessionIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ApplyGuildArena.Response response)
		{
			if (response.Result != NShared.ApplyGuildArena.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.SetGuildArenaApplied();
				Vespa_System_Action_Extension_Method.fire(callback);
			}
		});
	}

	void GuildArena.Listener.OpenGuildArenaSetting()
	{
		OpenGuildArenaSetting();
	}

	void GuildArena.Listener.OpenGuildArenaTotalInfo()
	{
		GetInstance<GuildArenaTotalInfo>().Open(null);
	}

	void GuildArena.Listener.OpenGuildArenaRankingInfo()
	{
		GetInstance<GuildArenaRankingInfo>().Open(this, 0);
	}

	void GuildArenaSetting.Listener.OnClickSkillSetting()
	{
		GetInstance<GuildArenaSkillSetting>().Open(this);
	}

	void GuildArenaSetting.Listener.OnClickSaveDeck(List<int[]> heroIndices)
	{
		RequestSetGuildArenaDeck(heroIndices, delegate
		{
			GetInstance<GuildArenaSetting>().OnDeckSaved();
		});
	}

	void GuildArenaSetting.Listener.OpenGuildArenaSettingInfo()
	{
		GetInstance<GuildArenaSettingInfo>().Open(this);
	}

	void GuildArena.Listener.OpenGuildArenaMatchList()
	{
		OpenGuildArenaMatchList(null);
	}

	void GuildArenaMatchList.Listener.OnClickDeckInfo(GuildArenaMemberDeckInfo memberInfo)
	{
		if (memberInfo != null)
		{
			RequestGetGuildArenaDeck("", 0L, 0L, memberInfo.AccountId, delegate(GuildArenaDeckInfo[] deckInfos)
			{
				GetInstance<GuildArenaDeckListPopup>().Open(GetInstance<GuildArenaMatchList>(), memberInfo, deckInfos, TeamType.Left);
			});
		}
	}

	void GuildArenaMatchList.Listener.OnClickDeckAttack(GuildArenaMemberDeckInfo memberInfo)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetEnemyGuildInfo() != null && memberInfo != null)
		{
			string serverGroup = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetEnemyGuildInfo().ServerGroup;
			long guildId = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetEnemyGuildInfo().GuildId;
			long accountId = memberInfo.AccountId;
			RequestGetGuildArenaDeck(serverGroup, guildId, accountId, 0L, delegate(GuildArenaDeckInfo[] deckInfos)
			{
				GetInstance<GuildArenaDeckListPopup>().Open(GetInstance<GuildArenaMatchList>(), memberInfo, deckInfos, TeamType.Right);
			});
		}
	}

	void GuildArenaMatchList.Listener.OnClickHistory()
	{
		GetInstance<GuildArenaHistory>().Open();
	}

	void GuildArenaBattlePartySetting.Listener.OnClickStartBattle(int deckIndex, GuildArenaMemberDeckInfo memberInfo, CachedHeroInfo[] myHeroInfos)
	{
		RequestMatchGuildArena(deckIndex, memberInfo, myHeroInfos, null);
	}

	void GuildArenaSkillSetting.Listener.OnClickSaveSkillIndices(GuildArenaDeckInfo[] deckInfos)
	{
		RequestSetGuildArenaDeckSkill(deckInfos, delegate
		{
			GetInstance<GuildArenaSkillSetting>().SaveAndClose();
		});
	}

	void GuildArena.Listener.OnClickApplyGuildArena()
	{
		RequestApplyGuildArena(delegate
		{
			GetInstance<GuildArena>().InitButtons();
		});
	}
}
