using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_NodeFunctionTypeValueComparer : IKeyComparerT<NodeFunctionType>, IKeyComparer, IEqualityComparer<NodeFunctionType>
{
	private Type HandleType => typeof(NodeFunctionType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<NodeFunctionType>.Equals(NodeFunctionType x, NodeFunctionType y)
	{
		return x == y;
	}

	int IEqualityComparer<NodeFunctionType>.GetHashCode(NodeFunctionType obj)
	{
		return (int)obj;
	}
}
