using NGame2.NContentsDefine;
using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ContentsNoticePopupRankerComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/ContentsNotice/ContentsNoticePopupRankerComponent";

	public static PrefabAuto<ContentsNoticePopupRankerComponent> PrefabAuto = new PrefabAuto<ContentsNoticePopupRankerComponent>("Prefab2/Component/ContentsNotice/ContentsNoticePopupRankerComponent");

	public UITexture TextureHeroPortrait;

	public UITexture TextureCountryFlag;

	public UISpriteSelector RankSpriteSelector;

	public UILabel LabelPoint;

	public UILabel LabelServer;

	public UILabel LabelGlobalName;

	public UISprite SpriteGuildIconBg;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public bool SetData(ContentsNoticeType contentsType, ContentsNoticePopupRankerInfo rankerInfo)
	{
		Clear();
		if (rankerInfo == null)
		{
			return false;
		}
		SetPortraitImage(contentsType, rankerInfo.AvatarHeroIndex);
		SetRank(VisualTierRank.CreateFromServer(rankerInfo.Rank));
		SetRegionInfo(rankerInfo.ServerGroup, rankerInfo.CountryCode);
		UIHelper.SetLabelText(LabelGlobalName, rankerInfo.Nick);
		UIHelper.SetLabelText(LabelPoint, rankerInfo.Score.ToString("#,0"));
		return true;
	}

	private void SetPortraitImage(ContentsNoticeType contentsType, int portraitIndex)
	{
		AssetTexture assetTex = null;
		switch (contentsType)
		{
		case ContentsNoticeType.WorldBoss:
		case ContentsNoticeType.BanPick:
		{
			string iconPath = UIAvatarHelper.GetIconPath(portraitIndex);
			if (String_Extension_Method.IsNullOrEmpty(iconPath))
			{
				return;
			}
			assetTex = UIHelper.GetPlainTexture(iconPath);
			UIHelper.EnableGameObject(SpriteGuildIconBg, enable: false);
			break;
		}
		case ContentsNoticeType.GuildSuppress:
			assetTex = UIHelper.GetGuildLogoIcon2(portraitIndex);
			UIHelper.EnableGameObject(SpriteGuildIconBg, enable: true);
			break;
		}
		UIHelper.ResetPlaneAssetTextureAsync(this, TextureHeroPortrait, assetTex);
	}

	private void SetRank(VisualTierRank visualTierRank)
	{
		if (RankSpriteSelector != null)
		{
			RankSpriteSelector.Select(visualTierRank.Value.ToString());
		}
	}

	private void SetRegionInfo(string serverGroup, string countryCode)
	{
		string serverName = GameRoot.Instance.ServerGroupNameDataContainer.GetServerName(serverGroup);
		if (!String_Extension_Method.IsNullOrEmpty(serverName))
		{
			UIHelper.SetLabelText(LabelServer, "[" + serverName + "]");
		}
		else
		{
			UIHelper.SetLabelText(LabelServer, string.Empty);
		}
		CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
		if (dataByCode != null)
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, TextureCountryFlag, plainTexture);
		}
	}

	private void Clear()
	{
		UIHelper.SetLabelText(LabelGlobalName, string.Empty);
		UIHelper.SetLabelText(LabelServer, string.Empty);
		UIHelper.SetLabelText(LabelPoint, string.Empty);
		UIHelper.ClearPlaneAssetTexture(TextureCountryFlag);
		UIHelper.ClearPlaneAssetTexture(TextureHeroPortrait);
	}
}
