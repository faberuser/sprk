using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_HeroVoiceDataKeyComparer : IKeyComparerT<HeroVoiceDataKey>, IKeyComparer, IEqualityComparer<HeroVoiceDataKey>
{
	private Type HandleType => typeof(HeroVoiceDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<HeroVoiceDataKey>.Equals(HeroVoiceDataKey x, HeroVoiceDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<HeroVoiceDataKey>.GetHashCode(HeroVoiceDataKey obj)
	{
		return obj.GetHashCode();
	}
}
