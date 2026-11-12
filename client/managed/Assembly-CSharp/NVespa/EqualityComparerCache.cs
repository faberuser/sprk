using System;
using System.Collections.Generic;

namespace NVespa;

public class EqualityComparerCache : SingletonAuto<EqualityComparerCache>
{
	public abstract class HolderBase
	{
	}

	public class Holder<T> : HolderBase, IEqualityComparer<T>
	{
		public Func<T, T, bool> EqualFunc;

		public Func<T, int> HashCodeFunc;

		public bool Equals(T x, T y)
		{
			return EqualFunc(x, y);
		}

		public int GetHashCode(T obj)
		{
			return HashCodeFunc(obj);
		}
	}

	public class UndefinedInfo
	{
		public Type TypeInfo;

		public bool HasDuplicateValue;

		public void Set<T>()
		{
			TypeInfo = typeof(T);
			if (!TypeInfo.IsEnum)
			{
				return;
			}
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(EnumLookup<T>.ValueArray); i++)
			{
				EnumLookup<T>.ValueData valueData = EnumLookup<T>.ValueArray[i];
				for (int j = i + 1; j < __Extension_Method_ICollection__.COUNT(EnumLookup<T>.ValueArray); j++)
				{
					EnumLookup<T>.ValueData valueData2 = EnumLookup<T>.ValueArray[i];
					if (valueData.SystemValue == valueData2.SystemValue)
					{
						HasDuplicateValue = true;
					}
				}
			}
		}
	}

	public Dictionary<Type, HolderBase> Dic = new Dictionary<Type, HolderBase>();

	public Dictionary<Type, UndefinedInfo> UndefinedRequestType = new Dictionary<Type, UndefinedInfo>();

	protected override bool ThreadSafe => true;

	public EqualityComparerCache()
	{
		lock (SingletonAuto<EqualityComparerCache>.LockObj)
		{
			SingletonAuto<InitialConstructor>.CreateInstance();
		}
	}

	public IEqualityComparer<T> Get<T>()
	{
		lock (SingletonAuto<EqualityComparerCache>.LockObj)
		{
			if (!(Vespa_Generic_IDictionary_Extension_Method.GETVALUE(Dic, typeof(T), null) is Holder<T> result))
			{
				return null;
			}
			return result;
		}
	}

	public void Add<T>(Func<T, T, bool> equalFunc, Func<T, int> hashCodeFunc)
	{
		lock (SingletonAuto<EqualityComparerCache>.LockObj)
		{
			if (!Dic.ContainsKey(typeof(T)))
			{
				Holder<T> value = new Holder<T>
				{
					EqualFunc = equalFunc,
					HashCodeFunc = hashCodeFunc
				};
				Dic.Add(typeof(T), value);
			}
		}
	}
}
