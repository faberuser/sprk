using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CampaignTacticKeyComparer : IKeyComparerT<CampaignTacticKey>, IKeyComparer, IEqualityComparer<CampaignTacticKey>
{
	private Type HandleType => typeof(CampaignTacticKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CampaignTacticKey>.Equals(CampaignTacticKey x, CampaignTacticKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<CampaignTacticKey>.GetHashCode(CampaignTacticKey obj)
	{
		return obj.GetHashCode();
	}
}
