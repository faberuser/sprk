using System.Collections.Generic;
using LitJson;
using NVespa;

public static class Json_Extension_Method_IncreaseDic
{
	public static bool LoadFromJson<TKey, TValue>(this IncreaseDic<TKey, TValue> This, JsonData json, bool clear = true) where TKey : new() where TValue : new()
	{
		ILoadFunctor<TKey> loadFunctor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctor<TKey>();
		ILoadFunctor<TValue> loadFunctor2 = SingletonAuto<FunctorContainer>.instance.GetLoadFunctor<TValue>();
		return LoadFromJsonInternal(This, json, clear, loadFunctor, loadFunctor2);
	}

	public static bool LoadFromJsonK<TKey, TValue>(this IncreaseDic<TKey, TValue> This, JsonData json, bool clear = true) where TValue : new()
	{
		ILoadFunctor<TKey> loadFunctorIfExist = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<TKey>();
		ILoadFunctor<TValue> loadFunctor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctor<TValue>();
		return LoadFromJsonInternal(This, json, clear, loadFunctorIfExist, loadFunctor);
	}

	public static bool LoadFromJsonV<TKey, TValue>(this IncreaseDic<TKey, TValue> This, JsonData json, bool clear = true) where TKey : new()
	{
		ILoadFunctor<TKey> loadFunctor = SingletonAuto<FunctorContainer>.instance.GetLoadFunctor<TKey>();
		ILoadFunctor<TValue> loadFunctorIfExist = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<TValue>();
		return LoadFromJsonInternal(This, json, clear, loadFunctor, loadFunctorIfExist);
	}

	public static bool LoadFromJsonKV<TKey, TValue>(this IncreaseDic<TKey, TValue> This, JsonData json, bool clear = true)
	{
		ILoadFunctor<TKey> loadFunctorIfExist = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<TKey>();
		ILoadFunctor<TValue> loadFunctorIfExist2 = SingletonAuto<FunctorContainer>.instance.GetLoadFunctorIfExist<TValue>();
		return LoadFromJsonInternal(This, json, clear, loadFunctorIfExist, loadFunctorIfExist2);
	}

	private static bool LoadFromJsonInternal<TKey, TValue>(IncreaseDic<TKey, TValue> This, JsonData json, bool clear, ILoadFunctor<TKey> keyFunctor, ILoadFunctor<TValue> valueFunctor)
	{
		if (clear)
		{
			This.Clear();
		}
		if (!json.IsArray())
		{
			return false;
		}
		if (keyFunctor == null)
		{
			return false;
		}
		if (valueFunctor == null)
		{
			return false;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonData fromArray = json.GetFromArray(i);
			TKey key = keyFunctor.CreateFromJson(fromArray["Key"]);
			TValue value = valueFunctor.CreateFromJson(fromArray["Value"]);
			This.Add(key, value);
		}
		return true;
	}

	public static JsonData MakeJson<TKey, TValue>(this IncreaseDic<TKey, TValue> This)
	{
		JsonData jsonData = new JsonData();
		if (This.IsNullOrEmpty())
		{
			return jsonData;
		}
		IMakeFunctor<TKey> makeFunctor = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<TKey>();
		if (makeFunctor == null)
		{
			return jsonData;
		}
		IMakeFunctor<TValue> makeFunctor2 = SingletonAuto<FunctorContainer>.instance.GetMakeFunctor<TValue>();
		if (makeFunctor2 == null)
		{
			return jsonData;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(This); i++)
		{
			KeyValuePair<TKey, TValue> keyValuePairByIndex = This.GetKeyValuePairByIndex(i);
			TKey key = keyValuePairByIndex.Key;
			TValue value = keyValuePairByIndex.Value;
			JsonData jsonData2 = makeFunctor.MakeJson(key);
			if (jsonData2 != null)
			{
				JsonData jsonData3 = makeFunctor2.MakeJson(value);
				if (jsonData3 != null)
				{
					JsonData jsonData4 = new JsonData();
					jsonData4.AddField("Key", jsonData2);
					jsonData4.AddField("Value", jsonData3);
					jsonData.Add(jsonData4);
				}
			}
		}
		return jsonData;
	}
}
