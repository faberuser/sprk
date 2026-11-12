using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_HelpListDataKeyComparer : IKeyComparerT<HelpListDataKey>, IKeyComparer, IEqualityComparer<HelpListDataKey>
{
	private Type HandleType => typeof(HelpListDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<HelpListDataKey>.Equals(HelpListDataKey x, HelpListDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<HelpListDataKey>.GetHashCode(HelpListDataKey obj)
	{
		return obj.GetHashCode();
	}
}
