using System.Collections.Generic;

namespace NShared.NJit.NResolver;

public interface IKeyComparerT<T> : IKeyComparer, IEqualityComparer<T>
{
}
