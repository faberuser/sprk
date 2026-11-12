using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PtsEntryCondition
{
	public static PtsEntryCondition Parse(string s)
	{
		return Parse(s, new PtsEntryCondition());
	}

	public static PtsEntryCondition Parse(string s, PtsEntryCondition o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PtsEntryCondition Parse(IDictionary d)
	{
		return Parse(d, new PtsEntryCondition());
	}

	public static PtsEntryCondition Parse(IDictionary d, PtsEntryCondition o)
	{
		o.CCULimit = BaseJsonMarshaler.ParseValueString(d, "CCULimit");
		o.TeamLevelLocalization = BaseJsonMarshaler.ParseValueString(d, "TeamLevelLocalization");
		o.TeamLevelValue = BaseJsonMarshaler.ParseValueInt(d, "TeamLevelValue");
		return o;
	}

	public static string EncodeJsonString(PtsEntryCondition o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PtsEntryCondition o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.CCULimit != null)
		{
			dictionary.Add("CCULimit", o.CCULimit);
		}
		if (o.TeamLevelLocalization != null)
		{
			dictionary.Add("TeamLevelLocalization", o.TeamLevelLocalization);
		}
		dictionary.Add("TeamLevelValue", o.TeamLevelValue.ToString());
		return dictionary;
	}
}
