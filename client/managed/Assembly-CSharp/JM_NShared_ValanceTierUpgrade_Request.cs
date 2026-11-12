using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ValanceTierUpgrade;

public static class JM_NShared_ValanceTierUpgrade_Request
{
	public static NShared.ValanceTierUpgrade.Request Parse(string s)
	{
		return Parse(s, new NShared.ValanceTierUpgrade.Request());
	}

	public static NShared.ValanceTierUpgrade.Request Parse(string s, NShared.ValanceTierUpgrade.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ValanceTierUpgrade.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ValanceTierUpgrade.Request());
	}

	public static NShared.ValanceTierUpgrade.Request Parse(IDictionary d, NShared.ValanceTierUpgrade.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ValanceTierUpgrade.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ValanceTierUpgrade.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
