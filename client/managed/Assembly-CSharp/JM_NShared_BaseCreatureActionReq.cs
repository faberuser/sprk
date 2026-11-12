using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BaseCreatureActionReq
{
	public static BaseCreatureActionReq Parse(string s)
	{
		return Parse(s, new BaseCreatureActionReq());
	}

	public static BaseCreatureActionReq Parse(string s, BaseCreatureActionReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BaseCreatureActionReq Parse(IDictionary d)
	{
		return Parse(d, new BaseCreatureActionReq());
	}

	public static BaseCreatureActionReq Parse(IDictionary d, BaseCreatureActionReq o)
	{
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(BaseCreatureActionReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BaseCreatureActionReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
