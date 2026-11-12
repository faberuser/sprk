using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_SetManualTargetReq
{
	public static SetManualTargetReq Parse(string s)
	{
		return Parse(s, new SetManualTargetReq());
	}

	public static SetManualTargetReq Parse(string s, SetManualTargetReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SetManualTargetReq Parse(IDictionary d)
	{
		return Parse(d, new SetManualTargetReq());
	}

	public static SetManualTargetReq Parse(IDictionary d, SetManualTargetReq o)
	{
		o.TargetCreatureUid = BaseJsonMarshaler.ParseValueInt(d, "TargetCreatureUid");
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(SetManualTargetReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SetManualTargetReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TargetCreatureUid", o.TargetCreatureUid.ToString());
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
