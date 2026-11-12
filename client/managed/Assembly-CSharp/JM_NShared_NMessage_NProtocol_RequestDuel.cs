using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_RequestDuel
{
	public static RequestDuel Parse(string s)
	{
		return Parse(s, new RequestDuel());
	}

	public static RequestDuel Parse(string s, RequestDuel o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RequestDuel Parse(IDictionary d)
	{
		return Parse(d, new RequestDuel());
	}

	public static RequestDuel Parse(IDictionary d, RequestDuel o)
	{
		o.GroupId = BaseJsonMarshaler.ParseValueLong(d, "GroupId");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.TeamLevel = BaseJsonMarshaler.ParseValueInt(d, "TeamLevel");
		o.AvatarIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarIndex");
		return o;
	}

	public static string EncodeJsonString(RequestDuel o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RequestDuel o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GroupId", o.GroupId.ToString());
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		dictionary.Add("TeamLevel", o.TeamLevel.ToString());
		dictionary.Add("AvatarIndex", o.AvatarIndex.ToString());
		return dictionary;
	}
}
