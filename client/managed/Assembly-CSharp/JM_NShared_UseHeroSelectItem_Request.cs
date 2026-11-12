using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UseHeroSelectItem;

public static class JM_NShared_UseHeroSelectItem_Request
{
	public static NShared.UseHeroSelectItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UseHeroSelectItem.Request());
	}

	public static NShared.UseHeroSelectItem.Request Parse(string s, NShared.UseHeroSelectItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UseHeroSelectItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UseHeroSelectItem.Request());
	}

	public static NShared.UseHeroSelectItem.Request Parse(IDictionary d, NShared.UseHeroSelectItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.GetOtherReward = BaseJsonMarshaler.ParseValueBool(d, "GetOtherReward");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UseHeroSelectItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UseHeroSelectItem.Request o)
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
