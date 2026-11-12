using System;
using System.Collections.Generic;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildRankingBoardSuppress : BaseWindow
{
	public interface Listener
	{
		void GetServerTotalRankingList(int page, int viewSeasonIndex, Action<GuildRankingBoardServerTotalRankingInfo[]> onList);

		void GetRankerList(int page, GuildRankingType rankingType, Action<GuildRankingBoardRankerInfo[]> onList);

		void GetMyRankingInfo(GuildRankingType rankingType, Action<GuildRankingBoardRankerInfo> onComplete, Action<int> onNext);

		void GetMyServerTotalRankingInfo(int viewSeasonIndex, long guildId, Action<GuildRankingBoardServerTotalRankingInfo> onComplete, Action<int> onNext);
	}

	public enum EToggle
	{
		None,
		Server,
		World,
		TotalRank
	}

	private Listener _listener;

	public const string PrefabPath = "Resources/Prefab2/Window/GuildRankingBoard/GuildRankingBoardSuppress";

	public const int MaxPageCount = 5;

	public UIBaseGrid RankerGrid;

	public Transform Transform_GlobalRoot;

	public Transform Transform_GuildLogoRoot;

	public UILabel Label_EmptyRank;

	public UILabel Label_NoneRankingInfo;

	public UILabel Label_SeasonNo;

	public UILabel Label_PageNum;

	public UILabel Label_RankingTitle;

	public UILabel Label_MyGuildName;

	public UILabel Label_MyGuildRank;

	public UILabel Label_MyGuildSuppressScore;

	public UITexture MyGuildLogoTexture;

	public UITexture MyGuildBackTexture;

	public UIToggle Toggle_WorldRanking;

	public UIToggle Toggle_ServerRanking;

	public UIToggle Toggle_TotalRanking;

	private int _page;

	private int _viewSeasonIndex;

	private EToggle _toggle;

	private GuildRankingType _curRankingType = GuildRankingType.World;

	private string _myServerGroup;

	private List<GuildRankingBoardComponent> _rankerComponentList = new List<GuildRankingBoardComponent>();

	public override void UpdateDerived()
	{
		base.UpdateDerived();
	}

	public override void Reset()
	{
		base.Reset();
		InitToggle(EToggle.None);
		_toggle = EToggle.None;
		_curRankingType = GuildRankingType.None;
		ClearRanker();
	}

	public void Open(Listener listener, int viewSeasonIndex, Action onComplete = null)
	{
		_listener = listener;
		_viewSeasonIndex = viewSeasonIndex;
		Open(Init, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		});
	}

	private void Init()
	{
		InitToggle(EToggle.World);
	}

	private void SetMyGuildInfo(GuildRankingBoardRankerInfo myInfo)
	{
		if (myInfo == null || myInfo.Rank <= 0)
		{
			UIHelper.EnableGameObject(Label_NoneRankingInfo, enable: true);
			UIHelper.EnableGameObject(Label_MyGuildRank, enable: false);
			if (GlobalAutoBehaviour<GuildManager>.isValidInstance && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info != null)
			{
				byte logo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Logo;
				byte b = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Back;
				if (b <= 0 || b > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
				{
					b = 1;
				}
				InitMyGuildLogo(logo, b);
				UIHelper.SetLabelText(Label_MyGuildName, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Name);
			}
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("InitialSuppressScore");
			UIHelper.SetLabelText(Label_MyGuildSuppressScore, valueInt.ToString("#,0"));
			return;
		}
		_myServerGroup = myInfo.ServerGroup;
		UIHelper.EnableGameObject(Label_NoneRankingInfo, enable: false);
		UIHelper.EnableGameObject(Label_MyGuildRank, enable: true);
		if (_curRankingType != GuildRankingType.TotalRank)
		{
			byte b2 = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Back;
			if (b2 <= 0 || b2 > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
			{
				b2 = 1;
			}
			InitMyGuildLogo(myInfo.Logo, b2);
		}
		UIHelper.SetLabelText(Label_MyGuildName, myInfo.Name);
		UIHelper.SetLabelText(Label_MyGuildRank, myInfo.Rank);
		UIHelper.SetLabelText(Label_MyGuildSuppressScore, myInfo.Score.ToString("#,0"));
	}

	private void SetMyServerInfo(GuildRankingBoardServerTotalRankingInfo myServerInfo)
	{
		if (myServerInfo == null || myServerInfo.Rank <= 0)
		{
			UIHelper.EnableGameObject(Label_NoneRankingInfo, enable: true);
			UIHelper.EnableGameObject(Label_MyGuildRank, enable: false);
			UIHelper.SetLabelText(Label_MyGuildName, string.Empty);
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("InitialSuppressScore");
			UIHelper.SetLabelText(Label_MyGuildSuppressScore, valueInt.ToString("#,0"));
		}
		else
		{
			UIHelper.EnableGameObject(Label_NoneRankingInfo, enable: false);
			UIHelper.EnableGameObject(Label_MyGuildRank, enable: true);
			UIHelper.SetLabelText(Label_MyGuildRank, myServerInfo.Rank);
			UIHelper.SetLabelText(Label_MyGuildSuppressScore, myServerInfo.Score.ToString("#,0"));
		}
		if (myServerInfo != null && !String_Extension_Method.IsNullOrEmpty(myServerInfo.ServerGroup))
		{
			string serverName = GameRoot.Instance.ServerGroupNameDataContainer.GetServerName(myServerInfo.ServerGroup);
			UIHelper.SetLabelText(Label_MyGuildName, serverName);
		}
	}

	private void InitMyGuildLogo(byte guildLogo, byte guildBack)
	{
		AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(guildLogo);
		UIHelper.ResetPlaneAssetTextureAsync(this, MyGuildLogoTexture, guildLogoIcon);
		AssetTexture guildBackGround2D = UIHelper.GetGuildBackGround2D(guildBack);
		UIHelper.ResetPlaneAssetTextureAsync(this, MyGuildBackTexture, guildBackGround2D);
	}

	private void SetViewSeason()
	{
		string stringDirect = LocalizationManager.GetStringDirect("RANK_BOARD_SEASON_INFO", _viewSeasonIndex);
		UIHelper.SetLabelText(Label_SeasonNo, stringDirect);
	}

	private void RefreshRanker(GuildRankingBoardRankerInfo[] rankerInfos)
	{
		ClearRanker();
		if (rankerInfos == null)
		{
			return;
		}
		foreach (GuildRankingBoardRankerInfo rankerInfo in rankerInfos)
		{
			GuildRankingBoardComponent guildRankingBoardComponent = UIHelper.AddGridItem(RankerGrid, GuildRankingBoardComponent.PrefabAuto);
			if (guildRankingBoardComponent == null || !guildRankingBoardComponent.SetData(rankerInfo, _myServerGroup, _curRankingType))
			{
				FactoryBase.ReleaseComponent(guildRankingBoardComponent);
			}
			else
			{
				_rankerComponentList.Add(guildRankingBoardComponent);
			}
		}
		UIHelper.RepositionGrid(RankerGrid);
		UIHelper.EnableGameObject(Label_EmptyRank, enable: false);
	}

	private void RefreshServerTotalRanking(GuildRankingBoardServerTotalRankingInfo[] totalRankingInfos)
	{
		ClearRanker();
		if (totalRankingInfos == null)
		{
			return;
		}
		foreach (GuildRankingBoardServerTotalRankingInfo data in totalRankingInfos)
		{
			GuildRankingBoardComponent guildRankingBoardComponent = UIHelper.AddGridItem(RankerGrid, GuildRankingBoardComponent.TotalRankPrefabAuto);
			if (guildRankingBoardComponent == null || !guildRankingBoardComponent.SetData(data))
			{
				FactoryBase.ReleaseComponent(guildRankingBoardComponent);
			}
			else
			{
				_rankerComponentList.Add(guildRankingBoardComponent);
			}
		}
		UIHelper.RepositionGrid(RankerGrid);
		UIHelper.EnableGameObject(Label_EmptyRank, enable: false);
	}

	private void RefreshRanker(int page)
	{
		_page = page;
		UIHelper.SetLabelText(Label_PageNum, page + 1 + "/" + 5);
		if (_listener != null)
		{
			if (_curRankingType == GuildRankingType.TotalRank)
			{
				_listener.GetServerTotalRankingList(_page, _viewSeasonIndex, RefreshServerTotalRanking);
			}
			else
			{
				_listener.GetRankerList(_page, _curRankingType, RefreshRanker);
			}
		}
	}

	private void RefreshMyInfo(Action<int> onNext)
	{
		UIHelper.EnableGameObject(Transform_GuildLogoRoot, _curRankingType != GuildRankingType.TotalRank);
		if (_listener == null)
		{
			return;
		}
		if (_curRankingType == GuildRankingType.TotalRank)
		{
			if (GlobalAutoBehaviour<GuildManager>.isValidInstance && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info != null)
			{
				_listener.GetMyServerTotalRankingInfo(_viewSeasonIndex, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId(), SetMyServerInfo, onNext);
			}
		}
		else
		{
			_listener.GetMyRankingInfo(_curRankingType, SetMyGuildInfo, onNext);
		}
	}

	private void RefreshInfoTitle()
	{
		string stringDirect = LocalizationManager.GetStringDirect((_curRankingType != GuildRankingType.TotalRank) ? "RANK_BOARD_MY_GUILD_RANKING" : "RANK_BOARD_SERVER_TOTAL_RANK_TITLE");
		UIHelper.SetLabelText(Label_RankingTitle, stringDirect);
	}

	private void ClearRanker()
	{
		FactoryBase.ReleaseAndClearList(_rankerComponentList);
	}

	private void InitToggle(EToggle startToggle)
	{
		Toggle_WorldRanking.Set(startToggle == EToggle.World);
		Toggle_ServerRanking.Set(startToggle == EToggle.Server);
		Toggle_TotalRanking.Set(startToggle == EToggle.TotalRank);
	}

	public void OnToggleChanged()
	{
		EToggle eToggle = EToggle.None;
		if (Toggle_WorldRanking.value)
		{
			eToggle = EToggle.World;
		}
		else if (Toggle_ServerRanking.value)
		{
			eToggle = EToggle.Server;
		}
		else if (Toggle_TotalRanking.value)
		{
			eToggle = EToggle.TotalRank;
		}
		if (eToggle != EToggle.None && _toggle != eToggle)
		{
			_toggle = eToggle;
			if (_toggle == EToggle.World)
			{
				_curRankingType = GuildRankingType.World;
			}
			else if (_toggle == EToggle.Server)
			{
				_curRankingType = GuildRankingType.Server;
			}
			else if (_toggle == EToggle.TotalRank)
			{
				_curRankingType = GuildRankingType.TotalRank;
			}
			RefreshInfoTitle();
			SetViewSeason();
			RefreshMyInfo(RefreshRanker);
		}
	}

	public void OnClickNextPage()
	{
		if (_page < 4)
		{
			int page = _page + 1;
			RefreshRanker(page);
		}
	}

	public void OnClickPrevPage()
	{
		if (_page > 0)
		{
			int page = _page - 1;
			RefreshRanker(page);
		}
	}
}
