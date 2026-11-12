using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NChatting;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NShared;
using NShared.InspectUser;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;

namespace NGame2.NUI.NManager;

public class InspectUserManagement : BaseManagement<InspectUserManagement>, InspectUserList.Listener, HeroFullSceneViewBase.Listener, HeroDetailView.Listener
{
	public struct InspectUserData
	{
		public long AccountId { get; private set; }

		public int AvatarHeroIndex { get; private set; }

		public int TeamLevel { get; private set; }

		public string Nickname { get; private set; }

		public long RoomNo { get; private set; }

		public InspectUserCategory Category { get; private set; }

		public InspectUserData(long accountId, int avatarHeroIndex, int teamLevel, string nickname, InspectUserCategory category)
		{
			AccountId = accountId;
			AvatarHeroIndex = avatarHeroIndex;
			TeamLevel = teamLevel;
			Nickname = nickname;
			Category = category;
			RoomNo = 0L;
		}

		public InspectUserData(CachedUserInfo userInfo, InspectUserCategory category)
		{
			AccountId = userInfo.AccountId;
			AvatarHeroIndex = userInfo.AvatarHeroIndex;
			TeamLevel = userInfo.TeamLevel;
			Nickname = userInfo.Nick;
			Category = category;
			RoomNo = 0L;
		}

		public void SetRoomNo(long no)
		{
			RoomNo = no;
		}
	}

	private CachedAccountInfo _accountInfo;

	private CachedHeroInfo _heroInfo;

	private int[] _deckArray;

	private InspectUserCategory _inspectUserCategory;

	private VespaCoroutine.RuntimeData _inspectRuntime;

	protected override void Awake()
	{
		base.Awake();
		AddInstance<InspectUserList>("Resources/Prefab2/Window/Hero/InspectUserList");
		AddInstance<UserHeroRunehengeWindow>("Resources/Prefab2/Window/Hero/UserHeroRunehenge");
		AddInstance<HeroDetailView>("Resources/Prefab2/Window/FullScene/HeroDetailView");
		AddInstance<InspectEquipInfo>("Resources/Prefab2/Window/Hero/InspectEquipInfo");
		AddInstance<InspectEquipTreasure>("Resources/Prefab2/Window/Hero/InspectEquipTreasure");
		AddInstance<RecommendClearPartyPopup>("Resources/Prefab2/Window/RecommendClearParty/RecommendClearPartyPopup");
	}

	public void OpenMenuPopup(InspectUserData inspectUser)
	{
		if (inspectUser.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
		{
			InspectUserMenuPopup.Open(inspectUser);
		}
	}

	public void RequestInspectUser(InspectUserData inspectUser)
	{
		if (_inspectRuntime != null)
		{
			VespaCoroutine.Stop(_inspectRuntime);
			_inspectRuntime = null;
		}
		_inspectRuntime = VespaCoroutine.StartRuntime(coRequestInspectUser(inspectUser.AccountId, inspectUser.Category));
	}

	private IEnumerator coRequestInspectUser(long accountId, InspectUserCategory category)
	{
		NShared.InspectUser.Response response = null;
		bool responsed = false;
		MyFriendManagement.SendInspectUser(accountId, delegate(NShared.InspectUser.Response r)
		{
			response = r;
			responsed = true;
		});
		while (!responsed)
		{
			yield return null;
		}
		if (response == null)
		{
			yield break;
		}
		CachedAccountInfo cachedAccountInfo = response.CachedAccountInfo;
		if (cachedAccountInfo == null)
		{
			yield break;
		}
		GuildInfo guildInfo = response.GuildInfo;
		int matchTierRank = response.MatchTierRank;
		_accountInfo = cachedAccountInfo;
		_inspectUserCategory = category;
		DeckInfo deckInfo = null;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(cachedAccountInfo.DeckInfos))
		{
			if (_inspectUserCategory == InspectUserCategory.Arena && cachedAccountInfo.DeckInfos.ContainsKey(BattleType.Arena))
			{
				deckInfo = cachedAccountInfo.DeckInfos[BattleType.Arena];
			}
			else if (cachedAccountInfo.DeckInfos.ContainsKey(BattleType.Campaign))
			{
				deckInfo = cachedAccountInfo.DeckInfos[BattleType.Campaign];
			}
		}
		int[] deckArray = null;
		if (deckInfo != null)
		{
			deckArray = new int[4] { deckInfo.HeroIndex1, deckInfo.HeroIndex2, deckInfo.HeroIndex3, deckInfo.HeroIndex4 };
		}
		InspectUserInfoPopup.Open(_inspectUserCategory, cachedAccountInfo, deckInfo, guildInfo, matchTierRank, delegate
		{
			OpenInspectUserListInternal(deckArray, -1, bImmediate: false, delegate
			{
				if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI() && GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<InspectUserList>())
				{
					InspectUserList window = GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<InspectUserList>();
					if (window != null)
					{
						LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3DUI);
						window.ChangeLayerRecursive(layerInfo);
					}
				}
			});
		});
	}

	public void OpenRunehengeWindow()
	{
		UserHeroRunehengeWindow userHeroRunehengeWindow = GetInstance<UserHeroRunehengeWindow>();
		if (!(userHeroRunehengeWindow == null))
		{
			userHeroRunehengeWindow.Open(_heroInfo);
		}
	}

	private void OpenInspectUserListInternal(int[] deckArray = null, int heroIndex = -1, bool bImmediate = false, Action onComplete = null, Action onError = null)
	{
		if (_accountInfo != null)
		{
			Dictionary<int, CachedHeroInfo> heroInfos = _accountInfo.HeroInfos;
			CachedHeroInfo[] heroInfosArr = heroInfos.Values.ToArray();
			Array.Sort(heroInfosArr, new HeroComparer());
			if (heroIndex == -1)
			{
				heroIndex = heroInfosArr.First().HeroIndex;
			}
			CachedHeroInfo[] array = heroInfosArr;
			foreach (CachedHeroInfo heroInfo in array)
			{
				if (heroInfo.HeroIndex == heroIndex)
				{
					_deckArray = deckArray;
					_heroInfo = heroInfo;
					new GroupTask().ADDTASK(base.TaskFactory.Open(delegate(InspectUserList window)
					{
						window.Init(this, heroInfosArr, heroInfo.HeroIndex, deckArray);
					}, bImmediate)).ADDTASK(base.TaskFactory.Run(onComplete)).Commit();
					return;
				}
			}
		}
		Vespa_System_Action_Extension_Method.fire(onError);
	}

	public void OpenInspectUserList(InspectUserList.Listener listener, CachedHeroInfo[] heroInfos, int selectedHero = 0, int[] deckArray = null)
	{
		InspectUserList inspectUserList = GetInstance<InspectUserList>();
		if (!(inspectUserList == null) && !inspectUserList.IsOpeningWindowAnimating() && __Extension_Method_ICollection__.COUNT(heroInfos) != 0)
		{
			if (listener == null)
			{
				listener = this;
			}
			if (selectedHero == 0)
			{
				selectedHero = heroInfos[0].HeroIndex;
			}
			_heroInfo = Array.Find(heroInfos, (CachedHeroInfo info) => info.HeroIndex == selectedHero);
			inspectUserList.Open(listener, heroInfos, selectedHero, deckArray);
		}
	}

	private IEnumerator coRestoreArena(InspectUserReturnToArenaParam param)
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		SceneAutoBehaviour<MatchManagement>.instance.OpenMatchSelect(immediately: true);
		SceneAutoBehaviour<MatchManagement>.instance.OpenMatchType2(param.MatchType);
		_accountInfo = param.AccountInfo;
		OpenInspectUserListInternal(param.DeckArray, param.HeroIndex, bImmediate: true);
		GetInstance<HeroDetailView>().Open(_heroInfo, this);
		while (!GetInstance<HeroDetailView>()._sceneChangerFinished)
		{
			yield return null;
		}
		sceneChanger.Close();
	}

	private IEnumerator coRestoreFriend(InspectUserReturnToFriendParam param)
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		SceneAutoBehaviour<MyFriendManagement>.instance.OpenFriendMenu(param._lastViewToggle);
		_accountInfo = param.AccountInfo;
		OpenInspectUserListInternal(param.DeckArray, param.HeroIndex, bImmediate: true);
		GetInstance<HeroDetailView>().Open(_heroInfo, this);
		while (!GetInstance<HeroDetailView>()._sceneChangerFinished)
		{
			yield return null;
		}
		sceneChanger.Close();
	}

	private IEnumerator coRestoreGuild(InspectUserReturnParam param)
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		SceneAutoBehaviour<GuildManagement2>.instance.OpenGuildManage();
		_accountInfo = param.AccountInfo;
		OpenInspectUserListInternal(param.DeckArray, param.HeroIndex, bImmediate: true);
		GetInstance<HeroDetailView>().Open(_heroInfo, this);
		while (!GetInstance<HeroDetailView>()._sceneChangerFinished)
		{
			yield return null;
		}
		sceneChanger.Close();
	}

	public void RestoreToArena(InspectUserReturnToArenaParam param)
	{
		VespaCoroutine.Start(coRestoreArena(param));
	}

	public void RestoreToFriend(InspectUserReturnToFriendParam param)
	{
		VespaCoroutine.Start(coRestoreFriend(param));
	}

	public void RestoreToGuild(InspectUserReturnParam param)
	{
		VespaCoroutine.Start(coRestoreGuild(param));
	}

	private void ShowEquipInfo(ItemIconComponent itemIconComponent)
	{
		if (!(itemIconComponent == null))
		{
			EquipItemInfo equipItemInfo = itemIconComponent.EquipItemInfo;
			ShowEquipInfo(equipItemInfo);
		}
	}

	private void ShowEquipInfo(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			GetInstance<InspectEquipInfo>().Open(_accountInfo, _heroInfo, equipItemInfo);
		}
	}

	void HeroDetailView.Listener.OnTryBattle(HeroDetailView sender)
	{
		if (HeroDetailView.TryBattle(sender.HeroInfo))
		{
			if (_inspectUserCategory == InspectUserCategory.Friend)
			{
				InspectUserReturnToFriendParam param = new InspectUserReturnToFriendParam(SceneAutoBehaviour<MyFriendManagement>.instance.friendMenuToggle, _accountInfo, _heroInfo.HeroIndex, _deckArray);
				LobbyScene2.ListReturnState.Add(new InspectUserTryBattleReturnToFriend(param));
			}
			else if (_inspectUserCategory == InspectUserCategory.Guild)
			{
				InspectUserReturnParam param2 = new InspectUserReturnParam(_accountInfo, _heroInfo.HeroIndex, _deckArray);
				LobbyScene2.ListReturnState.Add(new InspectUserTryBattleReturnToGuild(param2));
			}
		}
	}

	void HeroFullSceneViewBase.Listener.OnOpen()
	{
		if (UIHelper.IsEnabledGameObjet(GetInstance<InspectUserList>()))
		{
			GetInstance<InspectUserList>().ActivatePreview(isActive: false);
		}
	}

	void HeroFullSceneViewBase.Listener.OnClose()
	{
		if (UIHelper.IsEnabledGameObjet(GetInstance<InspectUserList>()))
		{
			GetInstance<InspectUserList>().ActivatePreview(isActive: true);
		}
	}

	void InspectUserList.Listener.OnClickDetailView()
	{
		if (_heroInfo != null)
		{
			GetInstance<HeroDetailView>().Open(_heroInfo, this);
		}
	}

	void InspectUserList.Listener.OnClickEquipIcon(ItemIconComponent caller)
	{
		if (!(caller == null) && _heroInfo != null)
		{
			if (caller.EquipPartType == EquipPartType.Treasure)
			{
				GetInstance<InspectEquipTreasure>().Open(null, _heroInfo);
			}
			else if (caller.EquipItemInfo != null)
			{
				ShowEquipInfo(caller);
			}
		}
	}

	void InspectUserList.Listener.OnClickHeroPortrait(HeroPortraitComponent heroPortrait)
	{
		_heroInfo = heroPortrait.HeroInfo;
		CloseOpenedInfoWindow();
		GetInstance<InspectUserList>().SetupHeroInfo(_heroInfo);
		UserHeroRunehengeWindow userHeroRunehengeWindow = GetInstance<UserHeroRunehengeWindow>();
		if (userHeroRunehengeWindow != null && userHeroRunehengeWindow.IsOpen())
		{
			userHeroRunehengeWindow.Close();
		}
	}

	void InspectUserList.Listener.OnClickChatting()
	{
		SceneAutoBehaviour<MailManagement>.instance.CloseAllActiveWindow();
		SceneAutoBehaviour<PayShopManagement>.instance.CloseAllActiveWindow();
		if (!GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<ChattingWindow>())
		{
			SceneAutoBehaviour<ChattingManagement>.instance.OpenChatting(ChatType.None, 0L);
		}
		else
		{
			SceneAutoBehaviour<ChattingManagement>.instance.CloseAllActiveWindow();
		}
	}

	void InspectUserList.Listener.OnClose()
	{
		CloseOpenedInfoWindow();
	}

	private void CloseOpenedInfoWindow()
	{
		CloseIfOpen(GetInstance<InspectEquipInfo>());
		CloseIfOpen(GetInstance<InspectEquipTreasure>());
		CloseIfOpen(GetInstance<UserHeroRunehengeWindow>());
	}

	private void CloseIfOpen<T>(T window) where T : BaseWindow
	{
		if ((bool)window && window.IsOpen())
		{
			window.Close();
		}
	}

	public void OpenRecommendClearPartyPopup(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		GetInstance<RecommendClearPartyPopup>().Open(chapterIndex, dungeonIndex, difficulty);
	}

	void InspectUserList.Listener.OnClickRunehengeIcon()
	{
		OpenRunehengeWindow();
	}
}
