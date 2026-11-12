using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_LanguagePatternKeyComparer : IKeyComparerT<LanguagePatternKey>, IKeyComparer, IEqualityComparer<LanguagePatternKey>
{
	private Type HandleType => typeof(LanguagePatternKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<LanguagePatternKey>.Equals(LanguagePatternKey x, LanguagePatternKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<LanguagePatternKey>.GetHashCode(LanguagePatternKey obj)
	{
		return obj.GetHashCode();
	}
}
