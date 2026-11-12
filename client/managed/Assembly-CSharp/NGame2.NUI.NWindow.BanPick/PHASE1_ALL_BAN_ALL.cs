using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow.BanPick;

public class PHASE1_ALL_BAN_ALL : PhaseBase
{
	public PHASE1_ALL_BAN_ALL(MatchBanPick.Player me, MatchBanPick.Player opponent, IListener listener)
		: base(me, opponent, MatchBanPickState.PHASE1_ALL_BAN_ALL, listener)
	{
	}

	public override Tuple<MatchedAccountInfo, Dictionary<int, CachedHeroInfo>, BanPickAccountInfo, BanPickAccountInfo> GetCurrentTurnHeroInfos()
	{
		Tuple<MatchedAccountInfo, Dictionary<int, CachedHeroInfo>, BanPickAccountInfo, BanPickAccountInfo> currentTurnHeroInfos = base.GetCurrentTurnHeroInfos();
		if (GameRoot.Instance.ConstantDataContainer.GetValueBool("MatchingBanPickAllHeroBan"))
		{
			currentTurnHeroInfos.Item2 = CreateAllHeroInfoDictionary();
		}
		return currentTurnHeroInfos;
	}

	private Dictionary<int, CachedHeroInfo> CreateAllHeroInfoDictionary()
	{
		List<CreatureData> listOf = GameRoot.Instance.CreatureDataContainer.GetListOf((CreatureData creatureData) => creatureData.OpenType != CreatureOpenType.None && creatureData.OpenType != CreatureOpenType.CommingSoon);
		Dictionary<int, CachedHeroInfo> cachedHeroInfoList = new Dictionary<int, CachedHeroInfo>();
		listOf.ForEach(delegate(CreatureData data)
		{
			cachedHeroInfoList.Add(data.Index, new CachedHeroInfo(data));
		});
		return cachedHeroInfoList;
	}

	public override int GetDeckSlotIndicateCount()
	{
		return 0;
	}

	public override bool IsHideDeckMine()
	{
		return true;
	}
}
