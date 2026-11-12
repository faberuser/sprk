using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_CancelDuel
{
	public static CancelDuel Parse(string s)
	{
		return Parse(s, new CancelDuel());
	}

	public static CancelDuel Parse(string s, CancelDuel o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CancelDuel Parse(IDictionary d)
	{
		return Parse(d, new CancelDuel());
	}

	public static CancelDuel Parse(IDictionary d, CancelDuel o)
	{
		o.GroupId = BaseJsonMarshaler.ParseValueLong(d, "GroupId");
		o.IsReject = BaseJsonMarshaler.ParseValueBool(d, "IsReject");
		return o;
	}

	public static string EncodeJsonString(CancelDuel o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CancelDuel o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GroupId", o.GroupId.ToString());
		dictionary.Add("IsReject", o.IsReject.ToString());
		return dictionary;
	}
}
