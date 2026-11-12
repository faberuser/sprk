using System;
using NShared;

namespace NGame2.NUI.NWindow.NHonoraryChamber;

[Serializable]
public class GuildSuppressInfo : HonoraryInfoBase
{
	public UITexture Logo;

	public UILabel LabelRankLabel;

	public UILabel LabelRank;

	public UILabel LabelScoreLabel;

	public UILabel LabelScore;

	public override void Release()
	{
		base.Release();
	}

	public override bool IsActiveCondition(RecordsOfHonorRankingInfo info)
	{
		return info.ContentType == RecordsOfHonorContentType.GuildSuppress;
	}

	public override void SetInfo(RecordsOfHonorRankingInfo info)
	{
		base.SetInfo(info);
		UIHelper.ResetPlaneAssetTextureAsync(TransformRoot, Logo, UIHelper.GetGuildLogoIcon2(info.PictureIndex));
		UIHelper.SetLabelText(LabelRank, GetLocale("HONOR_RANK_NUM", info.Rank + 1));
		UIHelper.SetLabelText(LabelScore, Vespa_Long_Extension_Method.ToCommaString(info.Score));
	}
}
