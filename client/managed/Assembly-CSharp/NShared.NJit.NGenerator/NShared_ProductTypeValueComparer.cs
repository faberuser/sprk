using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ProductTypeValueComparer : IKeyComparerT<ProductType>, IKeyComparer, IEqualityComparer<ProductType>
{
	private Type HandleType => typeof(ProductType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ProductType>.Equals(ProductType x, ProductType y)
	{
		return x == y;
	}

	int IEqualityComparer<ProductType>.GetHashCode(ProductType obj)
	{
		return (int)obj;
	}
}
