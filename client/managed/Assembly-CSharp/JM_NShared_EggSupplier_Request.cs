using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.EggSupplier;

public static class JM_NShared_EggSupplier_Request
{
	public static NShared.EggSupplier.Request Parse(string s)
	{
		return Parse(s, new NShared.EggSupplier.Request());
	}

	public static NShared.EggSupplier.Request Parse(string s, NShared.EggSupplier.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EggSupplier.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.EggSupplier.Request());
	}

	public static NShared.EggSupplier.Request Parse(IDictionary d, NShared.EggSupplier.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.EggSupplier.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.EggSupplier.Request o)
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
