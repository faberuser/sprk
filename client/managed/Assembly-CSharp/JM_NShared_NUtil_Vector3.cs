using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NUtil;

public static class JM_NShared_NUtil_Vector3
{
	public static Vector3 Parse(string s)
	{
		return Parse(Json.Deserialize(s) as IDictionary);
	}

	public static Vector3 Parse(IDictionary d)
	{
		return new Vector3
		{
			Mx = BaseJsonMarshaler.ParseValueLong(d, "Mx"),
			My = BaseJsonMarshaler.ParseValueLong(d, "My"),
			Mz = BaseJsonMarshaler.ParseValueLong(d, "Mz")
		};
	}

	public static string EncodeJsonString(Vector3 o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(Vector3 o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("Mx", o.Mx.ToString());
		dictionary.Add("My", o.My.ToString());
		dictionary.Add("Mz", o.Mz.ToString());
		return dictionary;
	}
}
