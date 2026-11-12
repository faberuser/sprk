using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CampaignWaveKey2Comparer : IKeyComparerT<CampaignWaveKey2>, IKeyComparer, IEqualityComparer<CampaignWaveKey2>
{
	private Type HandleType => typeof(CampaignWaveKey2);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CampaignWaveKey2>.Equals(CampaignWaveKey2 x, CampaignWaveKey2 y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<CampaignWaveKey2>.GetHashCode(CampaignWaveKey2 obj)
	{
		return obj.GetHashCode();
	}
}
