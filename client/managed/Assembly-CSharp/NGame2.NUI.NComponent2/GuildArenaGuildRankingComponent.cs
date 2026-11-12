using NGame2.NGuild;
using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class GuildArenaGuildRankingComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/GuildArena/GuildArenaGuildRankingComponent";

	public static PrefabAuto<GuildArenaGuildRankingComponent> PrefabAuto = new PrefabAuto<GuildArenaGuildRankingComponent>("Prefab2/Component/GuildArena/GuildArenaGuildRankingComponent");

	public UILabel Label_GuildName;

	public UILabel Label_Point;

	public UILabel Label_ServerGroup;

	public UILabel Label_MemberCount;

	public UILabel RankLabel_Other;

	public UILabel RankLabel_Me;

	public UISpriteSelector RankSpriteSelector;

	public UITexture Texture_GuildLogo;

	public UITexture Texture_CountryFlag;

	private GuildArenaRankerInfo _rankerInfo;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		Clear();
	}

	public void Open(GuildArenaRankerInfo rankerInfo)
	{
		_rankerInfo = rankerInfo;
		InitLabels();
		InitRank();
		InitGuildLogo();
		InitCountryFlag();
	}

	public void InitLabels()
	{
		if (_rankerInfo != null)
		{
			string text = _rankerInfo.Name;
			string serverGroup = _rankerInfo.ServerGroup;
			int seasonScore = _rankerInfo.SeasonScore;
			UIHelper.SetLabelText(Label_GuildName, text);
			UIHelper.SetLabelText(Label_Point, seasonScore);
			UIHelper.SetLabelText(Label_ServerGroup, $"[{serverGroup}]");
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxRecvJoinGuildRequest");
			UIHelper.SetLabelText(Label_MemberCount, $"({_rankerInfo.MemberCount}/{valueInt})");
		}
	}

	private void InitRank()
	{
		if (_rankerInfo == null)
		{
			return;
		}
		bool flag = _rankerInfo.GuildId == GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Id;
		int rank = _rankerInfo.Rank;
		if (rank == -1)
		{
			if (flag)
			{
				UIHelper.SetLabelText(RankLabel_Me, "-");
			}
			else
			{
				UIHelper.SetLabelText(RankLabel_Other, "-");
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
			UIHelper.EnableGameObject(RankLabel_Me, flag);
			UIHelper.EnableGameObject(RankLabel_Other, !flag);
		}
		else if (rank <= 3)
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
			if (flag)
			{
				UIHelper.SetLabelText(RankLabel_Me, rank);
			}
			else
			{
				UIHelper.SetLabelText(RankLabel_Other, rank);
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
			UIHelper.EnableGameObject(RankLabel_Me, flag);
			UIHelper.EnableGameObject(RankLabel_Other, !flag);
		}
	}

	private void InitGuildLogo()
	{
		if (_rankerInfo != null)
		{
			AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2((_rankerInfo.Logo <= 0) ? 1 : _rankerInfo.Logo);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_GuildLogo, guildLogoIcon);
		}
	}

	private void InitCountryFlag()
	{
		if (_rankerInfo != null)
		{
			string countryCode = _rankerInfo.CountryCode;
			CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
			if (dataByCode != null)
			{
				AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
				UIHelper.ResetPlaneAssetTextureAsync(this, Texture_CountryFlag, plainTexture);
			}
		}
	}

	private void Clear()
	{
		UIHelper.ClearPlaneAssetTexture(Texture_GuildLogo);
		UIHelper.ClearPlaneAssetTexture(Texture_CountryFlag);
	}
}
