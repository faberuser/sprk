using System;
using NGame2.NAccount;
using NGame2.NCutScene;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NShared.ChangeRecruitHero;
using NShared.DoHeroFriendly;
using NShared.GiveRewardMaxClosenessHero;
using NShared.HeroInnResetTime;
using NShared.RecruitHero;
using NShared.RequestNewFriendlyHero;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class HeroInnManagement : BaseManagement<HeroInnManagement>, HeroInnView.Listener, HeroFullSceneViewBase.Listener, HeroDetailView.Listener
{
	private HeroInfo _heroInfo;

	private bool _requestRefresh;

	private bool _requestResetTime;

	private bool _isWaitingResponse;

	private DateTime _lastResetCheckTime;

	private DateTime LastResetTime;

	private DateTime NextResetTime;

	protected override void Awake()
	{
		base.Awake();
		AddInstance<HeroInnView>("Resources/Prefab2/Window/FullScene/HeroInnView");
		AddInstance<HeroDetailView>("Resources/Prefab2/Window/FullScene/HeroDetailView");
	}

	private void RequestResetTime()
	{
		if (_isWaitingResponse)
		{
			return;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		DateTime UTCNow = DateTime.UtcNow;
		if (_requestResetTime && !(currentServerUTCTime > NextResetTime) && !((UTCNow - _lastResetCheckTime).TotalSeconds > 300.0))
		{
			return;
		}
		_isWaitingResponse = true;
		NShared.HeroInnResetTime.Request request = new NShared.HeroInnResetTime.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.HeroInnResetTime.Response response)
		{
			_isWaitingResponse = false;
			if (response.BaseResult == BaseResultType.Success)
			{
				UTCNow = DateTime.UtcNow;
				_lastResetCheckTime = UTCNow;
				_requestResetTime = true;
				Vespa_String_Extension_Method2.TryParseNetworkTime2(response.LastResetTime, out LastResetTime);
				Vespa_String_Extension_Method2.TryParseNetworkTime2(response.NextResetTime, out NextResetTime);
			}
		});
	}

	public bool ResetHourElapsed(string lastResetTime)
	{
		RequestResetTime();
		if (!_requestResetTime)
		{
			return CheckResetLocalTime(lastResetTime);
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(lastResetTime, out var o_dateTime))
		{
			return true;
		}
		if (LastResetTime.CompareTo(o_dateTime) > 0)
		{
			return true;
		}
		return false;
	}

	public bool CheckResetLocalTime(string checkTimeString)
	{
		if (String_Extension_Method.IsNullOrEmpty(checkTimeString))
		{
			return true;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(checkTimeString, out var o_dateTime))
		{
			return true;
		}
		DateTime midNightTime = GetMidNightTime(o_dateTime);
		if (GetMidNightTime(GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).CompareTo(midNightTime) != 1)
		{
			return false;
		}
		return true;
	}

	public DateTime GetMidNightTime(DateTime time)
	{
		return new DateTime(time.Year, time.Month, time.Day);
	}

	private bool CheckRequestNeeded(PlayerHeroFriendlyInfo heroInfo)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetSelectedHeroIndices()[0] == 0)
		{
			return true;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroInfo.HeroIndex) != null)
		{
			return true;
		}
		return ResetHourElapsed(heroInfo.SelectedTime);
	}

	private void RequestNewFriendlyHero(Action<NShared.RequestNewFriendlyHero.Response> responseAction)
	{
		NShared.RequestNewFriendlyHero.Request request = new NShared.RequestNewFriendlyHero.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, responseAction);
	}

	private void RequestRewardMaxClosenessHero(int[] heroIndices)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(heroIndices))
		{
			return;
		}
		NShared.GiveRewardMaxClosenessHero.Request request = new NShared.GiveRewardMaxClosenessHero.Request
		{
			HeroIndices = heroIndices
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GiveRewardMaxClosenessHero.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else if (res.Result != NShared.GiveRewardMaxClosenessHero.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				RewardApply(res);
				GetInstance<HeroInnView>().ReservedMaxClosenessReward(res);
			}
		});
	}

	private void RewardApply(NShared.GiveRewardMaxClosenessHero.Response response)
	{
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
		GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.itemResults);
		GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.equipItemInfos);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.staminaResult);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.FriendshipPointResult);
	}

	private void RequestDoHeroFriendly(FriendlyActionType actionType)
	{
		NShared.DoHeroFriendly.Request request = new NShared.DoHeroFriendly.Request
		{
			HeroIndex = GetInstance<HeroInnView>().HeroInfo.HeroIndex,
			Action = actionType
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.DoHeroFriendly.Response response)
		{
			if (response.Result == NShared.DoHeroFriendly.ResultType.Success)
			{
				if (response.FriendshipPointResult != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.FriendshipPointResult);
				}
				if (response.CurrencyResult != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				}
				if (response.HeroInfo != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ApplyHeroCloseness(response.HeroInfo);
				}
				if (response.FriendlyInfo != null)
				{
					GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.Apply(response.FriendlyInfo);
				}
				GetInstance<HeroInnView>().UpdateResponse(actionType);
			}
			else
			{
				MessagePopup.OpenError(response.Result);
			}
		});
	}

	public void OpenHeroInn()
	{
		PlayerHeroFriendlyInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetValues();
		PlayerHeroFriendlyInfo playerHeroFriendlyInfo = (__Extension_Method_ICollection__.IsNullOrEmpty(values) ? null : values[0]);
		if (playerHeroFriendlyInfo == null)
		{
			return;
		}
		if (CheckRequestNeeded(playerHeroFriendlyInfo))
		{
			RequestNewFriendlyHero(delegate(NShared.RequestNewFriendlyHero.Response response)
			{
				if (response.Result == NShared.RequestNewFriendlyHero.ResultType.Success)
				{
					GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.Apply(response.FriendlyInfo);
					OpenHeroInnInternal(response);
					RequestRewardMaxClosenessHero(response.MaxClosenessHeroIndice);
				}
				else if (response.Result == NShared.RequestNewFriendlyHero.ResultType.CannotRecruitHero)
				{
					SceneAutoBehaviour<CutSceneManager>.instance.Open("CutScene/other/HM1_1A", canSkip: true, null);
				}
				else
				{
					MessagePopup.OpenError(response.Result);
				}
			});
		}
		else
		{
			OpenHeroInnInternal(null);
		}
	}

	private void OpenHeroInnInternal(NShared.RequestNewFriendlyHero.Response res)
	{
		PlayerHeroFriendlyInfo friendlyInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetFriendlyInfo();
		int heroIndex = ((friendlyInfo.HeroIndex != 0) ? friendlyInfo.HeroIndex : GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetSelectedHeroIndices()[0]);
		OpenHeroInnInternal(heroIndex, res);
	}

	private HeroInfo GetHeroInfo(int heroIndex)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.IsHaveHero(heroIndex))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data == null)
		{
			return null;
		}
		return SceneAutoBehaviour<StatDataCache>.instance.GetReferenceHero(heroIndex, data.StartHeroStar);
	}

	private void OpenHeroInnInternal(int heroIndex, NShared.RequestNewFriendlyHero.Response res)
	{
		if (heroIndex == 0)
		{
			return;
		}
		HeroInfo heroInfo = GetHeroInfo(heroIndex);
		if (heroInfo != null)
		{
			_heroInfo = heroInfo;
			HeroInnView heroInnView = GetInstance<HeroInnView>();
			if (heroInnView.IsAreadyOpenHero(heroIndex))
			{
				heroInnView.Reload(_heroInfo);
			}
			else
			{
				heroInnView.Open(_heroInfo, this, heroInfo.HeroIndex == heroIndex);
			}
		}
	}

	void HeroInnView.Listener.OnGreeting()
	{
		RequestDoHeroFriendly(FriendlyActionType.Greeting);
	}

	void HeroInnView.Listener.OnConversation()
	{
		RequestDoHeroFriendly(FriendlyActionType.Conversation);
	}

	void HeroInnView.Listener.OnSendGift()
	{
		RequestDoHeroFriendly(FriendlyActionType.Gift);
	}

	void HeroInnView.Listener.OnClosenessGreeting()
	{
		RequestDoHeroFriendly(FriendlyActionType.Closeness_Greeting);
	}

	void HeroInnView.Listener.OnClosenessConversation()
	{
		RequestDoHeroFriendly(FriendlyActionType.Closeness_Conversation);
	}

	void HeroInnView.Listener.OnClosenessGift()
	{
		RequestDoHeroFriendly(FriendlyActionType.Closeness_Gift);
	}

	private bool IsHaveHeroInnView()
	{
		HeroInnView heroInnView = GetInstance<HeroInnView>();
		if (heroInnView == null)
		{
			return false;
		}
		return heroInnView.IsHaveHero();
	}

	bool HeroInnView.Listener.CheckAreadySelectHero()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.IsAreadySelectHero();
	}

	private void ChangeRecruitHeroProtocol(int changeHeroindex, Action<NShared.ChangeRecruitHero.Response> responseCallback)
	{
		NShared.ChangeRecruitHero.Request request = new NShared.ChangeRecruitHero.Request
		{
			HeroIndex = changeHeroindex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, responseCallback);
	}

	void HeroInnView.Listener.OnStartRecruit()
	{
		ChangeRecruitHeroProtocol(GetInstance<HeroInnView>().HeroInfo.HeroIndex, delegate(NShared.ChangeRecruitHero.Response response)
		{
			if (response.Result == NShared.ChangeRecruitHero.ResultType.Success)
			{
				GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.Apply(response.FriendlyInfo);
				GetInstance<HeroInnView>().UpdateUI(bSelectHero: false);
				GetInstance<HeroInnView>().InitUIBtnGroup();
			}
			else
			{
				MessagePopup.OpenError(response.Result);
			}
		});
	}

	void HeroInnView.Listener.OnChangeAndStartRecruit()
	{
		ChangeRecruitHeroProtocol(0, delegate(NShared.ChangeRecruitHero.Response response)
		{
			if (response.Result == NShared.ChangeRecruitHero.ResultType.Success)
			{
				GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.Apply(response.FriendlyInfo);
				Vespa_Common_Class_Extension_Method.Interface((HeroInnView.Listener)this).OnStartRecruit();
			}
			else
			{
				MessagePopup.OpenError(response.Result);
			}
		});
	}

	void HeroInnView.Listener.OnChangeRecruit()
	{
		MessagePopup.Open(LocalizationManager.GetStringDirect("HEROINN_CHOICE_ANOTHERHERO"), LocalizationManager.GetStringDirect("HEROINN_POPUP_RESET"), delegate
		{
			ChangeRecruitHeroProtocol(0, delegate(NShared.ChangeRecruitHero.Response response)
			{
				if (response.Result == NShared.ChangeRecruitHero.ResultType.Success)
				{
					GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.Apply(response.FriendlyInfo);
					GetInstance<HeroInnView>().UpdateUI(bSelectHero: true);
					GetInstance<HeroInnView>().InitUIBtnGroup();
				}
				else
				{
					MessagePopup.OpenError(response.Result);
				}
			});
		}, null);
	}

	void HeroInnView.Listener.OnRecruit()
	{
		NShared.RecruitHero.Request request = new NShared.RecruitHero.Request
		{
			HeroIndex = GetInstance<HeroInnView>().HeroInfo.HeroIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RecruitHero.Response response)
		{
			if (response.Result == NShared.RecruitHero.ResultType.Success)
			{
				GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHero(response.HeroResult);
				GetInstance<HeroInnView>().Close();
			}
			else
			{
				MessagePopup.OpenError(response.Result);
			}
		});
	}

	void HeroInnView.Listener.OnShowDetail()
	{
		CachedHeroInfo heroInfo = new CachedHeroInfo(GetInstance<HeroInnView>().HeroInfo);
		GetInstance<HeroDetailView>().Open(heroInfo, this);
	}

	void HeroInnView.Listener.RequestRefresh()
	{
		if (_requestRefresh)
		{
			return;
		}
		PlayerHeroFriendlyInfo[] values = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetValues();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(values))
		{
			_ = values[0];
		}
		if (GetInstance<HeroDetailView>().IsOpen() || !GetInstance<HeroInnView>().IsOpen())
		{
			return;
		}
		_requestRefresh = true;
		RequestNewFriendlyHero(delegate(NShared.RequestNewFriendlyHero.Response response)
		{
			if (response.Result == NShared.RequestNewFriendlyHero.ResultType.Success)
			{
				GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.Apply(response.FriendlyInfo);
				int num = GetInstance<HeroInnView>().HeroIndex;
				if (!GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.IsContainsIndicesHeroIndex(num))
				{
					num = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.GetSelectedHeroIndices()[0];
				}
				HeroInfo heroInfo = GetHeroInfo(num);
				if (heroInfo != null)
				{
					_heroInfo = heroInfo;
					HeroInnView heroInnView = GetInstance<HeroInnView>();
					bool num2 = heroInnView.IsAreadyOpenHero(num);
					RequestRewardMaxClosenessHero(response.MaxClosenessHeroIndice);
					if (num2)
					{
						heroInnView.Reload(_heroInfo);
					}
					else
					{
						heroInnView.Open(_heroInfo, this, heroInfo.HeroIndex == num);
					}
				}
				_requestRefresh = false;
			}
			else
			{
				MessagePopup.OpenError(response.Result);
				GetInstance<HeroInnView>().Close();
				_requestRefresh = false;
			}
		});
	}

	void HeroInnView.Listener.OnChangeHero(int heroIndex)
	{
		OpenHeroInnInternal(heroIndex, null);
	}

	void HeroFullSceneViewBase.Listener.OnOpen()
	{
	}

	void HeroFullSceneViewBase.Listener.OnClose()
	{
	}

	void HeroDetailView.Listener.OnTryBattle(HeroDetailView sender)
	{
		if (HeroDetailView.TryBattle(sender.HeroInfo))
		{
			HeroInnTryBattleReturnState heroInnTryBattleReturnState = new HeroInnTryBattleReturnState();
			heroInnTryBattleReturnState.HeroIndex = sender.HeroInfo.HeroIndex;
			LobbyScene2.ListReturnState.Add(heroInnTryBattleReturnState);
		}
	}

	public void RestoreHeroDetailView(int heroIndex)
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenuReal(chapterIndex, dungeonIndex, skipAni: true, playNPCSound: false, isIgnoreTrigger: false, null);
		OpenHeroInnInternal(heroIndex, null);
	}
}
