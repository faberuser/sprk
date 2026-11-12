using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_SetManualMoveReq
{
	public static SetManualMoveReq Parse(string s)
	{
		return Parse(s, new SetManualMoveReq());
	}

	public static SetManualMoveReq Parse(string s, SetManualMoveReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SetManualMoveReq Parse(IDictionary d)
	{
		return Parse(d, new SetManualMoveReq());
	}

	public static SetManualMoveReq Parse(IDictionary d, SetManualMoveReq o)
	{
		o.Position = BaseJsonMarshaler.ParseObject(d, "Position", JM_NShared_NUtil_Vector3.Parse);
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(SetManualMoveReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SetManualMoveReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Position", JM_NShared_NUtil_Vector3.EncodeJsonObject(o.Position));
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
