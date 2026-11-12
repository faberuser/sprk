using System.Collections.Generic;

namespace NVespa;

internal static class EmptyDicHolder<TKey, TValue>
{
	internal static Dictionary<TKey, TValue> Empty;

	static EmptyDicHolder()
	{
		Empty = new Dictionary<TKey, TValue>();
	}
}
