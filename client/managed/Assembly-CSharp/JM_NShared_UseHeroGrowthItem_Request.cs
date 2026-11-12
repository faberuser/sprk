using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UseHeroGrowthItem;

public static class JM_NShared_UseHeroGrowthItem_Request
{
	public static NShared.UseHeroGrowthItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UseHeroGrowthItem.Request());
	}

	public static NShared.UseHeroGrowthItem.Request Parse(string s, NShared.UseHeroGrowthItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UseHeroGrowthItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UseHeroGrowthItem.Request());
	}

	public static NShared.UseHeroGrowthItem.Request Parse(IDictionary d, NShared.UseHeroGrowthItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.GetOtherReward = BaseJsonMarshaler.ParseValueBool(d, "GetOtherReward");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UseHeroGrowthItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UseHeroGrowthItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("GetOtherReward", o.GetOtherReward.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
