using System;
using System.Collections.Generic;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NShared.GetGuildArenaMemberRecord;
using NShared.GetGuildArenaMemberScore;
using NShared.GetGuildArenaRanker;
using NShared.GetGuildArenaRankerRecord;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildArenaRankingInfo : BaseWindow
{
	public interface Listener
	{
	}

	public enum EToggle
	{
		NONE,
		RECORD,
		RANK,
		CONTRIBUTION,
		MyRecord
	}

	private Listener _listener;

	public const string PrefabPath = "Resources/Prefab2/Window/GuildArena/GuildArenaRankingInfo";

	public UIBaseGrid RecordGrid;

	public UIBaseGrid RankGrid;

	public UIBaseGrid ContributionGrid;

	public UIBaseGrid MyRecordGrid;

	public EToggle _currentTab = EToggle.RECORD;

	public UIToggle Toggle_ArenaRecord;

	public UIToggle Toggle_ArenaRank;

	public UIToggle Toggle_ContributionRank;

	public UIToggle Toggle_MyRecord;

	public GameObject ArenaRecordRoot;

	public GameObject ArenaRankRoot;

	public GameObject ContributionRankRoot;

	public GameObject MyRecordRoot;

	public GameObject NotExistRoot;

	public UILabel Label_Page_Rank;

	public UILabel Label_Page_Record;

	public UILabel Label_Page_Contribution;

	private int Index_Rank;

	private int Index_Record;

	private int Index_Contribution;

	private int maxPageIndex = 5;

	public UILabel Label_Rank_GuildName;

	public UILabel Label_Rank_Point;

	public UILabel Label_Rank_Season;

	public UILabel Label_Rank_Rank;

	public UITexture Texture_Rank_MyGuildLogo;

	public UITexture Texture_Rank_MyGuildBack;

	public UITexture Texture_Rank_CountryFlag;

	public UILabel Label_Cont_Nick;

	public UILabel Label_Cont_Point;

	public UILabel Label_Cont_Session;

	public UILabel Label_Cont_Rank;

	public UITexture Texture_Cont_MyGuildLogo;

	public UITexture Texture_Cont_MyGuildBack;

	public UITexture Texture_Cont_CountryFlag;

	private List<GuildArenaRecordRankingComponent> GuildArenaRecordRankingComponents = new List<GuildArenaRecordRankingComponent>();

	private List<GuildArenaGuildRankingComponent> GuildArenaGuildRankingComponents = new List<GuildArenaGuildRankingComponent>();

	private List<GuildArenaContributionRankComponent> GuildArenaContributionRankComponents = new List<GuildArenaContributionRankComponent>();

	private List<GuildArenaHistoryComponent> GuildArenaHistoryComponents = new List<GuildArenaHistoryComponent>();

	public override void Reset()
	{
		base.Reset();
	}

	public override void Close()
	{
		base.Close();
	}

	public void Open(Listener listener, int viewSeasonIndex, Action onComplete = null)
	{
		_listener = listener;
		Open(Init, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		});
	}

	private void Init()
	{
		Clear();
		InitToggle();
		RequestCurrentTabInfo();
	}

	private void InitToggle()
	{
		Toggle_ArenaRecord.value = true;
		Toggle_ArenaRecord.Set(state: true);
	}

	private void Clear()
	{
		Index_Rank = 0;
		Index_Record = 0;
		Index_Contribution = 0;
	}

	public void OnToggleChanged()
	{
		SetCurrentTab();
	}

	private void RefreshPages()
	{
		UIHelper.EnableGameObject(ArenaRankRoot, Toggle_ArenaRank.value);
		UIHelper.EnableGameObject(ArenaRecordRoot, Toggle_ArenaRecord.value);
		UIHelper.EnableGameObject(ContributionRankRoot, Toggle_ContributionRank.value);
		UIHelper.EnableGameObject(MyRecordRoot, Toggle_MyRecord.value);
		UIHelper.EnableGameObject(RankGrid.gameObject, enable: false);
		UIHelper.EnableGameObject(RecordGrid.gameObject, enable: false);
		UIHelper.EnableGameObject(ContributionGrid.gameObject, enable: false);
		UIHelper.EnableGameObject(MyRecordGrid.gameObject, enable: false);
	}

	private void SetCurrentTab()
	{
		InitPages(notExist: false, EToggle.NONE);
		if (Toggle_ArenaRank.value && _currentTab != EToggle.RANK)
		{
			_currentTab = EToggle.RANK;
			RequestCurrentTabInfo();
		}
		else if (Toggle_ArenaRecord.value && _currentTab != EToggle.RECORD)
		{
			_currentTab = EToggle.RECORD;
			RequestCurrentTabInfo();
		}
		else if (Toggle_ContributionRank.value && _currentTab != EToggle.CONTRIBUTION)
		{
			_currentTab = EToggle.CONTRIBUTION;
			RequestCurrentTabInfo();
		}
		else if (Toggle_MyRecord.value && _currentTab != EToggle.MyRecord)
		{
			_currentTab = EToggle.MyRecord;
			RequestCurrentTabInfo();
		}
	}

	public void InitPages(bool notExist, EToggle tab)
	{
		UIHelper.EnableGameObject(NotExistRoot, notExist);
		GameObject gameObject = null;
		switch (tab)
		{
		case EToggle.NONE:
			return;
		case EToggle.RANK:
			gameObject = RankGrid.gameObject;
			break;
		}
		if (tab == EToggle.RECORD)
		{
			gameObject = RecordGrid.gameObject;
		}
		if (tab == EToggle.CONTRIBUTION)
		{
			gameObject = ContributionGrid.gameObject;
		}
		if (tab == EToggle.MyRecord)
		{
			gameObject = MyRecordGrid.gameObject;
		}
		RefreshPages();
		UIHelper.EnableGameObject(gameObject, !notExist);
	}

	private void InitRecordPage(NShared.GetGuildArenaRankerRecord.Response response)
	{
		FactoryBase.ReleaseAndClearList(GuildArenaRecordRankingComponents);
		if (response == null || response.Result != NShared.GetGuildArenaRankerRecord.ResultType.Success || response.BaseResult != BaseResultType.Success || response.RankerRecords == null || response.RankerRecords.Length == 0)
		{
			InitPages(notExist: true, EToggle.RECORD);
			return;
		}
		InitPages(notExist: false, EToggle.RECORD);
		GuildArenaRankerRecordInfo[] rankerRecords = response.RankerRecords;
		foreach (GuildArenaRankerRecordInfo recordInfo in rankerRecords)
		{
			GuildArenaRecordRankingComponent guildArenaRecordRankingComponent = UIHelper.AddGridItem(RecordGrid, GuildArenaRecordRankingComponent.PrefabAuto);
			if (!(guildArenaRecordRankingComponent == null))
			{
				guildArenaRecordRankingComponent.Open(recordInfo);
				GuildArenaRecordRankingComponents.Add(guildArenaRecordRankingComponent);
			}
		}
		RecordGrid.RepositionGrid();
	}

	private void InitRankPage(NShared.GetGuildArenaRanker.Response response)
	{
		FactoryBase.ReleaseAndClearList(GuildArenaGuildRankingComponents);
		if (response == null || response.Result != NShared.GetGuildArenaRanker.ResultType.Success || response.BaseResult != BaseResultType.Success || response.Rankers == null || response.Rankers.Length == 0)
		{
			InitPages(notExist: true, EToggle.RANK);
			return;
		}
		InitPages(notExist: false, EToggle.RANK);
		GuildArenaRankerInfo[] rankers = response.Rankers;
		foreach (GuildArenaRankerInfo rankerInfo in rankers)
		{
			GuildArenaGuildRankingComponent guildArenaGuildRankingComponent = UIHelper.AddGridItem(RankGrid, GuildArenaGuildRankingComponent.PrefabAuto);
			if (!(guildArenaGuildRankingComponent == null))
			{
				guildArenaGuildRankingComponent.Open(rankerInfo);
				GuildArenaGuildRankingComponents.Add(guildArenaGuildRankingComponent);
			}
		}
		GuildArenaRankerInfo myRankerInfo = response.MyRankerInfo;
		if (myRankerInfo != null)
		{
			UIHelper.SetLabelText(Label_Rank_GuildName, myRankerInfo.Name);
			UIHelper.SetLabelText(Label_Rank_Point, myRankerInfo.SeasonScore);
			string guildArenaSeasonDisplayName = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetGuildArenaSeasonDisplayName();
			UIHelper.SetLabelText(Label_Rank_Season, LocalizationManager.GetStringDirect("GUILD_ARENA_SEASON_INFO", guildArenaSeasonDisplayName));
			UIHelper.SetLabelText(Label_Rank_Rank, LocalizationManager.GetStringDirect("GUILD_MY_RANK", (myRankerInfo.Rank >= 0) ? myRankerInfo.Rank.ToString() : "-"));
			AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2((myRankerInfo.Logo <= 0) ? 1 : myRankerInfo.Logo);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Rank_MyGuildLogo, guildLogoIcon);
			byte b = myRankerInfo.Back;
			if (b <= 0 || b > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
			{
				b = 1;
			}
			AssetTexture guildBackGround2D = UIHelper.GetGuildBackGround2D(b);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Rank_MyGuildBack, guildBackGround2D);
			string countryCode = myRankerInfo.CountryCode;
			CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
			if (dataByCode != null)
			{
				AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Rank_CountryFlag, plainTexture);
			}
		}
		RankGrid.RepositionGrid();
	}

	private void InitContributionPage(NShared.GetGuildArenaMemberScore.Response response)
	{
		FactoryBase.ReleaseAndClearList(GuildArenaContributionRankComponents);
		if (response == null || response.Result != NShared.GetGuildArenaMemberScore.ResultType.Success || response.BaseResult != BaseResultType.Success || response.MemberScores == null || response.MemberScores.Length == 0)
		{
			InitPages(notExist: true, EToggle.CONTRIBUTION);
			return;
		}
		InitPages(notExist: false, EToggle.CONTRIBUTION);
		GuildArenaMemberScoreInfo[] memberScores = response.MemberScores;
		foreach (GuildArenaMemberScoreInfo memberScoreInfo in memberScores)
		{
			GuildArenaContributionRankComponent guildArenaContributionRankComponent = UIHelper.AddGridItem(ContributionGrid, GuildArenaContributionRankComponent.PrefabAuto);
			if (!(guildArenaContributionRankComponent == null))
			{
				guildArenaContributionRankComponent.Open(memberScoreInfo);
				GuildArenaContributionRankComponents.Add(guildArenaContributionRankComponent);
			}
		}
		GuildArenaMemberScoreInfo myMemberScore = response.MyMemberScore;
		if (myMemberScore != null)
		{
			UIHelper.SetLabelText(Label_Cont_Nick, myMemberScore.Nick);
			UIHelper.SetLabelText(Label_Cont_Point, myMemberScore.SessionScore);
			UIHelper.SetLabelText(Label_Cont_Session, LocalizationManager.GetStringDirect("GUILD_ARENA_SESSION_INFO", myMemberScore.SessionNumber));
			UIHelper.SetLabelText(Label_Cont_Rank, LocalizationManager.GetStringDirect("GUILD_MY_CONTRIBUTION_RANK", (myMemberScore.ScoreRank >= 0) ? myMemberScore.ScoreRank.ToString() : "-"));
			AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo().Logo);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Cont_MyGuildLogo, guildLogoIcon);
			byte b = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo().Back;
			if (b <= 0 || b > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
			{
				b = 1;
			}
			AssetTexture guildBackGround2D = UIHelper.GetGuildBackGround2D(b);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Cont_MyGuildBack, guildBackGround2D);
			string countryCode = myMemberScore.CountryCode;
			CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
			if (dataByCode != null)
			{
				AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Cont_CountryFlag, plainTexture);
			}
		}
		ContributionGrid.RepositionGrid();
	}

	private void InitMyRecordPage(NShared.GetGuildArenaMemberRecord.Response response)
	{
		FactoryBase.ReleaseAndClearList(GuildArenaHistoryComponents);
		if (response.MemberRecords == null || __Extension_Method_ICollection__.COUNT(response.MemberRecords) <= 0)
		{
			InitPages(notExist: true, EToggle.MyRecord);
			return;
		}
		InitPages(notExist: false, EToggle.MyRecord);
		GuildArenaMemberRecordInfo[] memberRecords = response.MemberRecords;
		foreach (GuildArenaMemberRecordInfo guildArenaMemberRecordInfo in memberRecords)
		{
			if (guildArenaMemberRecordInfo != null)
			{
				GuildArenaHistoryComponent guildArenaHistoryComponent = UIHelper.AddGridItem(MyRecordGrid, GuildArenaHistoryComponent.PrefabAuto);
				GuildArenaHistoryComponents.Add(guildArenaHistoryComponent);
				guildArenaHistoryComponent.Init(guildArenaMemberRecordInfo, GuildArenaRecordType.MyRecord);
			}
		}
		UIHelper.RepositionGrid(MyRecordGrid);
	}

	private void RequestCurrentTabInfo()
	{
		switch (_currentTab)
		{
		case EToggle.RECORD:
			Index_Record = 0;
			UIHelper.SetLabelText(Label_Page_Record, $"{Index_Record + 1}/{maxPageIndex}");
			SceneAutoBehaviour<GuildArenaManagement>.instance.RequestGetGuildArenaRankerRecord(Index_Record, delegate(NShared.GetGuildArenaRankerRecord.Response response)
			{
				InitRecordPage(response);
			});
			break;
		case EToggle.RANK:
			Index_Rank = 0;
			UIHelper.SetLabelText(Label_Page_Rank, $"{Index_Rank + 1}/{maxPageIndex}");
			SceneAutoBehaviour<GuildArenaManagement>.instance.RequestGetGuildArenaRanker(Index_Rank, delegate(NShared.GetGuildArenaRanker.Response response)
			{
				InitRankPage(response);
			});
			break;
		case EToggle.CONTRIBUTION:
			Index_Contribution = 0;
			UIHelper.SetLabelText(Label_Page_Contribution, $"{Index_Contribution + 1}/{maxPageIndex}");
			SceneAutoBehaviour<GuildArenaManagement>.instance.RequestGetGuildArenaMemberScore(Index_Contribution, delegate(NShared.GetGuildArenaMemberScore.Response response)
			{
				InitContributionPage(response);
			});
			break;
		case EToggle.MyRecord:
			Index_Contribution = 0;
			SceneAutoBehaviour<GuildArenaManagement>.instance.RequestGetGuildArenaMyRecord(GuildArenaRecordType.MyRecord, delegate(NShared.GetGuildArenaMemberRecord.Response response)
			{
				InitMyRecordPage(response);
			});
			break;
		}
	}

	public void OnClickPrevRecord()
	{
		if (Index_Record > 0)
		{
			Index_Record--;
			UIHelper.SetLabelText(Label_Page_Record, $"{Index_Record + 1}/{maxPageIndex}");
			SceneAutoBehaviour<GuildArenaManagement>.instance.RequestGetGuildArenaRankerRecord(Index_Record, delegate(NShared.GetGuildArenaRankerRecord.Response response)
			{
				InitRecordPage(response);
			});
		}
	}

	public void OnClickNextRecord()
	{
		if (Index_Record < 4)
		{
			Index_Record++;
			UIHelper.SetLabelText(Label_Page_Record, $"{Index_Record + 1}/{maxPageIndex}");
			SceneAutoBehaviour<GuildArenaManagement>.instance.RequestGetGuildArenaRankerRecord(Index_Record, delegate(NShared.GetGuildArenaRankerRecord.Response response)
			{
				InitRecordPage(response);
			});
		}
	}

	public void OnClickPrevRank()
	{
		if (Index_Rank > 0)
		{
			Index_Rank--;
			UIHelper.SetLabelText(Label_Page_Rank, $"{Index_Rank + 1}/{maxPageIndex}");
			SceneAutoBehaviour<GuildArenaManagement>.instance.RequestGetGuildArenaRanker(Index_Rank, delegate(NShared.GetGuildArenaRanker.Response response)
			{
				InitRankPage(response);
			});
		}
	}

	public void OnClickNextRank()
	{
		if (Index_Rank < 4)
		{
			Index_Rank++;
			UIHelper.SetLabelText(Label_Page_Rank, $"{Index_Rank + 1}/{maxPageIndex}");
			SceneAutoBehaviour<GuildArenaManagement>.instance.RequestGetGuildArenaRanker(Index_Rank, delegate(NShared.GetGuildArenaRanker.Response response)
			{
				InitRankPage(response);
			});
		}
	}

	public void OnClickPrevContribution()
	{
		if (Index_Contribution > 0)
		{
			Index_Contribution--;
			UIHelper.SetLabelText(Label_Page_Contribution, $"{Index_Contribution + 1}/{maxPageIndex}");
			SceneAutoBehaviour<GuildArenaManagement>.instance.RequestGetGuildArenaMemberScore(Index_Contribution, delegate(NShared.GetGuildArenaMemberScore.Response response)
			{
				InitContributionPage(response);
			});
		}
	}

	public void OnClickNextContribution()
	{
		if (Index_Contribution < 4)
		{
			Index_Contribution++;
			UIHelper.SetLabelText(Label_Page_Contribution, $"{Index_Contribution + 1}/{maxPageIndex}");
			SceneAutoBehaviour<GuildArenaManagement>.instance.RequestGetGuildArenaMemberScore(Index_Contribution, delegate(NShared.GetGuildArenaMemberScore.Response response)
			{
				InitContributionPage(response);
			});
		}
	}
}
