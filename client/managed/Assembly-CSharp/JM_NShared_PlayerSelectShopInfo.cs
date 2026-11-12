using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerSelectShopInfo
{
	public static PlayerSelectShopInfo Parse(string s)
	{
		return Parse(s, new PlayerSelectShopInfo());
	}

	public static PlayerSelectShopInfo Parse(string s, PlayerSelectShopInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerSelectShopInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerSelectShopInfo());
	}

	public static PlayerSelectShopInfo Parse(IDictionary d, PlayerSelectShopInfo o)
	{
		o.CategoryGroup = BaseJsonMarshaler.ParseValueString(d, "CategoryGroup");
		o.GachaSelectIndex = BaseJsonMarshaler.ParseValueInt(d, "GachaSelectIndex");
		o.ResetCount = BaseJsonMarshaler.ParseValueInt(d, "ResetCount");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.RewardInfo = BaseJsonMarshaler.ParseValueString(d, "RewardInfo");
		return o;
	}

	public static string EncodeJsonString(PlayerSelectShopInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerSelectShopInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.CategoryGroup != null)
		{
			dictionary.Add("CategoryGroup", o.CategoryGroup);
		}
		dictionary.Add("GachaSelectIndex", o.GachaSelectIndex.ToString());
		dictionary.Add("ResetCount", o.ResetCount.ToString());
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		if (o.RewardInfo != null)
		{
			dictionary.Add("RewardInfo", o.RewardInfo);
		}
		return dictionary;
	}
}
