using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UseNickChangeItem;

public static class JM_NShared_UseNickChangeItem_Request
{
	public static NShared.UseNickChangeItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UseNickChangeItem.Request());
	}

	public static NShared.UseNickChangeItem.Request Parse(string s, NShared.UseNickChangeItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UseNickChangeItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UseNickChangeItem.Request());
	}

	public static NShared.UseNickChangeItem.Request Parse(IDictionary d, NShared.UseNickChangeItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UseNickChangeItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UseNickChangeItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
