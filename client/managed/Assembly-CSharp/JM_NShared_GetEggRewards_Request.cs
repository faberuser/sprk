using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetEggRewards;

public static class JM_NShared_GetEggRewards_Request
{
	public static NShared.GetEggRewards.Request Parse(string s)
	{
		return Parse(s, new NShared.GetEggRewards.Request());
	}

	public static NShared.GetEggRewards.Request Parse(string s, NShared.GetEggRewards.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetEggRewards.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetEggRewards.Request());
	}

	public static NShared.GetEggRewards.Request Parse(IDictionary d, NShared.GetEggRewards.Request o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.IncubatorIndex = BaseJsonMarshaler.ParseValueInt(d, "IncubatorIndex");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetEggRewards.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetEggRewards.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		dictionary.Add("IncubatorIndex", o.IncubatorIndex.ToString());
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
