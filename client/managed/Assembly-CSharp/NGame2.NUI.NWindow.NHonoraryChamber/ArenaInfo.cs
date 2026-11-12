using System;
using NGame2.NContentsDefine;
using NGame2.NUI.NComponent2;
using NShared;

namespace NGame2.NUI.NWindow.NHonoraryChamber;

[Serializable]
public class ArenaInfo : HonoraryInfoBase
{
	public MyMatchTierIconComponent TierComponent;

	public UILabel LabelScore;

	public UILabel LabelSuccessiveWin;

	public UILabel LabelRecord;

	public UILabel LabelRecordWin;

	public UILabel LabelRecordLose;

	public UILabel LabelWinRate;

	public override void Release()
	{
		base.Release();
	}

	public override bool IsActiveCondition(RecordsOfHonorRankingInfo info)
	{
		MatchArenaType arenaType;
		return MatchArena.TryGetArenaType(info.ContentType, out arenaType);
	}

	public override void SetInfo(RecordsOfHonorRankingInfo info)
	{
		base.SetInfo(info);
		if (!MatchArena.TryGetArenaType(info.ContentType, out var arenaType))
		{
			arenaType = MatchArenaType.Normal;
		}
		TierComponent.Open(MatchTierType.Challenger, 0, VisualTierRank.CreateFromServer(info.Rank), arenaType);
		string locale = GetLocale("HONOR_RANK_SCORE_INFO", Vespa_Long_Extension_Method.ToCommaString(info.Score), info.Rank + 1);
		string text = GetLocale("HONOR_RANK_SCORE") + locale;
		UIHelper.SetLabelText(LabelScore, text);
		string locale2 = GetLocale("HONOR_RANK_REPEATWIN", info.SuccessiveWin);
		UIHelper.SetLabelText(LabelSuccessiveWin, locale2);
		string locale3 = GetLocale("HONOR_RANK_RECORD");
		UIHelper.SetLabelText(LabelRecord, locale3);
		string locale4 = GetLocale("PVP_WINCOUNT", info.Win);
		string locale5 = GetLocale("PVP_LOSECOUNT", info.Lose);
		UIHelper.SetLabelText(LabelRecordWin, locale4);
		UIHelper.SetLabelText(LabelRecordLose, locale5);
		string locale6 = GetLocale("HONOR_RANK_WINNINGRATE", GetWinRate(info.Win, info.Lose));
		UIHelper.SetLabelText(LabelWinRate, locale6);
	}
}
