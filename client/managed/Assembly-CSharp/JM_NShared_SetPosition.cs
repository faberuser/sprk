using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_SetPosition
{
	public static string EncodeJsonString(SetPosition o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SetPosition o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("Position", JM_NShared_NUtil_Vector3.EncodeJsonObject(o.Position));
		dictionary.Add("MethodType", o.MethodType.ToString());
		dictionary.Add("OperationUid", o.OperationUid.ToString());
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
