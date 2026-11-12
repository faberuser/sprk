using System;
using NGame2.NContentsDefine;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

[Serializable]
public class PvpResultScoreInfo
{
	public UILabelAnimator Label_MatchScore;

	public UILabel Label_MatchScoreDiff;

	public UILabel Label_TierRank;

	public UILabel Label_TierRankDiff;

	public Transform TierIconRoot;

	public Color WinLabelColor = Color.green;

	public Color LoseLabelColor = Color.red;

	private MyMatchTierIconComponent _tierIconComponent;

	public void Open(MatchResultInfo matchResultInfo, MatchArenaType arenaType)
	{
		Open(matchResultInfo.NewMatchScore, matchResultInfo.GainedMatchScore, VisualTierRank.CreateFromServer(matchResultInfo.NewTierRank), matchResultInfo.NewTotalRank - matchResultInfo.PrevTotalRank, matchResultInfo.NewTierIndex, arenaType);
	}

	public void Open(int matchScore, int matchScoreDiff, VisualTierRank visualTierRank, int rankDiff, int tierIndex, MatchArenaType arenaType)
	{
		MatchTierData tierData = MatchArena.GetTierData(arenaType, tierIndex);
		_ = tierData?.Tier;
		int num = matchScore - matchScoreDiff;
		int num2 = -rankDiff;
		if (num2 < 0 && matchScoreDiff > 0)
		{
			num2 = 0;
		}
		UIHelper.SetLabelText(Label_MatchScore, num, matchScore);
		UIHelper.SetLabelText(Label_TierRank, visualTierRank.Value);
		UIHelper.SetLabelText(Label_MatchScoreDiff, ((matchScoreDiff >= 0) ? "(+" : "(") + matchScoreDiff + ")");
		UIHelper.SetLabelText(Label_TierRankDiff, ((num2 >= 0) ? "(+" : "(") + num2 + ")");
		UIHelper.SetLabelColor(Label_MatchScoreDiff, (matchScoreDiff >= 0) ? WinLabelColor : LoseLabelColor);
		UIHelper.SetLabelColor(Label_TierRankDiff, (num2 >= 0) ? WinLabelColor : LoseLabelColor);
		if (tierData != null)
		{
			_tierIconComponent = MyMatchTierIconComponent.PrefabAuto.Factory.Create(TierIconRoot);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_tierIconComponent.transform);
			_tierIconComponent.Open(tierData.Tier, tierData.Level, visualTierRank, arenaType);
		}
	}

	public void Reset()
	{
		FactoryBase.SafeReleaseComponent(ref _tierIconComponent);
	}
}
