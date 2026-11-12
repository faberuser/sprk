using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CampaignMultiAvatarKeyComparer : IKeyComparerT<CampaignMultiAvatarKey>, IKeyComparer, IEqualityComparer<CampaignMultiAvatarKey>
{
	private Type HandleType => typeof(CampaignMultiAvatarKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CampaignMultiAvatarKey>.Equals(CampaignMultiAvatarKey x, CampaignMultiAvatarKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<CampaignMultiAvatarKey>.GetHashCode(CampaignMultiAvatarKey obj)
	{
		return obj.GetHashCode();
	}
}
