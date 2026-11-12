using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_KarmaCardSuffle
{
	public static string EncodeJsonString(KarmaCardSuffle o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(KarmaCardSuffle o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SuffledCardIndexes", BaseJsonMarshaler.EncodeArray(o.SuffledCardIndexes));
		dictionary.Add("GlobalTimestamp", o.GlobalTimestamp.ToString());
		return dictionary;
	}
}
