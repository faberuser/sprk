using NGame2.NGuild;
using NGameAsset;
using NNguiExtension;
using NShared.GetGuildRaidGuildRanking;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class GuildRaidGuildRankingComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/GuildRaid/GuildRaidGuildRankingComponent";

	public static PrefabAuto<GuildRaidGuildRankingComponent> PrefabAuto = new PrefabAuto<GuildRaidGuildRankingComponent>("Prefab2/Component/GuildRaid/GuildRaidGuildRankingComponent");

	public UISpriteSelector RankSpriteSelector;

	public UITexture GuildMark;

	public UILabel RankLabel_Other;

	public UILabel RankLabel_Me;

	public UILabel NameLabel;

	public UILabel ClearTimeLabel;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
	}

	public void SetRankData(int rank, GuildRankingInfo rankingInfo)
	{
		SetRank(rank, rankingInfo.GuildId == GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId());
		SetMarkImage(rankingInfo.GuildLogo);
		UIHelper.SetLabelText(NameLabel, rankingInfo.GuildName);
		UILabel clearTimeLabel = ClearTimeLabel;
		int clearTime = rankingInfo.ClearTime;
		UIHelper.SetLabelText(clearTimeLabel, UIHelper.GetRemainTimeLabelText(clearTime));
	}

	private void SetRank(int rank, bool isMe = false)
	{
		if (rank <= 3)
		{
			if (RankSpriteSelector != null)
			{
				RankSpriteSelector.Select(rank.ToString());
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: true);
			UIHelper.EnableGameObject(RankLabel_Me, enable: false);
			UIHelper.EnableGameObject(RankLabel_Other, enable: false);
		}
		else
		{
			if (isMe)
			{
				UIHelper.SetLabelText(RankLabel_Me, rank);
			}
			else
			{
				UIHelper.SetLabelText(RankLabel_Other, rank);
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
			UIHelper.EnableGameObject(RankLabel_Me, isMe);
			UIHelper.EnableGameObject(RankLabel_Other, !isMe);
		}
	}

	private void SetMarkImage(int idx_Mark)
	{
		AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(idx_Mark);
		UIHelper.ResetPlaneAssetTextureAsync(this, GuildMark, guildLogoIcon);
	}
}
