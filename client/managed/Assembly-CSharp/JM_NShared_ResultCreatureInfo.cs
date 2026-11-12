using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ResultCreatureInfo
{
	public static ResultCreatureInfo Parse(string s)
	{
		return Parse(s, new ResultCreatureInfo());
	}

	public static ResultCreatureInfo Parse(string s, ResultCreatureInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ResultCreatureInfo Parse(IDictionary d)
	{
		return Parse(d, new ResultCreatureInfo());
	}

	public static ResultCreatureInfo Parse(IDictionary d, ResultCreatureInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Key = BaseJsonMarshaler.ParseValueString(d, "Key");
		o.Hp = BaseJsonMarshaler.ParseValueLong(d, "Hp");
		o.Mp = BaseJsonMarshaler.ParseValueInt(d, "Mp");
		o.TeamId = BaseJsonMarshaler.ParseValueInt(d, "TeamId");
		o.MaxHp = BaseJsonMarshaler.ParseValueLong(d, "MaxHp");
		o.MaxMp = BaseJsonMarshaler.ParseValueInt(d, "MaxMp");
		o.GivedDamage = BaseJsonMarshaler.ParseValueLong(d, "GivedDamage");
		return o;
	}

	public static string EncodeJsonString(ResultCreatureInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ResultCreatureInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.Key != null)
		{
			dictionary.Add("Key", o.Key);
		}
		dictionary.Add("Hp", o.Hp.ToString());
		dictionary.Add("Mp", o.Mp.ToString());
		dictionary.Add("TeamId", o.TeamId.ToString());
		dictionary.Add("MaxHp", o.MaxHp.ToString());
		dictionary.Add("MaxMp", o.MaxMp.ToString());
		dictionary.Add("GivedDamage", o.GivedDamage.ToString());
		return dictionary;
	}
}
