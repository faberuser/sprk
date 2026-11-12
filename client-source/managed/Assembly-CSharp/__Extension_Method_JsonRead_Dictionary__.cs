using System.Collections.Generic;
using JsonBin;

public static class __Extension_Method_JsonRead_Dictionary__
{
	public static bool LoadDictionaryFromJson<TKey, TValue>(this Dictionary<TKey, TValue> This, JsonRead json) where TKey : new() where TValue : new()
	{
		return This.internal_LoadDictionaryFromJson(json, FunctorContainer.Get<TKey>(), FunctorContainer.Get<TValue>());
	}

	private static bool internal_LoadDictionaryFromJson<TKey, TValue>(this Dictionary<TKey, TValue> This, JsonRead json, IFunctorJsonRead<TKey> keyFunctor, IFunctorJsonRead<TValue> valueFunctor)
	{
		if (This == null)
		{
			return false;
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
		This.Clear();
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			TKey key = keyFunctor.CreateFrom(fromArray["Key"]);
			TValue value = valueFunctor.CreateFrom(fromArray["Value"]);
			This.Add(key, value);
		}
		return true;
	}
}
