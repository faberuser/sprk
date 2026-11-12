using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EclipseRecordInfo
{
	public static EclipseRecordInfo Parse(string s)
	{
		return Parse(s, new EclipseRecordInfo());
	}

	public static EclipseRecordInfo Parse(string s, EclipseRecordInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EclipseRecordInfo Parse(IDictionary d)
	{
		return Parse(d, new EclipseRecordInfo());
	}

	public static EclipseRecordInfo Parse(IDictionary d, EclipseRecordInfo o)
	{
		o.LastWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "LastWaveIndex");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(EclipseRecordInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EclipseRecordInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("LastWaveIndex", o.LastWaveIndex.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
