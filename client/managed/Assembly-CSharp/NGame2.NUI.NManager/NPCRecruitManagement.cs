using System;
using NGame2.NAccount;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NRender3D.NCinema;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NShared;
using NShared.GiveGiftItem;
using NShared.NLocalization;
using NShared.RecvRewardFriendlyPoint;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;

namespace NGame2.NUI.NManager;

public class NPCRecruitManagement : BaseManagement<NPCRecruitManagement>, NPCRecruitView.Listener, HeroDetailView.Listener, HeroFullSceneViewBase.Listener, ITouchDisabler
{
	bool ITouchDisabler.IsValid => true;

	protected override void Awake()
	{
		base.Awake();
		AddInstance<NPCGiftView>("Resources/Prefab2/Window/NPCRecruit/NPCGiftView");
		AddInstance<NPCFriendshipRewardList>("Resources/Prefab2/Window/NPCRecruit/NPCFriendshipRewardList");
		AddInstance<HeroDetailView>("Resources/Prefab2/Window/FullScene/HeroDetailView");
	}

	public void OpenNPCRecruitView(string npcCode, NpcFriendlyType friendlyType, NpcRecruitType recruitType)
	{
		NPCData data = GameRoot.Instance.NPCDataContainer.GetData(npcCode);
		if (data != null)
		{
			string prefabPath = NPCRecruitView.GetPrefabPath(recruitType);
			NPCRecruitView comp = GetInstance<NPCRecruitView>();
			if (comp == null)
			{
				AddInstance<NPCRecruitView>(prefabPath);
			}
			else if (comp.RecruitType != recruitType)
			{
				CloseInstance<NPCRecruitView>();
				FactoryBase.SafeReleaseComponent(ref comp);
				RemoveInstance(typeof(NPCRecruitView));
				AddInstance<NPCRecruitView>(prefabPath);
			}
			GetInstance<NPCRecruitView>().Open(data, friendlyType, recruitType, this);
		}
	}

	public void OpenNPCGiftView(string npcCode, string titleKey, NpcFriendlyType friendlyType, string[] entryMenuCondtion, string helpListCode, int subHeroIndex)
	{
		NPCData data = GameRoot.Instance.NPCDataContainer.GetData(npcCode);
		if (data != null)
		{
			GetInstance<NPCGiftView>().Open(data, titleKey, friendlyType, entryMenuCondtion, helpListCode, subHeroIndex, this);
		}
	}

	public void OnTakeRewardItem(int heroIndex, int step, NpcFriendlyType type, Action action = null)
	{
		NShared.RecvRewardFriendlyPoint.Request request = new NShared.RecvRewardFriendlyPoint.Request
		{
			HeroIndex = heroIndex,
			Step = step
		};
		GlobalTopmostWindow.instance.AddTouchDisabler(this);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RecvRewardFriendlyPoint.Response response)
		{
			GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
			if (response.Result != NShared.RecvRewardFriendlyPoint.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.Apply(response.FriendlyInfo);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHero(response.HeroInfos, response.ExpResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResultInfos);
				string title = LocalizationManager.GetString(NPCRecruit.NPC_TAKE_FRIENDLY_REWARD_TITLE);
				if (type == NpcFriendlyType.EventGodKing || type == NpcFriendlyType.EventGiftItem)
				{
					title = LocalizationManager.GetStringDirect("GUILD_RAID_CONTRIBUTE_REWARD");
				}
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(title).SetItemResultInfos(response.ItemResults).SetEquipResultInfos(response.EquipItemResults)
					.SetCurrencyResultInfos3(response.CurrencyResults), isGlobal: false);
				if (action != null)
				{
					Vespa_System_Action_Extension_Method.fire(action);
				}
			}
		});
	}

	public void OnGiveGift(int heroIndex, int itemIndex, int itemCount, Action action = null)
	{
		NShared.GiveGiftItem.Request request = new NShared.GiveGiftItem.Request();
		request.HeroIndex = heroIndex;
		request.ItemIndices = new int[1] { itemIndex };
		request.ItemCount = new int[1] { itemCount };
		NShared.GiveGiftItem.Request request2 = request;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request2, delegate(NShared.GiveGiftItem.Response response)
		{
			if (response.Result != NShared.GiveGiftItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.Apply(response.FriendlyInfo);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.EventGiftPointResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				if (action != null)
				{
					Vespa_System_Action_Extension_Method.fire(action);
				}
			}
		});
	}

	void NPCRecruitView.Listener.OnOpenRewardList(NPCFriendlyInfo info, NpcFriendlyType type)
	{
		GetInstance<NPCFriendshipRewardList>().Open(info.HeroIndex, info.Step, type);
	}

	void NPCRecruitView.Listener.OnGiveGift(int heroIndex, int itemIndex, int itemCount)
	{
		OnGiveGift(heroIndex, itemIndex, itemCount);
	}

	void NPCRecruitView.Listener.OnTakeRewardItem(int heroIndex, int step, NpcFriendlyType type)
	{
		OnTakeRewardItem(heroIndex, step, type);
	}

	void NPCRecruitView.Listener.OnTakeRewardItem(int heroIndex, int step, string title, Action<Action> effectAction)
	{
		NShared.RecvRewardFriendlyPoint.Request request = new NShared.RecvRewardFriendlyPoint.Request
		{
			HeroIndex = heroIndex,
			Step = step
		};
		GlobalTopmostWindow.instance.AddTouchDisabler(this);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RecvRewardFriendlyPoint.Response response)
		{
			if (response.Result != NShared.RecvRewardFriendlyPoint.ResultType.Success)
			{
				GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
				MessagePopup.OpenError(response.Result);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.Apply(response.FriendlyInfo);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHero(response.HeroInfos, response.ExpResultInfos);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResultInfos);
				Vespa_System_Action_Extension_Method.fire(effectAction, delegate
				{
					GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
					BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(title).SetItemResultInfos(response.ItemResults).SetEquipResultInfos(response.EquipItemResults)
						.SetCurrencyResultInfos3(response.CurrencyResults), isGlobal: false);
				});
			}
		});
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
			NPCRecruitTryBattleReturnState nPCRecruitTryBattleReturnState = new NPCRecruitTryBattleReturnState();
			NPCRecruitView nPCRecruitView = GetInstance<NPCRecruitView>();
			if (nPCRecruitView != null)
			{
				nPCRecruitTryBattleReturnState.RecruitType = nPCRecruitView.RecruitType;
			}
			LobbyScene2.ListReturnState.Add(nPCRecruitTryBattleReturnState);
		}
	}

	void NPCRecruitView.Listener.OnShowDetail(int heroIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data != null)
		{
			int startHeroStar = data.StartHeroStar;
			CachedHeroInfo referenceHero = SceneAutoBehaviour<StatDataCache>.instance.GetReferenceHero(heroIndex, startHeroStar);
			GetInstance<HeroDetailView>().Open(referenceHero, this);
		}
	}

	public void RestoreHeroDetailView(NpcRecruitType recruitType)
	{
		switch (NPCRecruitView.GetParentWindowType(recruitType))
		{
		case NPCRecruitView.ParentWindowType.Default:
			RestoreByDefaultRecruitView(recruitType);
			break;
		case NPCRecruitView.ParentWindowType.Cinema:
			RestoreByCinemaRecruitView(recruitType);
			break;
		}
	}

	public void RestoreByDefaultRecruitView(NpcRecruitType recruitType)
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenuReal(chapterIndex, dungeonIndex, skipAni: true, playNPCSound: false, isIgnoreTrigger: false, null);
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			OpenNPCRecruitView(data.EntryNPCCode, NpcFriendlyType.Recruit, recruitType);
			int heroIndex = GetInstance<NPCRecruitView>().NPCData.HeroIndex;
			((NPCRecruitView.Listener)this).OnShowDetail(heroIndex);
		}
	}

	public void RestoreByCinemaRecruitView(NpcRecruitType recruitType)
	{
		int CurChapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int CurDungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		CinemaOwner cinemaOwner = SingletonAuto<CinemaOwnerContainer>.instance.GetCinemaOwner(ECinemaOwnerType.NodeTreeEntry);
		SequenceTask onCompleteTask = Vespa_Task_ExtensionMethod.ADDTASK(new SequenceTask().ADDTASK(new WaitTask(() => !cinemaOwner.IsCompleted)).ADDTASK(new WaitTask(() => cinemaOwner.Cinema.IsPlaying)), delegate
		{
			CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(CurChapterIndex, CurDungeonIndex);
			if (data != null)
			{
				OpenNPCRecruitView(data.EntryNPCCode, NpcFriendlyType.Recruit, recruitType);
				int heroIndex = GetInstance<NPCRecruitView>().NPCData.HeroIndex;
				((NPCRecruitView.Listener)this).OnShowDetail(heroIndex);
			}
		});
		SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenuReal_Task(CurChapterIndex, CurDungeonIndex, skipAni: true, playNPCSound: false, isIgnoreTrigger: false, onCompleteTask);
	}
}
