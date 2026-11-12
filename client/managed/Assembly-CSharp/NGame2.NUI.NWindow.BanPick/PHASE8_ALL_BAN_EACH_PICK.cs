using System.Collections.Generic;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow.BanPick;

public class PHASE8_ALL_BAN_EACH_PICK : PhaseBase
{
	public PHASE8_ALL_BAN_EACH_PICK(MatchBanPick.Player me, MatchBanPick.Player opponent, IListener listener)
		: base(me, opponent, MatchBanPickState.PHASE8_ALL_BAN_EACH_PICK, listener)
	{
	}

	public override int GetDeckSlotIndicateCount()
	{
		return 5;
	}

	public override bool IsHidePickHeroList()
	{
		return true;
	}

	public override bool IsHideDeckMine()
	{
		return true;
	}

	public override bool IsHideDeckOpponent()
	{
		return false;
	}

	public override Tuple<MatchedAccountInfo, Dictionary<int, CachedHeroInfo>, BanPickAccountInfo, BanPickAccountInfo> GetCurrentTurnHeroInfos()
	{
		Tuple<MatchedAccountInfo, Dictionary<int, CachedHeroInfo>, BanPickAccountInfo, BanPickAccountInfo> currentTurnHeroInfos = base.GetCurrentTurnHeroInfos();
		currentTurnHeroInfos.Item2 = null;
		return currentTurnHeroInfos;
	}

	public override void RandomBanPick()
	{
		List<int> list = new List<int>(_listener.GetCurrentSelectedHeroList());
		while (list.Count < GetSelectableCount())
		{
			int index = Random.Range(0, __Extension_Method_ICollection__.COUNT(_opponent.DeckSlotList));
			int heroIndex = _opponent.DeckSlotList[index].HeroInfo.HeroIndex;
			if (list.Count <= 0 || !list.Contains(heroIndex))
			{
				list.Add(heroIndex);
			}
		}
		_listener.RandomPick(base.MatchBanPickState, list.ToArray(), IsPickMode());
	}
}
