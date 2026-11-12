using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UseFlaskItem;

public static class JM_NShared_UseFlaskItem_Request
{
	public static NShared.UseFlaskItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UseFlaskItem.Request());
	}

	public static NShared.UseFlaskItem.Request Parse(string s, NShared.UseFlaskItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UseFlaskItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UseFlaskItem.Request());
	}

	public static NShared.UseFlaskItem.Request Parse(IDictionary d, NShared.UseFlaskItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UseFlaskItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UseFlaskItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
