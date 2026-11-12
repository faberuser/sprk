using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UseGuildNameChangeItem;

public static class JM_NShared_UseGuildNameChangeItem_Request
{
	public static NShared.UseGuildNameChangeItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UseGuildNameChangeItem.Request());
	}

	public static NShared.UseGuildNameChangeItem.Request Parse(string s, NShared.UseGuildNameChangeItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UseGuildNameChangeItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UseGuildNameChangeItem.Request());
	}

	public static NShared.UseGuildNameChangeItem.Request Parse(IDictionary d, NShared.UseGuildNameChangeItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UseGuildNameChangeItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UseGuildNameChangeItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
