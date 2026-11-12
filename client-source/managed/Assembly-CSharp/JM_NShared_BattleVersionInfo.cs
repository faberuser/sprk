using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleVersionInfo
{
	public static BattleVersionInfo Parse(string s)
	{
		return Parse(s, new BattleVersionInfo());
	}

	public static BattleVersionInfo Parse(string s, BattleVersionInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleVersionInfo Parse(IDictionary d)
	{
		return Parse(d, new BattleVersionInfo());
	}

	public static BattleVersionInfo Parse(IDictionary d, BattleVersionInfo o)
	{
		o.Checksum = BaseJsonMarshaler.ParseValueString(d, "Checksum");
		o.Length = BaseJsonMarshaler.ParseValueInt(d, "Length");
		o.Count = BaseJsonMarshaler.ParseValueInt(d, "Count");
		return o;
	}

	public static string EncodeJsonString(BattleVersionInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleVersionInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Checksum != null)
		{
			dictionary.Add("Checksum", o.Checksum);
		}
		dictionary.Add("Length", o.Length.ToString());
		dictionary.Add("Count", o.Count.ToString());
		return dictionary;
	}
}
