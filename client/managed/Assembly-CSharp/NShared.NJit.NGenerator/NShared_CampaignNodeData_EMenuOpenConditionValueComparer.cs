using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CampaignNodeData_EMenuOpenConditionValueComparer : IKeyComparerT<CampaignNodeData.EMenuOpenCondition>, IKeyComparer, IEqualityComparer<CampaignNodeData.EMenuOpenCondition>
{
	private Type HandleType => typeof(CampaignNodeData.EMenuOpenCondition);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CampaignNodeData.EMenuOpenCondition>.Equals(CampaignNodeData.EMenuOpenCondition x, CampaignNodeData.EMenuOpenCondition y)
	{
		return x == y;
	}

	int IEqualityComparer<CampaignNodeData.EMenuOpenCondition>.GetHashCode(CampaignNodeData.EMenuOpenCondition obj)
	{
		return (int)obj;
	}
}
