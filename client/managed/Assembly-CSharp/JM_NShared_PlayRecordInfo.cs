using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayRecordInfo
{
	public static PlayRecordInfo Parse(string s)
	{
		return Parse(s, new PlayRecordInfo());
	}

	public static PlayRecordInfo Parse(string s, PlayRecordInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayRecordInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayRecordInfo());
	}

	public static PlayRecordInfo Parse(IDictionary d, PlayRecordInfo o)
	{
		o.RecordIndex = BaseJsonMarshaler.ParseValueInt(d, "RecordIndex");
		o.RecordValue = BaseJsonMarshaler.ParseValueString(d, "RecordValue");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(PlayRecordInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayRecordInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RecordIndex", o.RecordIndex.ToString());
		if (o.RecordValue != null)
		{
			dictionary.Add("RecordValue", o.RecordValue);
		}
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
