using NGame2.NAccount;
using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class GuildRaidContributionRankComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		void OnClickComponent(long accountId);
	}

	public const string PrefabPath = "Prefab2/Component/GuildRaid/GuildRaidContributionRankComponent";

	public static PrefabAuto<GuildRaidContributionRankComponent> PrefabAuto = new PrefabAuto<GuildRaidContributionRankComponent>("Prefab2/Component/GuildRaid/GuildRaidContributionRankComponent");

	public UITexture HeroTex;

	public UILabel RankLabel_Other;

	public UILabel RankLabel_Me;

	public UISpriteSelector RankSpriteSelector;

	public UILabel NameLabel;

	public UILabel PointLabel;

	private GuildRaidMemberTotalScoreInfo ScoreInfo;

	private GuildMemberInfo GuildMemberInfo;

	private Listener _Listener;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
	}

	public void SetData(GuildRaidMemberTotalScoreInfo scoreInfo, GuildMemberInfo guildMemberInfo, int rank, Listener listener)
	{
		ScoreInfo = scoreInfo;
		GuildMemberInfo = guildMemberInfo;
		_Listener = listener;
		SetHeroImage(guildMemberInfo);
		SetRank(rank, ScoreInfo.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
		UIHelper.SetLabelText(NameLabel, guildMemberInfo.Nick);
		UIHelper.SetLabelText(PointLabel, scoreInfo.Score.ToString("#,0"));
	}

	public void SetHeroImage(GuildMemberInfo memberInfo)
	{
		string iconPath = UIAvatarHelper.GetIconPath(memberInfo.AvatarHeroIndex);
		if (!String_Extension_Method.IsNullOrEmpty(iconPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(iconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, HeroTex, plainTexture);
		}
	}

	public void SetRank(int rank, bool isMe)
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

	public void OnClickComponent()
	{
		if (_Listener != null)
		{
			_Listener.OnClickComponent(ScoreInfo.AccountId);
		}
	}
}
