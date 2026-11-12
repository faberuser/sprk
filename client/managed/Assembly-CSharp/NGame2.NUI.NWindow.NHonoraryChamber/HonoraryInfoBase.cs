using System;
using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow.NHonoraryChamber;

[Serializable]
public abstract class HonoraryInfoBase
{
	protected const string LOCALE_SCORE = "HONOR_RANK_SCORE";

	protected const string LOCALE_SCORE_INFO = "HONOR_RANK_SCORE_INFO";

	protected const string LOCALE_SUCCESSIVE_WIN = "HONOR_RANK_REPEATWIN";

	protected const string LOCALE_RECORD = "HONOR_RANK_RECORD";

	protected const string LOCALE_WIN = "PVP_WINCOUNT";

	protected const string LOCALE_LOSE = "PVP_LOSECOUNT";

	protected const string LOCALE_WINNING_RATE = "HONOR_RANK_WINNINGRATE";

	protected const string LOCALE_RANK = "HONOR_RANK_NUM";

	public Transform TransformRoot;

	public UISpriteSelector SpriteSelectorMedal;

	public UILabel LabelMedalRank;

	public UITexture TextureCountry;

	public UITableHorizontal TableHorizontal;

	public abstract bool IsActiveCondition(RecordsOfHonorRankingInfo info);

	public virtual void SetInfo(RecordsOfHonorRankingInfo info)
	{
		SetRankingMedal(info.Rank);
		if (info.RankingType == RecordsOfHonorRankingType.World)
		{
			UIHelper.EnableGameObject(TextureCountry, enable: true);
			SetCountryTexture(TextureCountry, info.CountryCode);
		}
		else
		{
			UIHelper.EnableGameObject(TextureCountry, enable: false);
		}
		TableHorizontal.repositionNow = true;
	}

	protected string GetLocale(string key, params object[] args)
	{
		return LocalizationManager.GetStringDirect(key, args);
	}

	protected int GetWinRate(int win, int lose)
	{
		int num = win + lose;
		if (num == 0)
		{
			return 0;
		}
		return (int)((float)win / (float)num * 100f);
	}

	protected void SetCountryTexture(UITexture uITexture, string countryCode)
	{
		if (uITexture == null)
		{
			VespaDebug.LogError("uITexture == null");
			return;
		}
		CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
		if (dataByCode != null)
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
			UIHelper.ResetPlaneAssetTextureAsync(TransformRoot, uITexture, plainTexture);
		}
	}

	protected void SetRankingMedal(int rank)
	{
		rank++;
		bool flag = SpriteSelectorMedal.Select(rank.ToString());
		UIHelper.EnableGameObject(SpriteSelectorMedal, flag);
		UIHelper.SetLabelText(LabelMedalRank, rank.ToString());
		UIHelper.EnableGameObject(LabelMedalRank, !flag);
	}

	public virtual void Release()
	{
		UIHelper.ClearPlaneAssetTexture(TextureCountry);
	}
}
