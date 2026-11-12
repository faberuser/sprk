using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2M_Ping;

public static class JM_NShared_B2M_Ping_IntegrityInfo
{
	public static IntegrityInfo Parse(string s)
	{
		return Parse(s, new IntegrityInfo());
	}

	public static IntegrityInfo Parse(string s, IntegrityInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static IntegrityInfo Parse(IDictionary d)
	{
		return Parse(d, new IntegrityInfo());
	}

	public static IntegrityInfo Parse(IDictionary d, IntegrityInfo o)
	{
		o.Host = BaseJsonMarshaler.ParseValueString(d, "Host");
		o.Info = BaseJsonMarshaler.ParseObject(d, "Info", JM_NShared_B2M_Ping_BattleIntegrityCheckInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(IntegrityInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(IntegrityInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Host != null)
		{
			dictionary.Add("Host", o.Host);
		}
		dictionary.Add("Info", JM_NShared_B2M_Ping_BattleIntegrityCheckInfo.EncodeJsonObject(o.Info));
		return dictionary;
	}
}
