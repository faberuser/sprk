using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_Replay
{
	public static Replay Parse(string s)
	{
		return Parse(s, new Replay());
	}

	public static Replay Parse(string s, Replay o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static Replay Parse(IDictionary d)
	{
		return Parse(d, new Replay());
	}

	public static Replay Parse(IDictionary d, Replay o)
	{
		o.Uid = BaseJsonMarshaler.ParseValueLong(d, "Uid");
		o.Info = BaseJsonMarshaler.ParseValueString(d, "Info");
		o.Data = BaseJsonMarshaler.ParseValueString(d, "Data");
		return o;
	}

	public static string EncodeJsonString(Replay o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(Replay o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Uid", o.Uid.ToString());
		if (o.Info != null)
		{
			dictionary.Add("Info", o.Info);
		}
		if (o.Data != null)
		{
			dictionary.Add("Data", o.Data);
		}
		return dictionary;
	}
}
