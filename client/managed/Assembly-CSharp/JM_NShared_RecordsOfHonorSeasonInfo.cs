using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RecordsOfHonorSeasonInfo
{
	public static RecordsOfHonorSeasonInfo Parse(string s)
	{
		return Parse(s, new RecordsOfHonorSeasonInfo());
	}

	public static RecordsOfHonorSeasonInfo Parse(string s, RecordsOfHonorSeasonInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RecordsOfHonorSeasonInfo Parse(IDictionary d)
	{
		return Parse(d, new RecordsOfHonorSeasonInfo());
	}

	public static RecordsOfHonorSeasonInfo Parse(IDictionary d, RecordsOfHonorSeasonInfo o)
	{
		o.ContentType = BaseJsonMarshaler.ParseEnum<RecordsOfHonorContentType>(d, "ContentType");
		o.Season = BaseJsonMarshaler.ParseArrayCast(d, "Season", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(RecordsOfHonorSeasonInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RecordsOfHonorSeasonInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ContentType", o.ContentType.ToString());
		dictionary.Add("Season", BaseJsonMarshaler.EncodeArray(o.Season));
		return dictionary;
	}
}
