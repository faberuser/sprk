using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SignOut;

public static class JM_NShared_SignOut_Request
{
	public static NShared.SignOut.Request Parse(string s)
	{
		return Parse(s, new NShared.SignOut.Request());
	}

	public static NShared.SignOut.Request Parse(string s, NShared.SignOut.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SignOut.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SignOut.Request());
	}

	public static NShared.SignOut.Request Parse(IDictionary d, NShared.SignOut.Request o)
	{
		o.ForceSignOut = BaseJsonMarshaler.ParseValueBool(d, "ForceSignOut");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SignOut.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SignOut.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ForceSignOut", o.ForceSignOut.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
