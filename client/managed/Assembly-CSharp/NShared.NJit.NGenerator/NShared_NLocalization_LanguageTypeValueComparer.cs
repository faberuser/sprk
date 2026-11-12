using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;
using NShared.NLocalization;

namespace NShared.NJit.NGenerator;

public class NShared_NLocalization_LanguageTypeValueComparer : IKeyComparerT<LanguageType>, IKeyComparer, IEqualityComparer<LanguageType>
{
	private Type HandleType => typeof(LanguageType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<LanguageType>.Equals(LanguageType x, LanguageType y)
	{
		return x == y;
	}

	int IEqualityComparer<LanguageType>.GetHashCode(LanguageType obj)
	{
		return (int)obj;
	}
}
