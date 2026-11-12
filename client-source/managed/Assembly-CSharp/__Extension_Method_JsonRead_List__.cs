using System.Collections.Generic;
using JsonBin;
using NGameAsset.NCreature;

public static class __Extension_Method_JsonRead_List__
{
	public static bool LoadListFromJson(this List<AnimationKeyData> This, JsonRead json)
	{
		return This.internal_LoadListFromJson(json, FunctorContainer.GetUnsafe<AnimationKeyData>());
	}

	public static bool LoadListFromJson(this List<string> This, JsonRead json)
	{
		return This.internal_LoadListFromJson(json, FunctorContainer.GetUnsafe<string>());
	}

	public static bool LoadListFromJson<T>(this List<T> This, JsonRead json) where T : ILoadJson, new()
	{
		return This.internal_LoadListFromJson(json, FunctorContainer.Get<T>());
	}

	private static bool internal_LoadListFromJson<T>(this List<T> This, JsonRead json, IFunctorJsonRead<T> functor)
	{
		if (This == null)
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
		This.Clear();
		for (int i = 0; i < json.Count; i++)
		{
			T item = functor.CreateFrom(json.GetFromArray(i));
			This.Add(item);
		}
		return true;
	}
}
