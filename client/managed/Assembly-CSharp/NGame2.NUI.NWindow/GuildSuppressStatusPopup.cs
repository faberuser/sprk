using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NGuild;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildSuppressStatusPopup : BaseWindow
{
	public enum EShowType
	{
		None,
		Rank,
		History
	}

	[Serializable]
	public class ToggleGroup
	{
		public UIToggle Toggle;

		public EShowType ShowType;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GuildSuppress/GuildSuppressStatusPopup";

	public ToggleGroup[] ToggleGroups;

	public GameObject NothingRoot;

	public GameObject RankDescRoot;

	public GameObject HistoryDescRoot;

	public GameObject RankRoot;

	public UIBaseGrid RankGrid;

	public GameObject HistoryRoot;

	public UIBaseGrid HistoryGrid;

	public UITexture GuildLogoTexture;

	public UITexture GuildLogoBgTexture;

	public UITexture GuildCountyTexture;

	public GameObject GuildMyGo;

	public UILabel GuildNameLabel;

	public UITexture MyAvatarTexture;

	public UILabel MyNameLabel;

	public UILabel MyRankLabel;

	public UILabel MyScoreLabel;

	private List<GuildSuppressStatusPopupRankComponent> _listRankComponent = new List<GuildSuppressStatusPopupRankComponent>();

	private List<GuildSuppressStatusPopupHistoryComponent> _listHistoryComponent = new List<GuildSuppressStatusPopupHistoryComponent>();

	private EShowType _showType;

	private bool _isRankNothing;

	private bool _isHistoryNothing;

	public static void OpenPopup(GuildSuppressGroupRankerInfo rankerInfo, GuildInfo guildInfo, GuildSuppressMemberPlayHistory[] historyInfos, GuildSuppressMemberScoreInfo[] scoreInfos, GuildSuppressMemberScoreInfo myScoreInfo, int myRank, bool isMyGuild)
	{
		WindowPrefab<GuildSuppressStatusPopup> windowPrefab = new WindowPrefab<GuildSuppressStatusPopup>("Resources/Prefab2/Window/GuildSuppress/GuildSuppressStatusPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(rankerInfo, guildInfo, historyInfos, scoreInfos, myScoreInfo, myRank, isMyGuild);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal(GuildSuppressGroupRankerInfo rankerInfo, GuildInfo guildInfo, GuildSuppressMemberPlayHistory[] historyInfos, GuildSuppressMemberScoreInfo[] scoreInfos, GuildSuppressMemberScoreInfo myScoreInfo, int myRank, bool isMyGuild)
	{
		Open(delegate
		{
			SetRankComponent(scoreInfos);
			SetHistoryComponent(historyInfos);
			SetSelectGuildInfo(rankerInfo, guildInfo, isMyGuild);
			SetMyInfo(myScoreInfo, myRank);
			_showType = EShowType.Rank;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ToggleGroups); i++)
			{
				ToggleGroup toggleGroup = ToggleGroups[i];
				if (toggleGroup != null && !(toggleGroup.Toggle == null) && toggleGroup.ShowType == EShowType.Rank)
				{
					if (toggleGroup.Toggle.value)
					{
						OnChangeValue(toggleGroup.Toggle);
					}
					else
					{
						toggleGroup.Toggle.value = true;
					}
				}
			}
		}, null);
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ReleaseHistoryComponent();
		ReleaseRankComponent();
		ShowType(EShowType.None);
		UIHelper.ClearPlaneAssetTexture(MyAvatarTexture);
		UIHelper.ClearPlaneAssetTexture(GuildLogoTexture);
		UIHelper.ClearPlaneAssetTexture(GuildCountyTexture);
		UIHelper.ClearPlaneAssetTexture(GuildLogoBgTexture);
	}

	private void ShowType(EShowType showType)
	{
		UIHelper.EnableGameObject(RankRoot, showType == EShowType.Rank);
		UIHelper.EnableGameObject(RankDescRoot, showType == EShowType.Rank);
		UIHelper.EnableGameObject(HistoryRoot, showType == EShowType.History);
		UIHelper.EnableGameObject(HistoryDescRoot, showType == EShowType.History);
		bool enable = (showType == EShowType.Rank && _isRankNothing) || (showType == EShowType.History && _isHistoryNothing);
		UIHelper.EnableGameObject(NothingRoot, enable);
		_showType = showType;
	}

	private void SetSelectGuildInfo(GuildSuppressGroupRankerInfo rankerInfo, GuildInfo guildInfo, bool isMyGuild)
	{
		int guildLogo = rankerInfo.GuildLogo;
		byte back = guildInfo.Back;
		string countryCode = rankerInfo.CountryCode;
		string guildName = rankerInfo.GuildName;
		GuildSuppressHelper.SetGuildLogo(guildLogo, back, GuildLogoTexture, GuildLogoBgTexture, this);
		GuildSuppressHelper.SetGuildCountyTexture(countryCode, GuildCountyTexture, this);
		UIHelper.SetLabelText(GuildNameLabel, guildName);
		UIHelper.EnableGameObject(GuildMyGo, isMyGuild);
	}

	private void SetMyInfo(GuildSuppressMemberScoreInfo myScoreInfo, int rank)
	{
		int avatarHeroIndex = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AvatarHeroIndex;
		string nick = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick;
		string text = "";
		long num = myScoreInfo?.Score ?? 0;
		GuildSuppressHelper.SetHeroPortrait(avatarHeroIndex, isBattlePortrait: true, MyAvatarTexture, this);
		UIHelper.SetLabelText(MyNameLabel, nick);
		UIHelper.SetLabelText(MyRankLabel, text);
		UIHelper.SetLabelText(MyScoreLabel, Vespa_Long_Extension_Method.ToCommaString(num));
	}

	private void SetRankComponent(GuildSuppressMemberScoreInfo[] infos)
	{
		ReleaseRankComponent();
		_isRankNothing = __Extension_Method_ICollection__.IsNullOrEmpty(infos);
		if (_isRankNothing)
		{
			return;
		}
		long accountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			GuildSuppressMemberScoreInfo guildSuppressMemberScoreInfo = infos[i];
			GuildSuppressStatusPopupRankComponent guildSuppressStatusPopupRankComponent = UIHelper.AddGridItem(RankGrid, GuildSuppressStatusPopupRankComponent.PrefabAuto);
			if (!(guildSuppressStatusPopupRankComponent == null))
			{
				int rank = i;
				int avatarHeroIndex = guildSuppressMemberScoreInfo.AvatarHeroIndex;
				bool isMe = guildSuppressMemberScoreInfo.AccountId == accountId;
				string nickName = guildSuppressMemberScoreInfo.NickName;
				long score = guildSuppressMemberScoreInfo.Score;
				guildSuppressStatusPopupRankComponent.Open(rank, avatarHeroIndex, isMe, nickName, score);
				_listRankComponent.Add(guildSuppressStatusPopupRankComponent);
			}
		}
		UIHelper.RepositionGrid(RankGrid);
	}

	private void ReleaseRankComponent()
	{
		FactoryBase.ReleaseAndClearList(_listRankComponent);
	}

	private void SetHistoryComponent(GuildSuppressMemberPlayHistory[] infos)
	{
		ReleaseHistoryComponent();
		_isHistoryNothing = __Extension_Method_ICollection__.IsNullOrEmpty(infos);
		if (_isHistoryNothing)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			GuildSuppressMemberPlayHistory guildSuppressMemberPlayHistory = infos[i];
			if (guildSuppressMemberPlayHistory != null)
			{
				GuildSuppressStatusPopupHistoryComponent guildSuppressStatusPopupHistoryComponent = UIHelper.AddGridItem(HistoryGrid, GuildSuppressStatusPopupHistoryComponent.PrefabAuto);
				if (!(guildSuppressStatusPopupHistoryComponent == null))
				{
					Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressMemberPlayHistory.BattleTime, out var o_dateTime);
					string nickName = guildSuppressMemberPlayHistory.NickName;
					int raidIndex = guildSuppressMemberPlayHistory.RaidIndex;
					int level = guildSuppressMemberPlayHistory.Level;
					long score = guildSuppressMemberPlayHistory.Score;
					guildSuppressStatusPopupHistoryComponent.Open(o_dateTime, nickName, raidIndex, level, score);
					_listHistoryComponent.Add(guildSuppressStatusPopupHistoryComponent);
				}
			}
		}
		UIHelper.RepositionGrid(HistoryGrid);
	}

	private void ReleaseHistoryComponent()
	{
		FactoryBase.ReleaseAndClearList(_listHistoryComponent);
	}

	public void OnChangeValue(UIToggle toggle)
	{
		if (toggle == null || !toggle.value || _showType == EShowType.None)
		{
			return;
		}
		EShowType showType = EShowType.None;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ToggleGroups); i++)
		{
			ToggleGroup toggleGroup = ToggleGroups[i];
			if (toggle == toggleGroup.Toggle)
			{
				showType = toggleGroup.ShowType;
			}
		}
		ShowType(showType);
	}
}
