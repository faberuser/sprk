using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow.BanPick;

public class PHASE10_BATTLE_START : PhaseBase
{
	public PHASE10_BATTLE_START(MatchBanPick.Player me, MatchBanPick.Player opponent, IListener listener)
		: base(me, opponent, MatchBanPickState.PHASE10_BATTLE_START, listener)
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

	public override void OnEndAnimation()
	{
		if (base.MatchBanPickState == MatchBanPickState.PHASE10_BATTLE_START)
		{
			_listener.StartBattle(_recvAccountInfos);
		}
	}
}
