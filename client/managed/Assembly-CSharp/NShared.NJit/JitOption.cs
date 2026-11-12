using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using NGameAsset;
using NShared.NJit.NResolver;
using NVespa;

namespace NShared.NJit;

public static class JitOption
{
	public static int Max_countInHashed_from_JitDictionary;

	public const int MajorPrime = 7199369;

	public static readonly int[] primes = new int[72]
	{
		3, 7, 11, 17, 23, 29, 37, 47, 59, 71,
		89, 107, 131, 163, 197, 239, 293, 353, 431, 521,
		631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371,
		4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023,
		25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363,
		156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403,
		968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559,
		5999471, 7199369
	};

	public static JsonSyncLoader2 JsonLoader = new JsonSyncLoader2();

	public static JitSyncLoader JitLoader = new JitSyncLoader();

	public const bool CheckMD5 = true;

	public static bool IsClient => true;

	public static bool IsUnityEditor => false;

	public static bool LoadJitOnly
	{
		get
		{
			if (!UnityCache.isGamePlaying)
			{
				return false;
			}
			if (Define.LocalAssetLoading.Value)
			{
				return false;
			}
			return true;
		}
	}

	public static Type GetResolverGenericTypeDef(Type genericTypeDef)
	{
		if (genericTypeDef == typeof(List<>))
		{
			return typeof(ListResolver<>);
		}
		if (genericTypeDef == typeof(HashSet<>))
		{
			return typeof(HashSetResolver<>);
		}
		if (genericTypeDef == typeof(Dictionary<, >))
		{
			return typeof(DictionaryResolver<, >);
		}
		if (genericTypeDef == typeof(JitRef<>))
		{
			return typeof(JitRefResolver<>);
		}
		if (genericTypeDef == typeof(JitList<>))
		{
			return typeof(JitListResolver<>);
		}
		if (genericTypeDef == typeof(JitDictionary<, >))
		{
			return typeof(JitDictionaryResolver<, >);
		}
		if (genericTypeDef == typeof(JitDictionaryWithKey<, >))
		{
			return typeof(JitDictionaryWithKeyResolver<, >);
		}
		return null;
	}

	public static int BuildHash<T1>(T1 t1)
	{
		return Proxy<T1>.KeyComparer.GetHashCode(t1);
	}

	public static int BuildHash<T1, T2>(T1 t1, T2 t2)
	{
		int hashCode = Proxy<T1>.KeyComparer.GetHashCode(t1);
		int hashCode2 = Proxy<T2>.KeyComparer.GetHashCode(t2);
		return hashCode * 7199369 + hashCode2;
	}

	public static int BuildHash<T1, T2, T3>(T1 t1, T2 t2, T3 t3)
	{
		int hashCode = Proxy<T1>.KeyComparer.GetHashCode(t1);
		int hashCode2 = Proxy<T2>.KeyComparer.GetHashCode(t2);
		int hashCode3 = Proxy<T3>.KeyComparer.GetHashCode(t3);
		return (hashCode * 7199369 + hashCode2) * 7199369 + hashCode3;
	}

	public static int BuildHash<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4)
	{
		int hashCode = Proxy<T1>.KeyComparer.GetHashCode(t1);
		int hashCode2 = Proxy<T2>.KeyComparer.GetHashCode(t2);
		int hashCode3 = Proxy<T3>.KeyComparer.GetHashCode(t3);
		int hashCode4 = Proxy<T4>.KeyComparer.GetHashCode(t4);
		return ((hashCode * 7199369 + hashCode2) * 7199369 + hashCode3) * 7199369 + hashCode4;
	}

	public static int BuildHash<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		int hashCode = Proxy<T1>.KeyComparer.GetHashCode(t1);
		int hashCode2 = Proxy<T2>.KeyComparer.GetHashCode(t2);
		int hashCode3 = Proxy<T3>.KeyComparer.GetHashCode(t3);
		int hashCode4 = Proxy<T4>.KeyComparer.GetHashCode(t4);
		int hashCode5 = Proxy<T5>.KeyComparer.GetHashCode(t5);
		return (((hashCode * 7199369 + hashCode2) * 7199369 + hashCode3) * 7199369 + hashCode4) * 7199369 + hashCode5;
	}

	public static int BuildHash(int a, int b)
	{
		return a * 7199369 + b;
	}

	public static int BuildHash(int a, int b, int c)
	{
		return (a * 7199369 + b) * 7199369 + c;
	}

	public static int BuildHash(int a, int b, int c, int d)
	{
		return ((a * 7199369 + b) * 7199369 + c) * 7199369 + d;
	}

	public static int BuildHash(int a, int b, int c, int d, int e)
	{
		return (((a * 7199369 + b) * 7199369 + c) * 7199369 + d) * 7199369 + e;
	}

	public static int StringToHashCode(string str)
	{
		int num = 5381;
		int num2 = num;
		int num3 = ((!string.IsNullOrEmpty(str)) ? str.Length : 0);
		int num4 = 0;
		while (num4 < num3)
		{
			int num5 = str[num4++];
			if (num5 == 0)
			{
				break;
			}
			num = ((num << 5) + num) ^ num5;
			if (num4 >= num3)
			{
				break;
			}
			num5 = str[num4++];
			if (num5 == 0)
			{
				break;
			}
			num2 = ((num2 << 5) + num2) ^ num5;
		}
		return num + num2 * 1566083941;
	}

	public static int CharsToHashCode(char[] chars, int len)
	{
		int num = 5381;
		int num2 = num;
		int num3 = 0;
		while (num3 < len)
		{
			int num4 = chars[num3++];
			if (num4 == 0)
			{
				break;
			}
			num = ((num << 5) + num) ^ num4;
			if (num3 >= len)
			{
				break;
			}
			num4 = chars[num3++];
			if (num4 == 0)
			{
				break;
			}
			num2 = ((num2 << 5) + num2) ^ num4;
		}
		return num + num2 * 1566083941;
	}

	public static void OpenErrorPopup(Type type, bool isComparer)
	{
		string msg = ((!isComparer) ? $"{type} Resolver error" : $"{type} Comparer error");
		JitLogError(msg);
	}

	public static byte[] GetMD5(string str)
	{
		using MD5 mD = MD5.Create();
		byte[] bytes = Encoding.UTF8.GetBytes(str);
		return mD.ComputeHash(bytes);
	}

	public static void JitLog(string msg)
	{
		AppendLogToFile("[Verbose]", msg);
	}

	public static void JitLogWarning(string msg)
	{
		AppendLogToFile("[Warning]", msg);
	}

	public static void JitLogError(string msg)
	{
		VespaDebug.LogError(msg);
		AppendLogToFile("[Error]", msg);
	}

	public static void AppendLogToFile(string head, string msg)
	{
	}
}
