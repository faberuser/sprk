using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ChapterDungeonKey2Comparer : IKeyComparerT<ChapterDungeonKey2>, IKeyComparer, IEqualityComparer<ChapterDungeonKey2>
{
	private Type HandleType => typeof(ChapterDungeonKey2);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ChapterDungeonKey2>.Equals(ChapterDungeonKey2 x, ChapterDungeonKey2 y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ChapterDungeonKey2>.GetHashCode(ChapterDungeonKey2 obj)
	{
		return obj.GetHashCode();
	}
}
