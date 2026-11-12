using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BuyCostume;

public static class JM_NShared_BuyCostume_Request
{
	public static NShared.BuyCostume.Request Parse(string s)
	{
		return Parse(s, new NShared.BuyCostume.Request());
	}

	public static NShared.BuyCostume.Request Parse(string s, NShared.BuyCostume.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BuyCostume.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BuyCostume.Request());
	}

	public static NShared.BuyCostume.Request Parse(IDictionary d, NShared.BuyCostume.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.CostumeIndex = BaseJsonMarshaler.ParseValueInt(d, "CostumeIndex");
		o.BuyGem = BaseJsonMarshaler.ParseValueInt(d, "BuyGem");
		o.BuyGold = BaseJsonMarshaler.ParseValueInt(d, "BuyGold");
		o.BuyMileage = BaseJsonMarshaler.ParseValueInt(d, "BuyMileage");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BuyCostume.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BuyCostume.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("CostumeIndex", o.CostumeIndex.ToString());
		dictionary.Add("BuyGem", o.BuyGem.ToString());
		dictionary.Add("BuyGold", o.BuyGold.ToString());
		dictionary.Add("BuyMileage", o.BuyMileage.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
