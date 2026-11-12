using System.Collections.Generic;
using NShared.NJit;

namespace NClientData.NLocalization;

public class LocalizeCache
{
	public struct CacheKey
	{
		public class Comparer : IEqualityComparer<CacheKey>
		{
			public static Comparer Default = new Comparer();

			bool IEqualityComparer<CacheKey>.Equals(CacheKey x, CacheKey y)
			{
				if (x.KeyObject == y.KeyObject)
				{
					return x.KeyName == y.KeyName;
				}
				return false;
			}

			int IEqualityComparer<CacheKey>.GetHashCode(CacheKey obj)
			{
				return obj.HashCode;
			}
		}

		public object KeyObject;

		public string KeyName;

		public int HashCode;

		public CacheKey(object keyObject, string keyName)
		{
			KeyObject = keyObject;
			KeyName = keyName;
			HashCode = JitOption.BuildHash(keyObject.GetHashCode(), KeyName);
		}
	}

	private Dictionary<CacheKey, object> _cache = new Dictionary<CacheKey, object>(CacheKey.Comparer.Default);

	public bool TryGetCache(object keyObject, string keyName, out object value)
	{
		lock (_cache)
		{
			return _cache.TryGetValue(new CacheKey(keyObject, keyName), out value);
		}
	}

	public void AddCache(object keyObject, string keyName, object value)
	{
		lock (_cache)
		{
			_cache[new CacheKey(keyObject, keyName)] = value;
		}
	}
}
