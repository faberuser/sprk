using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow.BanPick;

public class PHASE9_ALL_WAIT_SETTING : PhaseBase
{
	public PHASE9_ALL_WAIT_SETTING(MatchBanPick.Player me, MatchBanPick.Player opponent, IListener listener)
		: base(me, opponent, MatchBanPickState.PHASE9_ALL_WAIT_SETTING, listener)
	{
	}

	public override Tuple<MatchedAccountInfo, Dictionary<int, CachedHeroInfo>, BanPickAccountInfo, BanPickAccountInfo> GetCurrentTurnHeroInfos()
	{
		Tuple<MatchedAccountInfo, Dictionary<int, CachedHeroInfo>, BanPickAccountInfo, BanPickAccountInfo> currentTurnHeroInfos = base.GetCurrentTurnHeroInfos();
		currentTurnHeroInfos.Item2 = null;
		return currentTurnHeroInfos;
	}

	public override bool IsHidePickHeroList()
	{
		return true;
	}

	public override void RandomBanPick()
	{
	}
}
