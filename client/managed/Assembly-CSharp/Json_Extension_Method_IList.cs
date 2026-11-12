using System;
using System.Collections.Generic;
using LitJson;
using NVespa;

public static class Json_Extension_Method_IList
{
	public static bool LoadFromJsonTuple<T1, T2>(this IList<Tuple<T1, T2>> This, JsonData json, bool clear = true)
	{
		return LoadFromJsonInternal(This, json, clear, new TupleLoadFunctor<T1, T2>());
	}

	public static bool LoadFromJsonTuple<T1, T2, T3>(this IList<Tuple<T1, T2, T3>> This, JsonData json, bool clear = true)
	{
		return LoadFromJsonInternal(This, json, clear, new TupleLoadFunctor<T1, T2, T3>());
	}

	public static bool LoadFromJsonTuple<T1, T2, T3, T4>(this IList<Tuple<T1, T2, T3, T4>> This, JsonData json, bool clear = true)
	{
		return LoadFromJsonInternal(This, json, clear, new TupleLoadFunctor<T1, T2, T3, T4>());
	}

	public static bool LoadFromJsonTuple<T1, T2, T3, T4, T5>(this IList<Tuple<T1, T2, T3, T4, T5>> This, JsonData json, bool clear = true)
	{
		return LoadFromJsonInternal(This, json, clear, new TupleLoadFunctor<T1, T2, T3, T4, T5>());
	}

	public static JsonData MakeJsonTuple<T1, T2>(this IList<Tuple<T1, T2>> This)
	{
		return This.MakeJsonInternal(new TupleMakeFunctor<T1, T2>());
	}

	public static JsonData MakeJsonTuple<T1, T2, T3>(this IList<Tuple<T1, T2, T3>> This)
	{
		return This.MakeJsonInternal(new TupleMakeFunctor<T1, T2, T3>());
	}

	public static JsonData MakeJsonTuple<T1, T2, T3, T4>(this IList<Tuple<T1, T2, T3, T4>> This)
	{
		return This.MakeJsonInternal(new TupleMakeFunctor<T1, T2, T3, T4>());
	}

	public static JsonData MakeJsonTuple<T1, T2, T3, T4, T5>(this IList<Tuple<T1, T2, T3, T4, T5>> This)
	{
		return This.MakeJsonInternal(new TupleMakeFunctor<T1, T2, T3, T4, T5>());
	}

	public static bool LoadFromJsonEx<T>(this IList<T> This, JsonData json, bool clear = true)
	{
		return LoadFromJsonInternal(This, json, clear, SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<T>());
	}

	public static bool LoadFromJson<T>(this IList<T> This, JsonData json, bool clear = true) where T : new()
	{
		return LoadFromJsonInternal(This, json, clear, SingletonAuto<FunctorContainer>.instance.GetLoadFunctor<T>());
	}

	private static bool LoadFromJsonInternal<T>(IList<T> list, JsonData json, bool clear, ILoadFunctor<T> functor)
	{
		if (list == null)
		{
			return false;
		}
		if (!json.IsArray())
		{
			return false;
		}
		if (functor == null)
		{
			return false;
		}
		if (list.IsReadOnly)
		{
			if (!(list is T[] array))
			{
				VespaDebug.LogError("Fixed Size IList Cannot Load From Json: " + list.GetType().FullName);
				return false;
			}
			return LoadFromJsonArray(array, json, clear, functor);
		}
		if (clear)
		{
			list.Clear();
		}
		for (int i = 0; i < json.Count; i++)
		{
			T item = functor.CreateFromJson(json.GetFromArray(i));
			list.Add(item);
		}
		return true;
	}

	private static bool LoadFromJsonArray<T>(T[] array, JsonData json, bool clear, ILoadFunctor<T> functor)
	{
		if (!json.IsArray())
		{
			return false;
		}
		for (int i = 0; i < json.Count && i < array.Length; i++)
		{
			T val = functor.CreateFromJson(json.GetFromArray(i));
			array[i] = val;
		}
		if (clear)
		{
			for (int j = json.Count; j < array.Length; j++)
			{
				array[j] = default(T);
			}
		}
		return true;
	}

	public static bool LoadFromJson<T>(this IList<T> This, JsonData json, Func<T> createFunc, bool clear = true) where T : ILoadJson
	{
		if (clear)
		{
			This.Clear();
		}
		if (!json.IsArray())
		{
			return false;
		}
		for (int i = 0; i < json.Count; i++)
		{
			T val = Vespa_System_Action_Extension_Method.fire(createFunc);
			if (val != null && val.Load(json.GetFromArray(i)))
			{
				This.Add(val);
			}
		}
		return true;
	}

	public static JsonData MakeJson<T>(this IList<T> This)
	{
		IMakeFunctor<T> makeFunctor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<T>();
		return This.MakeJsonInternal(makeFunctor);
	}

	private static JsonData MakeJsonInternal<T>(this IList<T> This, IMakeFunctor<T> functor)
	{
		JsonData jsonData = new JsonData();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(This))
		{
			return jsonData;
		}
		if (functor == null)
		{
			return jsonData;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(This); i++)
		{
			T item = This[i];
			JsonData jsonData2 = functor.MakeJson(item);
			if (jsonData2 != null)
			{
				jsonData.Add(jsonData2);
			}
		}
		return jsonData;
	}
}
