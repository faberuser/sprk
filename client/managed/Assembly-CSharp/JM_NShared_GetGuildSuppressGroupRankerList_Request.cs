using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildSuppressGroupRankerList;

public static class JM_NShared_GetGuildSuppressGroupRankerList_Request
{
	public static NShared.GetGuildSuppressGroupRankerList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildSuppressGroupRankerList.Request());
	}

	public static NShared.GetGuildSuppressGroupRankerList.Request Parse(string s, NShared.GetGuildSuppressGroupRankerList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildSuppressGroupRankerList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildSuppressGroupRankerList.Request());
	}

	public static NShared.GetGuildSuppressGroupRankerList.Request Parse(IDictionary d, NShared.GetGuildSuppressGroupRankerList.Request o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildSuppressGroupRankerList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildSuppressGroupRankerList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
