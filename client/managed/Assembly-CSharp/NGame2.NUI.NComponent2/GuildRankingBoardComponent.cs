using NGame2.NGuild;
using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildRankingBoardComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/GuildRankingBoard/GuildRankingBoardComponent";

	public const string TotalRankPrefabPath = "Prefab2/Component/GuildRankingBoard/GuildRankingBoardTotalRankComponent";

	public static PrefabAuto<GuildRankingBoardComponent> PrefabAuto = new PrefabAuto<GuildRankingBoardComponent>("Prefab2/Component/GuildRankingBoard/GuildRankingBoardComponent");

	public static PrefabAuto<GuildRankingBoardComponent> TotalRankPrefabAuto = new PrefabAuto<GuildRankingBoardComponent>("Prefab2/Component/GuildRankingBoard/GuildRankingBoardTotalRankComponent");

	public UITexture Texture_GuildLogo;

	public UITexture Texture_CountryFlag;

	public UILabel Label_RankOtherGuild;

	public UILabel Label_RankeMyGuild;

	public UISpriteSelector RankSpriteSelector;

	public UILabel Label_Name;

	public UILabel Label_Score;

	public UILabel Label_MemberCount;

	public UILabel Label_Level;

	public UILabel Label_Server;

	public UILabel Label_GlobalName;

	public UILabel Label_GlobalMemberCount;

	public UILabel Label_GlobalLevel;

	public UILabel LabelDisapperGuild;

	public Transform GlobalNameRoot;

	public Transform LocalNameRoot;

	private GuildRankingBoardRankerInfo rankerInfo;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		ClearRankerInfo();
	}

	public bool SetData(GuildRankingBoardRankerInfo rankerInfo, string myServerGroup, GuildRankingType rankingType)
	{
		if (rankerInfo == null)
		{
			return false;
		}
		this.rankerInfo = rankerInfo;
		ClearRankerInfo();
		SetGuildLogoImage(rankerInfo.Logo);
		SetGuildMemberCount(rankingType, rankerInfo.Level, rankerInfo.CurrentMemberCount);
		SetRank(rankerInfo, myServerGroup);
		SetRankerInfo(rankingType, rankerInfo);
		return true;
	}

	public bool SetData(GuildRankingBoardServerTotalRankingInfo rankingInfo)
	{
		if (rankingInfo == null)
		{
			return false;
		}
		ClearRankerInfo();
		SetServerRank(rankingInfo.Rank + 1);
		string serverName = GameRoot.Instance.ServerGroupNameDataContainer.GetServerName(rankingInfo.ServerGroup);
		UIHelper.SetLabelText(Label_Name, serverName);
		UIHelper.SetLabelText(Label_Score, rankingInfo.Score.ToString("#,0"));
		return true;
	}

	private void SetGuildLogoImage(byte guildLogo)
	{
		AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(guildLogo);
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_GuildLogo, guildLogoIcon);
	}

	private void SetGuildMemberCount(GuildRankingType rankingType, int level, int memberCount)
	{
		int num = GameRoot.Instance.GuildLevelDataContainer.GetData(level)?.MaxMember ?? 0;
		if (rankingType == GuildRankingType.World)
		{
			UIHelper.SetLabelText(Label_GlobalMemberCount, memberCount + "/" + num);
		}
		else
		{
			UIHelper.SetLabelText(Label_MemberCount, memberCount + "/" + num);
		}
	}

	private void SetRank(GuildRankingBoardRankerInfo rankerInfo, string myServerGroup)
	{
		bool flag = false;
		if (GlobalAutoBehaviour<GuildManager>.isValidInstance && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info != null && !String_Extension_Method.IsNullOrEmpty(myServerGroup))
		{
			flag = rankerInfo.GuildId == GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Id && rankerInfo.ServerGroup.Equals(myServerGroup);
		}
		int num = rankerInfo.Rank + 1;
		if (num <= 3)
		{
			if (RankSpriteSelector != null)
			{
				RankSpriteSelector.Select(num.ToString());
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: true);
			UIHelper.EnableGameObject(Label_RankeMyGuild, enable: false);
			UIHelper.EnableGameObject(Label_RankOtherGuild, enable: false);
		}
		else
		{
			if (flag)
			{
				UIHelper.SetLabelText(Label_RankeMyGuild, num);
			}
			else
			{
				UIHelper.SetLabelText(Label_RankOtherGuild, num);
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
			UIHelper.EnableGameObject(Label_RankeMyGuild, flag);
			UIHelper.EnableGameObject(Label_RankOtherGuild, !flag);
		}
	}

	private void SetServerRank(int rank)
	{
		UIHelper.EnableGameObject(Label_RankeMyGuild, enable: false);
		if (rank <= 3)
		{
			if (RankSpriteSelector != null)
			{
				RankSpriteSelector.Select(rank.ToString());
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: true);
			UIHelper.EnableGameObject(Label_RankOtherGuild, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
			UIHelper.SetLabelText(Label_RankOtherGuild, rank);
			UIHelper.EnableGameObject(Label_RankOtherGuild, enable: true);
		}
	}

	private void SetRankerInfo(GuildRankingType rankingType, GuildRankingBoardRankerInfo rankerInfo)
	{
		bool flag = CheckAvailableGuild(rankerInfo);
		UIHelper.EnableGameObject(LocalNameRoot, flag && rankingType != GuildRankingType.World);
		UIHelper.EnableGameObject(GlobalNameRoot, flag && rankingType == GuildRankingType.World);
		UIHelper.EnableGameObject(LabelDisapperGuild, !flag);
		UIHelper.EnableGameObject(Label_Server, rankingType == GuildRankingType.World);
		if (rankingType == GuildRankingType.World)
		{
			string serverName = GameRoot.Instance.ServerGroupNameDataContainer.GetServerName(rankerInfo.ServerGroup);
			if (!String_Extension_Method.IsNullOrEmpty(serverName))
			{
				UIHelper.SetLabelText(Label_Server, "[" + serverName + "]");
			}
			UIHelper.SetLabelText(Label_GlobalName, rankerInfo.Name);
			UIHelper.SetLabelText(Label_GlobalLevel, "Lv " + rankerInfo.Level);
		}
		else
		{
			UIHelper.SetLabelText(Label_Name, rankerInfo.Name);
			UIHelper.SetLabelText(Label_Level, "Lv " + rankerInfo.Level);
		}
		UIHelper.SetLabelText(Label_Score, rankerInfo.Score.ToString("#,0"));
		CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(rankerInfo.CountryCode);
		if (dataByCode != null)
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_CountryFlag, plainTexture);
		}
	}

	private bool CheckAvailableGuild(GuildRankingBoardRankerInfo rankerInfo)
	{
		if (rankerInfo.CurrentMemberCount == 0 || String_Extension_Method.IsNullOrEmpty(rankerInfo.Name) || rankerInfo.Logo == 0)
		{
			return false;
		}
		return true;
	}

	private void ClearRankerInfo()
	{
		UIHelper.SetLabelText(Label_MemberCount, string.Empty);
		UIHelper.SetLabelText(Label_GlobalMemberCount, string.Empty);
		UIHelper.SetLabelText(Label_RankOtherGuild, string.Empty);
		UIHelper.SetLabelText(Label_RankeMyGuild, string.Empty);
		UIHelper.SetLabelText(Label_Name, string.Empty);
		UIHelper.SetLabelText(Label_Score, string.Empty);
		UIHelper.SetLabelText(Label_Level, string.Empty);
		UIHelper.SetLabelText(Label_GlobalName, string.Empty);
		UIHelper.SetLabelText(Label_GlobalLevel, string.Empty);
		UIHelper.SetLabelText(Label_Server, string.Empty);
		UIHelper.EnableGameObject(LabelDisapperGuild, enable: false);
		UIHelper.ClearPlaneAssetTexture(Texture_CountryFlag);
	}
}
