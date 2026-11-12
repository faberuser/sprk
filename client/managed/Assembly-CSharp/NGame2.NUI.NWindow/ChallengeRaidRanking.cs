using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NShared.NRaid;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ChallengeRaidRanking : BaseWindow, ChallengeRaidRankerComponent.Listener, ChallengeRaidMultiRankerComponent.Listener
{
	public enum EChallangeRaidTab
	{
		None,
		SingleRank,
		MultiRank,
		Shop
	}

	[Serializable]
	public class MyInfoMenu
	{
		public UILabel RankTitleLabel;

		public UITexture GuildLogoTexture;

		public UITexture GuildBackTexture;

		public UITexture MyHeroTexture;

		public UILabel MyNameLabel;

		public UILabel MyScoreLabel;

		public UILabel MyClearTimeLabel;

		public UILabel MyRankLabel;

		public UIBaseGrid GridScroll;

		public Transform EmptyRankerRoot;

		public void InitRankUI(Component componentRoot, bool isSingle)
		{
			InitGuildLogo(componentRoot);
			InitMyHero(componentRoot);
			InitMyRank(isSingle);
		}

		private void InitMyHero(Component componentRoot)
		{
			string nick = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick;
			int avatarHeroIndex = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AvatarHeroIndex;
			ChallengeRaidRankerInfo myRankInfo = ChallengeRaidWrapper.MyRankInfo;
			if (myRankInfo != null && myRankInfo.Nick != null)
			{
				if (myRankInfo.Nick != nick)
				{
					myRankInfo.Nick = nick;
				}
				nick = myRankInfo.Nick;
				avatarHeroIndex = myRankInfo.AvatarHeroIndex;
			}
			AssetTexture plainTexture = UIHelper.GetPlainTexture(UIAvatarHelper.GetBattlePortraitPath(avatarHeroIndex));
			UIHelper.ResetPlaneAssetTextureAsync(componentRoot, MyHeroTexture, plainTexture);
			UIHelper.SetLabelText(MyNameLabel, nick);
		}

		private void InitGuildLogo(Component componentRoot)
		{
			if (GlobalAutoBehaviour<GuildManager>.isValidInstance && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info != null)
			{
				MyGuildInfo info = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info;
				UIHelper.ResetPlaneAssetTextureAsync(assetTex: UIHelper.GetGuildLogoIcon2(info.Logo), componentRoot: componentRoot, uiTex: GuildLogoTexture);
				int num = info.Back;
				if (num <= 0 || num > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
				{
					num = 1;
				}
				AssetTexture guildBackGround2D = UIHelper.GetGuildBackGround2D(num);
				UIHelper.ResetPlaneAssetTextureAsync(componentRoot, GuildBackTexture, guildBackGround2D);
				UIHelper.EnableGameObject(GuildBackTexture, enable: true);
			}
			else
			{
				AssetTexture guildBackGround2D2 = UIHelper.GetGuildBackGround2D(1);
				UIHelper.ResetPlaneAssetTextureAsync(componentRoot, GuildBackTexture, guildBackGround2D2);
			}
		}

		private void InitMyRank(bool isSingle)
		{
			string text = LocalizationManager.GetStringDirect("CHALLEGNERAID_NO_BATTLE_TIME");
			string text2 = LocalizationManager.GetStringDirect("CHALLENGE_MY_RANKING_ATTEND_YET");
			string text3 = "0";
			ChallengeRaidRankerInfo myRankInfo = ChallengeRaidWrapper.MyRankInfo;
			string text4 = (isSingle ? LocalizationManager.GetStringDirect("CHALLENGE_RAID_SINGLE_RANK") : LocalizationManager.GetStringDirect("CHALLENGE_RAID_MULTI_HIGHNOTE"));
			UIHelper.SetLabelText(RankTitleLabel, text4);
			bool enable = true;
			if (myRankInfo != null)
			{
				int num = myRankInfo.Rank + 1;
				if (num > 0 && myRankInfo.Score > 0)
				{
					text2 = LocalizationManager.GetString(WorldBoss.WORLDBOSS_MY_RANKING, num);
					text = UIHelper.GetElapseTimeLabelText(myRankInfo.BattleTime);
					text3 = myRankInfo.Score.ToString();
					if (!isSingle)
					{
						enable = false;
					}
				}
			}
			UIHelper.EnableGameObject(MyRankLabel, enable);
			UIHelper.SetLabelText(MyRankLabel, text2);
			UIHelper.SetLabelText(MyScoreLabel, text3);
			UIHelper.SetLabelText(MyClearTimeLabel, text);
		}
	}

	public interface Listener
	{
		bool IsAbleSeason();

		bool IsAbleEnter();

		void OnClickEnter(int raidIndex, int raidLevel);

		void OnClickEnterRank(int raidIndex, int raidLevel);

		void OnClickInfo(int raidIndex);

		void OnClickGuide();

		void OnClickRoomList(int raidIndex);

		void OnClickRankingReward();

		void GetSingleRankerList(int page, Action<ChallengeRaidRankerInfo[]> onList);

		void GetMultiRankerList(Action<MultiPlayChallengeRaidRankerInfo[]> onList);

		void OpenAffixSelectChallenge(int raidIndex, int raidLevel, int[] affixIndices);
	}

	[Header("[Left Menu]")]
	public UIToggle Toggle_SingleRank;

	public UIToggle Toggle_MultiRank;

	public UIToggle Toggle_Shop;

	public UIBaseGrid Grid_LeftMenuTab;

	[Header("[SingleRank]")]
	public UIWidget Widget_SingleRank;

	public MyInfoMenu SingleMyInfo;

	[Header("[MultiRank]")]
	public UIWidget Widget_MultiRank;

	public MyInfoMenu MultiMyInfo;

	[Header("[Shop Info]")]
	public UIWidget Widget_Shop;

	public ShopPanel ShopPanel;

	[Header("[Right Menu]")]
	public UITexture BossImageTexture;

	public UILabel BossNameLabel;

	public Transform RemainTimeOpenedRoot;

	public Transform RemainTimeClosedRoot;

	public UILabel RemainTimeLabel;

	public UILabel PageLabel;

	public UIButton GuideButton;

	public UIButton EnterButton;

	public UIButton InfoButton;

	public const string PrefabPath = "Resources/Prefab2/Window/Raid/ChallengeRaidRanking";

	public const int MaxPageCount = 10;

	private CreatureData _bossCreatureData;

	private RaidData _raidData;

	private ChallengeRaidData _challengeRaidData;

	private ChallengeRaidInfo _challengeRaidInfo;

	public UIButton BtnEnter;

	public UIButton BtnRoomList;

	public UIButton BtnEnterRank;

	private int _page;

	private Listener _listener;

	private List<ChallengeRaidRankerComponent> _singleRankerComponentList = new List<ChallengeRaidRankerComponent>();

	private List<ChallengeRaidMultiRankerComponent> _multiRankerComponentList = new List<ChallengeRaidMultiRankerComponent>();

	private float _lastRefreshInfoTime;

	private const string LOCALE_RAID_CLEAR_NOW = "CHALLENGERAID_REWARD_NOW";

	private EChallangeRaidTab _currentTab;

	private readonly string[] MULTI_CAHLLENGE_GUIDE_STR = new string[6] { "CHALLENGE_POPUP_INFO_TITLE", "", "CHALLENGE_POPUP_INFO_NAME_1", "CHALLENGE_POPUP_INFO_DESC_1", "CHALLENGE_POPUP_INFO_NAME_2", "CHALLENGE_POPUP_INFO_DESC_2" };

	public CreatureData BossCreatureData => _bossCreatureData;

	public RaidData RaidData => _raidData;

	public override void Reset()
	{
		base.Reset();
		ClearSingleRankerList();
		ClearMultiRankerList();
		ShopPanel.ResetShop();
	}

	private void ClearSingleRankerList()
	{
		FactoryBase.ReleaseAndClearList(_singleRankerComponentList);
	}

	private void ClearMultiRankerList()
	{
		FactoryBase.ReleaseAndClearList(_multiRankerComponentList);
	}

	private bool IsChallengeRaidSeason(int challengeRaidIndex)
	{
		if (!SceneAutoBehaviour<RaidManagement>.instance.IsAbleSeason())
		{
			VespaDebug.LogError("ChallengeRaidRanking :: IsAbleSeason() == false");
			return false;
		}
		_challengeRaidData = GameRoot.Instance.ChallengeRaidDataContainer.GetData(challengeRaidIndex);
		if (_challengeRaidData == null)
		{
			VespaDebug.LogError("ChallengeRaidRanking :: challengeRaidData == null / challengeRaidInfo.ChallengeRaidIndex = " + challengeRaidIndex);
			return false;
		}
		return true;
	}

	public void Open(Listener listener, ChallengeRaidInfo challengeRaidInfo, Action onOpen = null)
	{
		if (!SceneAutoBehaviour<RaidManagement>.isValidInstance)
		{
			VespaDebug.LogError("ChallengeRaidRanking :: RaidManagement.isValidInstance == false");
			return;
		}
		if (challengeRaidInfo == null)
		{
			VespaDebug.LogError("ChallengeRaidRanking :: challengeRaidInfo == null");
			return;
		}
		if (!IsChallengeRaidSeason(challengeRaidInfo.ChallengeRaidIndex))
		{
			SceneAutoBehaviour<RaidManagement>.instance.ShowPopupSeasonEnd();
			return;
		}
		_lastRefreshInfoTime = Time.time;
		_listener = listener;
		_challengeRaidInfo = challengeRaidInfo;
		int raidIndex = _challengeRaidInfo.RaidIndex;
		int raidMaxLevel = RaidHelper.GetRaidMaxLevel(raidIndex);
		_raidData = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidMaxLevel);
		if (_raidData == null)
		{
			VespaDebug.LogError("ChallengeRaidRanking :: _raidData == null");
			MessagePopup.OpenError("RaidData is null.");
			return;
		}
		base.Open(delegate
		{
			Init();
			Vespa_System_Action_Extension_Method.fire(onOpen);
		});
	}

	public void Init()
	{
		InitToggle();
		InitRankUI();
		SetBossDesc();
		InitLabel();
		InitButtonState();
	}

	private void InitToggle()
	{
		_currentTab = EChallangeRaidTab.SingleRank;
		if (_raidData.Type == RaidType.Challenge)
		{
			_currentTab = EChallangeRaidTab.MultiRank;
		}
		Toggle_SingleRank.Set(_currentTab == EChallangeRaidTab.SingleRank);
		Toggle_MultiRank.Set(_currentTab == EChallangeRaidTab.MultiRank);
		Toggle_Shop.Set(_currentTab == EChallangeRaidTab.Shop);
		UIHelper.EnableGameObject(Toggle_SingleRank, _currentTab == EChallangeRaidTab.SingleRank);
		UIHelper.EnableGameObject(Toggle_MultiRank, _currentTab == EChallangeRaidTab.MultiRank);
		UIHelper.RepositionGrid(Grid_LeftMenuTab);
	}

	private void InitRankUI()
	{
		UIHelper.EnableGameObject(Widget_SingleRank, _currentTab == EChallangeRaidTab.SingleRank);
		UIHelper.EnableGameObject(Widget_MultiRank, _currentTab == EChallangeRaidTab.MultiRank);
		UIHelper.EnableGameObject(Widget_Shop, _currentTab == EChallangeRaidTab.Shop);
		if (_raidData.Type == RaidType.Challenge)
		{
			InitMultiRankUIByRankType();
		}
		else
		{
			InitSingleRankUIByRankType();
		}
	}

	private void InitSingleRankUIByRankType()
	{
		if (_challengeRaidData != null)
		{
			SingleMyInfo.InitRankUI(this, isSingle: true);
			InitEnterButtons(isSingle: true);
			RequestSingleRanker(0);
		}
	}

	private void InitMultiRankUIByRankType()
	{
		if (_challengeRaidData != null)
		{
			MultiMyInfo.InitRankUI(this, isSingle: false);
			InitEnterButtons(isSingle: false);
			RequestMultiRanker();
		}
	}

	private void InitShop()
	{
		ShopPanel.ResetShop();
		if (_challengeRaidData.ShopIndex <= 0)
		{
			VespaDebug.LogError("ChallengeRaidRanking :: challengeRaidData ShopIndex is 0 / challengeRaidInfo.ChallengeRaidIndex = " + _challengeRaidData.Index);
			return;
		}
		ShopData data = GameRoot.Instance.ShopDataContainer.GetData(_challengeRaidData.ShopIndex);
		ShopPanel.Init(data, null, ShopPanel.ECompType.ChallengeRaid);
	}

	private void InitEnterButtons(bool isSingle)
	{
		UIHelper.EnableGameObject(BtnEnter, !isSingle, isForce: true);
		UIHelper.EnableGameObject(BtnRoomList, !isSingle, isForce: true);
		UIHelper.EnableGameObject(BtnEnterRank, isSingle, isForce: true);
	}

	private void InitButtonState()
	{
		if (_listener == null || !_listener.IsAbleEnter())
		{
			SetButtonActive(isEnable: false);
		}
		else
		{
			SetButtonActive(isEnable: true);
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		RefreshInfo();
		ShopPanel.UpdateShop();
	}

	private void SetBossDesc()
	{
		if (_challengeRaidData != null && !String_Extension_Method.IsNullOrEmpty(_challengeRaidData.RankingBossImage))
		{
			AssetTexture assetTexture = new AssetTexture();
			assetTexture.Open(_challengeRaidData.RankingBossImage);
			UIHelper.ResetPlaneAssetTextureAsync(this, BossImageTexture, assetTexture);
		}
		int chapterIndex = _raidData.ChapterIndex;
		int dungeonIndex = _raidData.DungeonIndex;
		NShared.DungeonDifficultyType difficulty = (NShared.DungeonDifficultyType)Helper.GetDifficulty(chapterIndex, dungeonIndex);
		_bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, difficulty, scenario: false, out var bossWaveData);
		if (_bossCreatureData == null)
		{
			VespaDebug.LogError("ChallengeRaidRanking:: BossDesc not found: chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			return;
		}
		string creatureName = string.Empty;
		string creatureNamePrefix = string.Empty;
		string visualInfo = bossWaveData?.BossVisualInfo;
		GameRoot.Instance.CreatureVisualDataContainer.GetCreatureName(_bossCreatureData, visualInfo, LocalizationManager.GetStringDirect, out creatureName, out creatureNamePrefix);
		string arg = ((_raidData.Type == RaidType.Challenge) ? LocalizationManager.GetStringDirect("CHALLENGE_RAID_SEASON_MULTI") : LocalizationManager.GetStringDirect("CHALLENGE_RAID_SEASON_SINGLE"));
		string text = $"{creatureName} {arg}";
		UIHelper.SetLabelText(BossNameLabel, text);
	}

	private void InitLabel()
	{
		if (_listener == null || !_listener.IsAbleEnter())
		{
			SetLabel(isOpened: false);
			return;
		}
		int remainTime = SceneAutoBehaviour<RaidManagement>.instance.GetRemainTime();
		bool isOpened = remainTime > 0;
		SetLabel(isOpened, remainTime);
	}

	private void SetLabel(bool isOpened, int remainTime = 0)
	{
		UIHelper.EnableGameObject(RemainTimeOpenedRoot, isOpened);
		UIHelper.EnableGameObject(RemainTimeClosedRoot, !isOpened);
		if (isOpened)
		{
			UILabel remainTimeLabel = RemainTimeLabel;
			object key = GuildRaid.GUILDRAID_STATE_OPENED;
			object[] array = new object[1];
			array[0] = UIHelper.GetRemainTimeLabelText(remainTime);
			UIHelper.SetLabelText(remainTimeLabel, LocalizationManager.GetString((Enum)key, array));
		}
	}

	private void RefreshInfo()
	{
		if (!(Time.time - _lastRefreshInfoTime < 1f))
		{
			_lastRefreshInfoTime = Time.time;
			InitLabel();
			InitButtonState();
		}
	}

	private void SetButtonActive(bool isEnable)
	{
		UIHelper.EnableButton(BtnEnter, isEnable, grayscaleIfDisable: true);
		UIHelper.EnableButton(BtnRoomList, isEnable, grayscaleIfDisable: true);
	}

	private void RefreshTabInfo()
	{
		UIHelper.EnableGameObject(Widget_SingleRank, _currentTab == EChallangeRaidTab.SingleRank);
		UIHelper.EnableGameObject(Widget_MultiRank, _currentTab == EChallangeRaidTab.MultiRank);
		UIHelper.EnableGameObject(Widget_Shop, _currentTab == EChallangeRaidTab.Shop);
		switch (_currentTab)
		{
		case EChallangeRaidTab.SingleRank:
			InitSingleRankUIByRankType();
			break;
		case EChallangeRaidTab.MultiRank:
			InitMultiRankUIByRankType();
			break;
		case EChallangeRaidTab.Shop:
			InitShop();
			break;
		}
	}

	private void RequestSingleRanker(int page)
	{
		_page = page;
		UIHelper.SetLabelText(PageLabel, page + 1 + "/" + 10);
		if (_listener != null)
		{
			_listener.GetSingleRankerList(_page, RefreshSingleRanker);
		}
	}

	private void RefreshSingleRanker(ChallengeRaidRankerInfo[] rankerInfos)
	{
		ClearSingleRankerList();
		if (rankerInfos == null)
		{
			UIHelper.EnableGameObject(SingleMyInfo.EmptyRankerRoot, enable: true);
			return;
		}
		foreach (ChallengeRaidRankerInfo rankerInfo in rankerInfos)
		{
			ChallengeRaidRankerComponent challengeRaidRankerComponent = UIHelper.AddGridItem(SingleMyInfo.GridScroll, ChallengeRaidRankerComponent.PrefabAuto);
			challengeRaidRankerComponent.SetData(rankerInfo, this);
			_singleRankerComponentList.Add(challengeRaidRankerComponent);
		}
		SingletonUpdater<Executer>.instance.NextRun(delegate
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_singleRankerComponentList); j++)
			{
				UIHelper.RefreshComponent(_singleRankerComponentList[j]);
			}
		});
		UIHelper.RepositionGrid(SingleMyInfo.GridScroll);
		UIHelper.EnableGameObject(SingleMyInfo.EmptyRankerRoot, _singleRankerComponentList.Count == 0);
	}

	private void RequestMultiRanker()
	{
		if (_listener != null)
		{
			_listener.GetMultiRankerList(RefreshMultiRanker);
		}
	}

	private void RefreshMultiRanker(MultiPlayChallengeRaidRankerInfo[] rankerInfos)
	{
		ClearMultiRankerList();
		if (rankerInfos == null)
		{
			UIHelper.EnableGameObject(MultiMyInfo.EmptyRankerRoot, enable: true);
			return;
		}
		foreach (MultiPlayChallengeRaidRankerInfo rankerInfo in rankerInfos)
		{
			ChallengeRaidMultiRankerComponent challengeRaidMultiRankerComponent = UIHelper.AddGridItem(MultiMyInfo.GridScroll, ChallengeRaidMultiRankerComponent.PrefabAuto);
			challengeRaidMultiRankerComponent.SetData(rankerInfo, this);
			_multiRankerComponentList.Add(challengeRaidMultiRankerComponent);
		}
		SingletonUpdater<Executer>.instance.NextRun(delegate
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_multiRankerComponentList); j++)
			{
				UIHelper.RefreshComponent(_multiRankerComponentList[j]);
			}
		});
		UIHelper.RepositionGrid(MultiMyInfo.GridScroll);
		UIHelper.EnableGameObject(MultiMyInfo.EmptyRankerRoot, __Extension_Method_ICollection__.COUNT(_multiRankerComponentList) == 0);
	}

	public void OnToggleChanged()
	{
		EChallangeRaidTab eChallangeRaidTab = EChallangeRaidTab.None;
		if (Toggle_SingleRank.value)
		{
			eChallangeRaidTab = EChallangeRaidTab.SingleRank;
		}
		else if (Toggle_MultiRank.value)
		{
			eChallangeRaidTab = EChallangeRaidTab.MultiRank;
		}
		else if (Toggle_Shop.value)
		{
			eChallangeRaidTab = EChallangeRaidTab.Shop;
		}
		if (eChallangeRaidTab != EChallangeRaidTab.None && _currentTab != eChallangeRaidTab)
		{
			_currentTab = eChallangeRaidTab;
			RefreshTabInfo();
		}
	}

	public void OnClickEnter()
	{
		if (SceneAutoBehaviour<MultiPartyManagement>.instance.CheckEnterBattle() && _listener != null)
		{
			if (!_listener.IsAbleEnter())
			{
				SetButtonActive(isEnable: false);
				SceneAutoBehaviour<RaidManagement>.instance.ShowPopupBattleEnd();
			}
			else
			{
				int raidMaxLevel = RaidHelper.GetRaidMaxLevel(_raidData.Index);
				_listener.OnClickEnter(_raidData.Index, raidMaxLevel);
			}
		}
	}

	public void OnClickEnterRank()
	{
		if (_listener != null)
		{
			if (!_listener.IsAbleEnter())
			{
				SetButtonActive(isEnable: false);
				SceneAutoBehaviour<RaidManagement>.instance.ShowPopupBattleEnd();
			}
			else
			{
				int raidMaxLevel = RaidHelper.GetRaidMaxLevel(_raidData.Index);
				_listener.OnClickEnterRank(_raidData.Index, raidMaxLevel);
			}
		}
	}

	public void OnClickRoomList()
	{
		if (_listener != null)
		{
			if (!_listener.IsAbleSeason())
			{
				SetButtonActive(isEnable: false);
				SceneAutoBehaviour<RaidManagement>.instance.ShowPopupSeasonEnd();
			}
			else
			{
				_listener.OnClickRoomList(_raidData.Index);
			}
		}
	}

	public void OnClickInfo()
	{
		if (_listener != null)
		{
			_listener.OnClickInfo(_challengeRaidInfo.RaidIndex);
		}
	}

	public void OnClickGuide()
	{
		if (_listener != null)
		{
			_listener.OnClickGuide();
		}
	}

	public void OnClickRankingReward()
	{
		if (_listener != null)
		{
			_listener.OnClickRankingReward();
		}
	}

	public void OnClickNextPage()
	{
		if (_page < 9)
		{
			int page = _page + 1;
			RequestSingleRanker(page);
		}
	}

	public void OnClickPrevPage()
	{
		if (_page > 0)
		{
			int page = _page - 1;
			RequestSingleRanker(page);
		}
	}

	public void OnClickBuyItem()
	{
		ShopPanel.OnClickBuy();
	}

	public void OnClickMultiChallengeGuide()
	{
		EventGuidePopup.Open(MULTI_CAHLLENGE_GUIDE_STR);
	}

	void ChallengeRaidRankerComponent.Listener.OnClickComponent(long accountId)
	{
		NGame2.NGuild.GuildMemberManager memberManager = GlobalAutoBehaviour<GuildManager>.instance.MemberManager;
		if (memberManager != null)
		{
			GuildMemberInfo guildMemberInfo = memberManager.Get(accountId);
			if (guildMemberInfo != null && guildMemberInfo.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
			{
				InspectUserManagement.InspectUserData inspectUser = new InspectUserManagement.InspectUserData(guildMemberInfo, InspectUserCategory.WorldBoss);
				SceneAutoBehaviour<InspectUserManagement>.instance.OpenMenuPopup(inspectUser);
			}
		}
	}

	void ChallengeRaidMultiRankerComponent.Listener.OnClickChallenge(MultiPlayChallengeRaidRankerInfo rankerInfo)
	{
		if (_listener != null && rankerInfo != null && _raidData != null)
		{
			int index = _raidData.Index;
			int raidLevel = rankerInfo.RaidLevel;
			int[] affixIndices = rankerInfo.AffixIndices;
			_listener.OpenAffixSelectChallenge(index, raidLevel, affixIndices);
		}
	}
}
