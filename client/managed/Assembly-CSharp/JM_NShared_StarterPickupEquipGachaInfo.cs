using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_StarterPickupEquipGachaInfo
{
	public static StarterPickupEquipGachaInfo Parse(string s)
	{
		return Parse(s, new StarterPickupEquipGachaInfo());
	}

	public static StarterPickupEquipGachaInfo Parse(string s, StarterPickupEquipGachaInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static StarterPickupEquipGachaInfo Parse(IDictionary d)
	{
		return Parse(d, new StarterPickupEquipGachaInfo());
	}

	public static StarterPickupEquipGachaInfo Parse(IDictionary d, StarterPickupEquipGachaInfo o)
	{
		o.GachaIndex = BaseJsonMarshaler.ParseValueInt(d, "GachaIndex");
		o.GachaCount = BaseJsonMarshaler.ParseValueInt(d, "GachaCount");
		o.IsTakeLastReward = BaseJsonMarshaler.ParseValueBool(d, "IsTakeLastReward");
		return o;
	}

	public static string EncodeJsonString(StarterPickupEquipGachaInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(StarterPickupEquipGachaInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GachaIndex", o.GachaIndex.ToString());
		dictionary.Add("GachaCount", o.GachaCount.ToString());
		dictionary.Add("IsTakeLastReward", o.IsTakeLastReward.ToString());
		return dictionary;
	}
}
