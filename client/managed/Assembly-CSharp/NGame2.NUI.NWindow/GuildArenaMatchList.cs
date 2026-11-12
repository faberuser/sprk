using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NTutorial;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NNguiExtension;
using NShared;
using NShared.GetGuildArenaGroup;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildArenaMatchList : BaseWindow, GuildArenaMemberComponent.Listener, GuildArenaDeckListPopup.Listener
{
	public interface Listener
	{
		void OnClickDeckInfo(GuildArenaMemberDeckInfo memberInfo);

		void OnClickDeckAttack(GuildArenaMemberDeckInfo memberInfo);

		void OnClickHistory();
	}

	private enum SORTINGOPTION
	{
		LIFE,
		TEAMLEVEL
	}

	private enum SORTINGORDER
	{
		DESCENDING,
		ASCENDING
	}

	private Listener _listener;

	public UIScrollGrid LeftMemberListGrid;

	private List<GuildArenaMemberComponent> _leftMemberComponentList = new List<GuildArenaMemberComponent>();

	public UIScrollGrid RightMemberListGrid;

	private List<GuildArenaMemberComponent> _rightMemberComponentList = new List<GuildArenaMemberComponent>();

	public UILabel Label_MyGuildname;

	public UILabel Label_MyGuildSessionScore;

	public UITexture Texture_MyGuildLogo;

	public UITexture Texture_MyGuildBack;

	public UITexture Texture_MyGuildCountryFlag;

	public UILabel Label_EnemyGuildname;

	public UILabel Label_EnemyGuildSessionScore;

	public UITexture Texture_EnemyGuildLogo;

	public UITexture Texture_EnemyGuildBack;

	public UITexture Texture_EnemyGuildCountryFlag;

	public UILabel Label_Stamina;

	public UILabel Label_GuildPoint;

	public UILabel Label_EndTime;

	public UIComboBox SortTeamComboBox;

	public UIComboBox SortOptionComboBox;

	private SORTINGOPTION _rightTeamSortingOption;

	private SORTINGORDER _rightTeamSortingOrder;

	private int _sortingFactor = 1;

	public static long LastOpenedAccountId;

	public const string PrefabPath = "Resources/Prefab2/Window/GuildArena/GuildArenaMatchList";

	private float _lastRefreshInfoTime;

	public int _mySessionScore { get; set; }

	public int[] _battledHeroIndices { get; set; }

	private GuildArenaRankerInfo[] _rankers { get; set; }

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public void Open(Listener listener, NShared.GetGuildArenaGroup.Response response, Action onOpen = null)
	{
		Clear();
		_listener = listener;
		_mySessionScore = response.MySessionScore;
		_battledHeroIndices = response.BattledHeroIndices;
		_rankers = response.Rankers;
		base.Open(delegate
		{
			Init(response);
		});
	}

	public override void Close()
	{
		base.Close();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		RefreshTime();
	}

	private void RefreshTime()
	{
		if (!SingletonUpdater<TutorialManager>.instance.IsProgressing && !(Time.time - _lastRefreshInfoTime < 1f))
		{
			_lastRefreshInfoTime = Time.time;
			InitBattleEndTimeLabel();
		}
	}

	private void InitBattleEndTimeLabel()
	{
		GuildArenaSessionInfo guildArenaSessionInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSessionInfo();
		if (guildArenaSessionInfo != null && Vespa_String_Extension_Method2.TryParseNetworkTime2(guildArenaSessionInfo.BattleEndTime, out var o_dateTime))
		{
			int num = Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
			long remainTime = num;
			UIHelper.SetLabelText(Label_EndTime, UIHelper.GetRemainTimeLabelText(remainTime));
		}
	}

	public void Clear()
	{
		_sortingFactor = 1;
		_rightTeamSortingOption = SORTINGOPTION.LIFE;
		_rightTeamSortingOrder = SORTINGORDER.DESCENDING;
		FactoryBase.ReleaseAndClearList(_rightMemberComponentList);
		FactoryBase.ReleaseAndClearList(_leftMemberComponentList);
		UIHelper.ClearPlaneAssetTexture(Texture_MyGuildLogo);
		UIHelper.ClearPlaneAssetTexture(Texture_MyGuildBack);
		UIHelper.ClearPlaneAssetTexture(Texture_MyGuildCountryFlag);
		UIHelper.ClearPlaneAssetTexture(Texture_EnemyGuildLogo);
		UIHelper.ClearPlaneAssetTexture(Texture_EnemyGuildBack);
		UIHelper.ClearPlaneAssetTexture(Texture_EnemyGuildCountryFlag);
	}

	public void Init(NShared.GetGuildArenaGroup.Response response)
	{
		InitSortComboBox();
		InitMemberList(TeamType.Left, response.LeftMemberDeckInfos, _leftMemberComponentList, LeftMemberListGrid);
		InitMemberList(TeamType.Right, response.RightMemberDeckInfos, _rightMemberComponentList, RightMemberListGrid);
		InitSort();
		InitLabels();
		InitTextures();
	}

	private void InitTextures()
	{
		GuildArenaRankerInfo myGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo();
		GuildArenaRankerInfo enemyGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetEnemyGuildInfo();
		if (enemyGuildInfo == null)
		{
			return;
		}
		string countryCode = enemyGuildInfo.CountryCode;
		CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
		if (dataByCode != null)
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_EnemyGuildCountryFlag, plainTexture);
		}
		AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2((enemyGuildInfo.Logo <= 0) ? 1 : enemyGuildInfo.Logo);
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_EnemyGuildLogo, guildLogoIcon);
		byte b = enemyGuildInfo.Back;
		if (b <= 0 || b > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
		{
			b = 1;
		}
		AssetTexture guildBackGround2D = UIHelper.GetGuildBackGround2D(b);
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_EnemyGuildBack, guildBackGround2D);
		if (myGuildInfo != null)
		{
			string countryCode2 = myGuildInfo.CountryCode;
			CountryCodeData dataByCode2 = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode2);
			if (dataByCode2 != null)
			{
				AssetTexture plainTexture2 = UIHelper.GetPlainTexture(dataByCode2.IconPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, Texture_MyGuildCountryFlag, plainTexture2);
			}
			AssetTexture guildLogoIcon2 = UIHelper.GetGuildLogoIcon2((myGuildInfo.Logo <= 0) ? 1 : myGuildInfo.Logo);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_MyGuildLogo, guildLogoIcon2);
			byte b2 = myGuildInfo.Back;
			if (b2 <= 0 || b2 > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
			{
				b2 = 1;
			}
			AssetTexture guildBackGround2D2 = UIHelper.GetGuildBackGround2D(b2);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_MyGuildBack, guildBackGround2D2);
		}
	}

	public void InitSortComboBox()
	{
		SortTeamComboBox.Clear();
		SortOptionComboBox.Clear();
		SortTeamComboBox.OnChanged = OnClickSortOption;
		SortOptionComboBox.OnChanged = OnClickSortOrder;
		SortTeamComboBox.AddItem(0, LocalizationManager.GetStringDirect("GUILD_ARENA_DEFENCE_TEAM"));
		SortTeamComboBox.AddItem(1, LocalizationManager.GetStringDirect("COMMON_ACCOUNT_LEVEL"));
		SortOptionComboBox.AddItem(0, LocalizationManager.GetStringDirect("COMMON.DESCENDING_ORDER"));
		SortOptionComboBox.AddItem(1, LocalizationManager.GetStringDirect("COMMON.ASCENDING_ORDER"));
		SortTeamComboBox.Reposition();
		SortOptionComboBox.Reposition();
	}

	public void InitSort()
	{
		_rightMemberComponentList.Sort(CompareLifeForList);
		RightMemberListGrid.Grid.sorting = UIGrid.Sorting.Custom;
		RightMemberListGrid.Grid.onCustomSort = CompareLifeForGrid;
		_leftMemberComponentList.Sort(CompareMyGuildForList);
		LeftMemberListGrid.Grid.sorting = UIGrid.Sorting.Custom;
		LeftMemberListGrid.Grid.onCustomSort = CompareMyGuildForGrid;
		SingletonUpdater<Executer>.instance.FrameDelayRun(2, this, delegate
		{
			UIHelper.RepositionGrid(RightMemberListGrid);
			UIHelper.RepositionGrid(LeftMemberListGrid);
		});
	}

	public int CompareLifeForGrid(Transform A, Transform B)
	{
		GuildArenaMemberComponent component = A.GetComponent<GuildArenaMemberComponent>();
		GuildArenaMemberComponent component2 = B.GetComponent<GuildArenaMemberComponent>();
		if (component == null)
		{
			return _sortingFactor;
		}
		if (component2 == null)
		{
			return -1 * _sortingFactor;
		}
		if (component == component2)
		{
			return 0;
		}
		return CompareLifeForList(component, component2);
	}

	public int CompareLifeForList(GuildArenaMemberComponent A, GuildArenaMemberComponent B)
	{
		if (A == null)
		{
			return _sortingFactor;
		}
		if (B == null)
		{
			return -1 * _sortingFactor;
		}
		if (A == B)
		{
			return 0;
		}
		GuildArenaMemberDeckInfo memberDeckInfo = A.MemberDeckInfo;
		GuildArenaMemberDeckInfo memberDeckInfo2 = B.MemberDeckInfo;
		if (memberDeckInfo.DefeatDeck == null)
		{
			return -1 * _sortingFactor;
		}
		if (memberDeckInfo2.DefeatDeck == null)
		{
			return _sortingFactor;
		}
		int num = 0;
		for (int i = 0; i < memberDeckInfo.DeckCount; i++)
		{
			if (memberDeckInfo.DefeatDeck[i] == 0)
			{
				num++;
			}
		}
		int num2 = 0;
		for (int j = 0; j < memberDeckInfo2.DeckCount; j++)
		{
			if (memberDeckInfo2.DefeatDeck[j] == 0)
			{
				num2++;
			}
		}
		if (num > num2)
		{
			return -1 * _sortingFactor;
		}
		if (num2 > num)
		{
			return _sortingFactor;
		}
		if (memberDeckInfo.DeckCount > memberDeckInfo2.DeckCount)
		{
			return -1 * _sortingFactor;
		}
		if (memberDeckInfo2.DeckCount > memberDeckInfo.DeckCount)
		{
			return _sortingFactor;
		}
		if (memberDeckInfo.TeamLevel > memberDeckInfo2.TeamLevel)
		{
			return -1 * _sortingFactor;
		}
		if (memberDeckInfo2.TeamLevel > memberDeckInfo.TeamLevel)
		{
			return _sortingFactor;
		}
		return 0;
	}

	public int CompareLevelForGrid(Transform A, Transform B)
	{
		GuildArenaMemberComponent component = A.GetComponent<GuildArenaMemberComponent>();
		GuildArenaMemberComponent component2 = B.GetComponent<GuildArenaMemberComponent>();
		if (component == null)
		{
			return _sortingFactor;
		}
		if (component2 == null)
		{
			return -1 * _sortingFactor;
		}
		if (component == component2)
		{
			return 0;
		}
		return CompareLevelForList(component, component2);
	}

	public int CompareLevelForList(GuildArenaMemberComponent A, GuildArenaMemberComponent B)
	{
		if (A == null)
		{
			return _sortingFactor;
		}
		if (B == null)
		{
			return -1 * _sortingFactor;
		}
		if (A == B)
		{
			return 0;
		}
		GuildArenaMemberDeckInfo memberDeckInfo = A.MemberDeckInfo;
		GuildArenaMemberDeckInfo memberDeckInfo2 = B.MemberDeckInfo;
		if (memberDeckInfo.DefeatDeck == null)
		{
			return -1 * _sortingFactor;
		}
		if (memberDeckInfo2.DefeatDeck == null)
		{
			return _sortingFactor;
		}
		if (memberDeckInfo.TeamLevel > memberDeckInfo2.TeamLevel)
		{
			return -1 * _sortingFactor;
		}
		if (memberDeckInfo2.TeamLevel > memberDeckInfo.TeamLevel)
		{
			return _sortingFactor;
		}
		int num = 0;
		for (int i = 0; i < memberDeckInfo.DeckCount; i++)
		{
			if (memberDeckInfo.DefeatDeck[i] == 0)
			{
				num++;
			}
		}
		int num2 = 0;
		for (int j = 0; j < memberDeckInfo2.DeckCount; j++)
		{
			if (memberDeckInfo2.DefeatDeck[j] == 0)
			{
				num2++;
			}
		}
		if (num > num2)
		{
			return -1 * _sortingFactor;
		}
		if (num2 > num)
		{
			return _sortingFactor;
		}
		if (memberDeckInfo.DeckCount > memberDeckInfo2.DeckCount)
		{
			return -1 * _sortingFactor;
		}
		if (memberDeckInfo2.DeckCount > memberDeckInfo.DeckCount)
		{
			return _sortingFactor;
		}
		return 0;
	}

	public int CompareMyGuildForGrid(Transform A, Transform B)
	{
		GuildArenaMemberComponent component = A.GetComponent<GuildArenaMemberComponent>();
		GuildArenaMemberComponent component2 = B.GetComponent<GuildArenaMemberComponent>();
		if (component == null)
		{
			return 1;
		}
		if (component2 == null)
		{
			return -1;
		}
		if (component == component2)
		{
			return 0;
		}
		return CompareMyGuildForList(component, component2);
	}

	public int CompareMyGuildForList(GuildArenaMemberComponent A, GuildArenaMemberComponent B)
	{
		if (A == null)
		{
			return 1;
		}
		if (B == null)
		{
			return -1;
		}
		if (A == B)
		{
			return 0;
		}
		long accountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		GuildArenaMemberDeckInfo memberDeckInfo = A.MemberDeckInfo;
		GuildArenaMemberDeckInfo memberDeckInfo2 = B.MemberDeckInfo;
		if (memberDeckInfo.AccountId == accountId)
		{
			return -1;
		}
		if (memberDeckInfo2.AccountId == accountId)
		{
			return 1;
		}
		int num = 0;
		for (int i = 0; i < memberDeckInfo.DeckCount; i++)
		{
			if (memberDeckInfo.DefeatDeck[i] == 0)
			{
				num++;
			}
		}
		int num2 = 0;
		for (int j = 0; j < memberDeckInfo2.DeckCount; j++)
		{
			if (memberDeckInfo2.DefeatDeck[j] == 0)
			{
				num2++;
			}
		}
		if (num > num2)
		{
			return -1 * _sortingFactor;
		}
		if (num2 > num)
		{
			return _sortingFactor;
		}
		if (memberDeckInfo.DeckCount > memberDeckInfo2.DeckCount)
		{
			return -1 * _sortingFactor;
		}
		if (memberDeckInfo2.DeckCount > memberDeckInfo.DeckCount)
		{
			return _sortingFactor;
		}
		if (memberDeckInfo.TeamLevel > memberDeckInfo2.TeamLevel)
		{
			return -1 * _sortingFactor;
		}
		if (memberDeckInfo2.TeamLevel > memberDeckInfo.TeamLevel)
		{
			return _sortingFactor;
		}
		return 0;
	}

	public void InitLabels()
	{
		long currentStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(StaminaType.GuildArenaKey);
		int num = ((Vespa_Generic_IList_Extension_Method.GetLast(_leftMemberComponentList) != null) ? Vespa_Generic_IList_Extension_Method.GetLast(_leftMemberComponentList).MemberDeckInfo.DeckCount : 3);
		UIHelper.SetLabelText(Label_Stamina, $"{currentStamina}/{num}");
		GuildArenaRankerInfo myGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo();
		GuildArenaRankerInfo enemyGuildInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetEnemyGuildInfo();
		if (myGuildInfo != null && enemyGuildInfo != null && _rankers != null && _rankers.Length != 0)
		{
			string text = myGuildInfo.Name;
			int sessionScore = _rankers[0].SessionScore;
			UIHelper.SetLabelText(Label_MyGuildname, text);
			UIHelper.SetLabelText(Label_MyGuildSessionScore, sessionScore);
			string text2 = enemyGuildInfo.Name;
			int sessionScore2 = _rankers[1].SessionScore;
			UIHelper.SetLabelText(Label_EnemyGuildname, text2);
			UIHelper.SetLabelText(Label_EnemyGuildSessionScore, sessionScore2);
			int mySessionScore = _mySessionScore;
			UIHelper.SetLabelText(Label_GuildPoint, mySessionScore);
			InitBattleEndTimeLabel();
		}
	}

	public void InitMemberList(TeamType teamType, GuildArenaMemberDeckInfo[] deckInfos, List<GuildArenaMemberComponent> targetList, UIScrollGrid targetGrid)
	{
		if (deckInfos != null && deckInfos.Length != 0)
		{
			foreach (GuildArenaMemberDeckInfo deckInfo in deckInfos)
			{
				CreateMemberItem(targetGrid, teamType, deckInfo, targetList);
			}
			UIHelper.RepositionGrid(targetGrid);
		}
	}

	private void CreateMemberItem(IGrid grid, TeamType teamType, GuildArenaMemberDeckInfo deckInfo, List<GuildArenaMemberComponent> targetList)
	{
		GuildArenaMemberComponent guildArenaMemberComponent = UIHelper.AddGridItem(grid, GuildArenaMemberComponent.PrefabAuto);
		guildArenaMemberComponent.Open(this, teamType, deckInfo);
		targetList.Add(guildArenaMemberComponent);
	}

	public void OnClickSortOption(int key)
	{
		if (_rightTeamSortingOption != (SORTINGOPTION)key)
		{
			_rightTeamSortingOption = (SORTINGOPTION)key;
			SortMemberList();
		}
	}

	public void OnClickSortOrder(int key)
	{
		if (_rightTeamSortingOrder != (SORTINGORDER)key)
		{
			_rightTeamSortingOrder = (SORTINGORDER)key;
			SortMemberList();
		}
	}

	private void SortMemberList()
	{
		List<GuildArenaMemberComponent> rightMemberComponentList = _rightMemberComponentList;
		UIScrollGrid rightMemberListGrid = RightMemberListGrid;
		_sortingFactor = 1;
		if (_rightTeamSortingOrder == SORTINGORDER.ASCENDING)
		{
			_sortingFactor = -1;
		}
		Comparison<GuildArenaMemberComponent> comparison = CompareLifeForList;
		Comparison<Transform> onCustomSort = CompareLifeForGrid;
		if (_rightTeamSortingOption == SORTINGOPTION.TEAMLEVEL)
		{
			comparison = CompareLevelForList;
			onCustomSort = CompareLevelForGrid;
		}
		rightMemberComponentList.Sort(comparison);
		rightMemberListGrid.Grid.onCustomSort = onCustomSort;
		UIHelper.RepositionGrid(rightMemberListGrid);
	}

	public void OpenLastOpenedEnemyMember()
	{
		if (LastOpenedAccountId == 0L)
		{
			return;
		}
		foreach (GuildArenaMemberComponent rightMemberComponent in _rightMemberComponentList)
		{
			if (rightMemberComponent.MemberDeckInfo.AccountId == LastOpenedAccountId)
			{
				_listener.OnClickDeckAttack(rightMemberComponent.MemberDeckInfo);
				break;
			}
		}
	}

	void GuildArenaMemberComponent.Listener.OnClickDeckInfo(GuildArenaMemberDeckInfo memberInfo)
	{
		if (_listener != null)
		{
			_listener.OnClickDeckInfo(memberInfo);
		}
	}

	void GuildArenaMemberComponent.Listener.OnClickDeckAttack(GuildArenaMemberDeckInfo memberInfo)
	{
		if (_listener != null)
		{
			_listener.OnClickDeckAttack(memberInfo);
			LastOpenedAccountId = memberInfo.AccountId;
		}
	}

	bool GuildArenaDeckListPopup.Listener.HasPrevMember(GuildArenaMemberDeckInfo deckInfo, TeamType _teamType)
	{
		List<GuildArenaMemberComponent> targetList = null;
		if (GetIndexOfTartgetMemberList(deckInfo, _teamType, ref targetList) > 0)
		{
			return true;
		}
		return false;
	}

	bool GuildArenaDeckListPopup.Listener.HasNextMember(GuildArenaMemberDeckInfo deckInfo, TeamType _teamType)
	{
		List<GuildArenaMemberComponent> targetList = null;
		int indexOfTartgetMemberList = GetIndexOfTartgetMemberList(deckInfo, _teamType, ref targetList);
		if (((targetList.Count > 0) ? (targetList.Count - 1) : 0) > indexOfTartgetMemberList)
		{
			return true;
		}
		return false;
	}

	void GuildArenaDeckListPopup.Listener.OnClickPrevDeck(GuildArenaDeckListPopup caller, GuildArenaMemberDeckInfo deckInfo, TeamType _teamType)
	{
		List<GuildArenaMemberComponent> targetList = null;
		int indexOfTartgetMemberList = GetIndexOfTartgetMemberList(deckInfo, _teamType, ref targetList);
		if (targetList.Count <= indexOfTartgetMemberList - 1)
		{
			return;
		}
		GuildArenaMemberDeckInfo memberDeckInfo = targetList[indexOfTartgetMemberList - 1].MemberDeckInfo;
		if (_listener != null)
		{
			if (_teamType == TeamType.Left)
			{
				_listener.OnClickDeckInfo(memberDeckInfo);
			}
			else
			{
				_listener.OnClickDeckAttack(memberDeckInfo);
			}
			LastOpenedAccountId = memberDeckInfo.AccountId;
		}
	}

	void GuildArenaDeckListPopup.Listener.OnClickNextDeck(GuildArenaDeckListPopup caller, GuildArenaMemberDeckInfo deckInfo, TeamType _teamType)
	{
		List<GuildArenaMemberComponent> targetList = null;
		int indexOfTartgetMemberList = GetIndexOfTartgetMemberList(deckInfo, _teamType, ref targetList);
		if (indexOfTartgetMemberList + 1 > targetList.Count - 1)
		{
			return;
		}
		GuildArenaMemberDeckInfo memberDeckInfo = targetList[indexOfTartgetMemberList + 1].MemberDeckInfo;
		if (_listener != null)
		{
			if (_teamType == TeamType.Left)
			{
				_listener.OnClickDeckInfo(memberDeckInfo);
			}
			else
			{
				_listener.OnClickDeckAttack(memberDeckInfo);
			}
			LastOpenedAccountId = memberDeckInfo.AccountId;
		}
	}

	private int GetIndexOfTartgetMemberList(GuildArenaMemberDeckInfo deckInfo, TeamType teamType, ref List<GuildArenaMemberComponent> targetList)
	{
		List<GuildArenaMemberComponent> list = null;
		list = ((teamType != TeamType.Left) ? _rightMemberComponentList : _leftMemberComponentList);
		targetList = list;
		int result = -1;
		for (int i = 0; i < targetList.Count; i++)
		{
			if (targetList[i].MemberDeckInfo != null && targetList[i].MemberDeckInfo == deckInfo)
			{
				result = i;
			}
		}
		return result;
	}

	public void OnClickHistroy()
	{
		if (_listener != null)
		{
			_listener.OnClickHistory();
		}
	}
}
